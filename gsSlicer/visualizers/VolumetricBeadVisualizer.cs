using g3;
using gs.FillTypes;
using gs.utility;
using Sutro.PathWorks.Plugins.API;
using System;
using System.Collections.Generic;

namespace gs
{
    public class VolumetricBeadVisualizer : IVisualizer
    {
        protected Vector3d position;
        protected double feedrate;
        protected double extrusion;
        protected Vector2d dimensions = new Vector2d(0.4, 0.2);
        protected string fillType;
        protected int layerIndex;
        protected int pointCount;

        protected List<PrintVertex> toolpath;
        protected PrintVertex lastVertex;

        public virtual event Action<ToolpathPreviewVertex[], int[], int> OnMeshGenerated;

        public virtual event Action<List<Vector3d>, int> OnLineGenerated;

        public virtual event Action<double, int> OnNewPlane;

        public string Name => "Bead Visualizer";

        public static readonly Dictionary<string, int> FillTypeIntegerId = new Dictionary<string, int>()
        {
            {DefaultFillType.Label, 0},
            {InnerPerimeterFillType.Label, 1},
            {OuterPerimeterFillType.Label, 2},
            {OpenShellCurveFillType.Label, 3},
            {SolidFillType.Label, 4},
            {SparseFillType.Label, 5},
            {SupportFillType.Label, 6},
            {BridgeFillType.Label, 7},
        };

        public Dictionary<int, FillType> FillTypes { get; protected set; } = new Dictionary<int, FillType>()
        {
            {FillTypeIntegerId[DefaultFillType.Label], new FillType("Unknown", new Vector3f(0.5, 0.5, 0.5))},
            {FillTypeIntegerId[InnerPerimeterFillType.Label], new FillType("Inner Perimeter", new Vector3f(1, 0, 0))},
            {FillTypeIntegerId[OuterPerimeterFillType.Label], new FillType("Outer Perimeter", new Vector3f(1, 1, 0))},
            {FillTypeIntegerId[OpenShellCurveFillType.Label], new FillType("Open Mesh Curve", new Vector3f(0, 1, 1))},
            {FillTypeIntegerId[SolidFillType.Label], new FillType("Solid Fill", new Vector3f(0, 0.5f, 1))},
            {FillTypeIntegerId[SparseFillType.Label], new FillType("Sparse Fill", new Vector3f(0.5f, 0, 1))},
            {FillTypeIntegerId[SupportFillType.Label], new FillType("Support", new Vector3f(1, 0, 1))},
            {FillTypeIntegerId[BridgeFillType.Label], new FillType("Bridge", new Vector3f(0, 0, 1))},
        };

        private readonly FixedRangeCustomDataDetails customDataBeadWidth =
            new FixedRangeCustomDataDetails(
                () => "Bead Width",
                (value) => $"{value:F2} mm", 0.1f, 0.8f);

        public IVisualizerCustomDataDetails CustomDataDetails0 => customDataBeadWidth;

        private readonly AdaptiveRangeCustomDataDetails customDataFeedRate =
            new AdaptiveRangeCustomDataDetails(
                () => "Feed Rate",
                (value) => $"{value:F0} mm/min");

        public IVisualizerCustomDataDetails CustomDataDetails1 => customDataFeedRate;

        private readonly NormalizedAdaptiveRangeCustomDataDetails customDataCompletion =
            new NormalizedAdaptiveRangeCustomDataDetails(
                () => "Completion",
                (value) => $"{value:P0}");

        public IVisualizerCustomDataDetails CustomDataDetails2 => customDataCompletion;

        public IVisualizerCustomDataDetails CustomDataDetails3 => null;
        public IVisualizerCustomDataDetails CustomDataDetails4 => null;
        public IVisualizerCustomDataDetails CustomDataDetails5 => null;

        public void BeginGCodeLineStream()
        {
            lastVertex = new PrintVertex(Vector3d.Zero, 0, Vector2d.Zero);
            layerIndex = 0;
        }

        public virtual void ProcessGCodeLine(GCodeLine line)
        {
            if (line == null || line.type == GCodeLine.LType.Blank)
                return;

            pointCount++;

            if (line.comment != null && line.comment.Contains("layer") && !line.comment.Contains("feature"))
            {
                if (toolpath != null)
                {
                    Emit(toolpath, layerIndex, pointCount - toolpath.Count);
                    toolpath = null;
                }
                layerIndex++;

                return;
            }

            ExtractPositionFeedrateAndExtrusion(line, ref position, ref feedrate, ref extrusion);
            ExtractDimensions(line, ref dimensions);
            GCodeLineUtil.ExtractFillType(line, ref fillType);

            if (line.comment?.Contains("Plane Change") ?? false)
            {
                OnNewPlane(position.z, layerIndex);
            }

            PrintVertex vertex = new PrintVertex(position, feedrate, dimensions)
            {
                Extrusion = new Vector3d(extrusion, 0, 0),
                Source = fillType,
            };

            if (line.type == GCodeLine.LType.GCode)
            {
                if (toolpath == null)
                {
                    if (extrusion > lastVertex.Extrusion.x)
                    {
                        lastVertex.Source = fillType;
                        lastVertex.Dimensions = vertex.Dimensions;
                        lastVertex.FeedRate = vertex.FeedRate;
                        lastVertex.ExtendedData = vertex.ExtendedData;

                        toolpath = new List<PrintVertex> { lastVertex, vertex };
                    }
                    else
                    {
                        RaiseLineGenerated(new List<Vector3d>() { lastVertex.Position, vertex.Position }, layerIndex);
                    }
                }
                else
                {
                    toolpath.Add(vertex);
                    if (extrusion <= lastVertex.Extrusion.x)
                    {
                        Emit(toolpath, layerIndex, pointCount - toolpath.Count);
                        toolpath = null;
                    }
                }
            }

            lastVertex = vertex;
        }

        protected void RaiseLineGenerated(List<Vector3d> list, int layerIndex)
        {
            OnLineGenerated(list, layerIndex);
        }

        public void EndGCodeLineStream()
        {
            if (toolpath != null)
            {
                Emit(toolpath, layerIndex, pointCount - toolpath.Count);
            }
        }

        protected void Emit(List<PrintVertex> printVertices, int layerIndex, int startPointCount)
        {
            List<ToolpathPreviewVertex> vertices = new List<ToolpathPreviewVertex>();
            List<int> triangles = new List<int>();

            ToolpathPreviewJoint[] joints = new ToolpathPreviewJoint[printVertices.Count];

            joints[joints.Length - 1] = GenerateMiterJoint(printVertices, joints.Length - 1, layerIndex, startPointCount, vertices);

            for (int i = joints.Length - 2; i > 0; i--)
            {
                Vector3d a = printVertices[i - 1].Position;
                Vector3d b = printVertices[i].Position;
                Vector3d c = printVertices[i + 1].Position;
                Vector3d ab = b - a;
                Vector3d bc = c - b;
                var angleRad = SignedAngleRad(ab.xy, bc.xy);
                if (Math.Abs(angleRad) > 0.698132)
                {
                    if (angleRad < 0)
                    {
                        joints[i] = GenerateRightBevelJoint(printVertices, i, layerIndex, startPointCount, vertices, triangles);
                    }
                    else
                    {
                        joints[i] = GenerateLeftBevelJoint(printVertices, i, layerIndex, startPointCount, vertices, triangles);
                    }
                }
                else
                {
                    joints[i] = GenerateMiterJoint(printVertices, i, layerIndex, startPointCount, vertices);
                }
            }

            joints[0] = GenerateMiterJoint(printVertices, 0, layerIndex, startPointCount, vertices);

            AddEdges(joints, triangles);

            var mesh = new Tuple<ToolpathPreviewVertex[], int[]>(vertices.ToArray(), triangles.ToArray());

            EndEmit(mesh, layerIndex);
        }

        protected void AddEdges(ToolpathPreviewJoint[] joints, List<int> triangles)
        {
            for (int i = joints.Length - 2; i >= 0; i--)
            {
                var start = joints[i];
                var end = joints[i + 1];

                triangles.Add(start.out0);
                triangles.Add(start.out1);
                triangles.Add(end.in0);

                triangles.Add(end.in0);
                triangles.Add(start.out1);
                triangles.Add(end.in1);

                triangles.Add(start.out1);
                triangles.Add(start.out2);
                triangles.Add(end.in1);

                triangles.Add(end.in1);
                triangles.Add(start.out2);
                triangles.Add(end.in2);

                triangles.Add(start.out2);
                triangles.Add(start.out3);
                triangles.Add(end.in2);

                triangles.Add(end.in2);
                triangles.Add(start.out3);
                triangles.Add(end.in3);

                triangles.Add(start.out3);
                triangles.Add(start.out0);
                triangles.Add(end.in3);

                triangles.Add(end.in3);
                triangles.Add(start.out0);
                triangles.Add(end.in0);
            }
        }

        protected ToolpathPreviewJoint GenerateMiterJoint(List<PrintVertex> toolpath, int toolpathIndex, int layerIndex, int startPointCount, List<ToolpathPreviewVertex> vertices)
        {
            double miterSecant = 1;
            Vector3d miterNormal;

            if (toolpathIndex == 0)
            {
                Vector3d miterTangent = toolpath[1].Position - toolpath[0].Position;
                miterNormal = new Vector3d(-miterTangent.y, miterTangent.x, 0).Normalized;
            }
            else if (toolpathIndex == toolpath.Count - 1)
            {
                Vector3d miterTangent = toolpath[toolpathIndex].Position - toolpath[toolpathIndex - 1].Position;
                miterNormal = new Vector3d(-miterTangent.y, miterTangent.x, 0).Normalized;
            }
            else
            {
                Vector3d a = toolpath[toolpathIndex - 1].Position;
                Vector3d b = toolpath[toolpathIndex].Position;
                Vector3d c = toolpath[toolpathIndex + 1].Position;
                Vector3d ab = (b - a).Normalized;
                Vector3d bc = (c - b).Normalized;
                miterNormal = GetNormalAndSecant(ab, bc, out miterSecant);
            }

            var pointCount = startPointCount + toolpathIndex;
            var point = toolpath[toolpathIndex].Position;
            var dimensions = toolpath[toolpathIndex].Dimensions;

            int fillType = GetFillTypeInteger(toolpath[toolpathIndex]);

            var feedRate = toolpath[toolpathIndex].FeedRate;
            ToolpathPreviewJoint joint = new ToolpathPreviewJoint();

            joint.in0 = joint.out0 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0.5f, -0.5f), miterSecant, 0, pointCount);
            joint.in1 = joint.out1 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, 0), miterSecant, 1, pointCount);
            joint.in2 = joint.out2 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(-0.5f, -0.5f), miterSecant, 0, pointCount);
            joint.in3 = joint.out3 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, -1), miterSecant, 1, pointCount);

            return joint;
        }

        private static int GetFillTypeInteger(PrintVertex vertex)
        {
            if (vertex.Source is string s && FillTypeIntegerId.TryGetValue(s, out int newFillType))
            {
                return newFillType;
            }
            else
            {
                return FillTypeIntegerId[DefaultFillType.Label];
            }
        }

        protected ToolpathPreviewJoint GenerateRightBevelJoint(List<PrintVertex> toolpath, int toolpathIndex,
            int layerIndex, int startPointCount, List<ToolpathPreviewVertex> vertices, List<int> triangles)
        {
            Vector3d a = toolpath[toolpathIndex - 1].Position;
            Vector3d b = toolpath[toolpathIndex].Position;
            Vector3d c = toolpath[toolpathIndex + 1].Position;
            Vector3d ab = (b - a).Normalized;
            Vector3d bc = (c - b).Normalized;
            Vector3d miterNormal = GetNormalAndSecant(ab, bc, out double miterSecant);
            Vector3d miterTangent = ab + bc;

            var pointCount = startPointCount + toolpathIndex;
            var point = toolpath[toolpathIndex].Position;
            var dimensions = toolpath[toolpathIndex].Dimensions;

            var fillType = GetFillTypeInteger(toolpath[toolpathIndex]);

            var feedRate = toolpath[toolpathIndex].FeedRate;
            ToolpathPreviewJoint joint = new ToolpathPreviewJoint();

            var bevelNormalIn = GetNormalAndSecant(ab, miterTangent, out double bevelSecantIn);
            joint.in0 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, bevelNormalIn, new Vector2d(0.5, -0.5), bevelSecantIn, 0, pointCount);

            var bevelNormalOut = GetNormalAndSecant(miterTangent, bc, out double bevelSecantOut);
            joint.out0 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, bevelNormalOut, new Vector2d(0.5, -0.5), bevelSecantOut, 0, pointCount);

            joint.in1 = joint.out1 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, 0), miterSecant, 1, pointCount);
            joint.in2 = joint.out2 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(-0.5, -0.5), miterSecant, 0, pointCount);
            joint.in3 = joint.out3 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, -1), miterSecant, 1, pointCount);

            triangles.Add(joint.in0);
            triangles.Add(joint.in1);
            triangles.Add(joint.out0);

            triangles.Add(joint.in0);
            triangles.Add(joint.out0);
            triangles.Add(joint.in3);

            return joint;
        }

        protected ToolpathPreviewJoint GenerateLeftBevelJoint(List<PrintVertex> toolpath, int toolpathIndex,
            int layerIndex, int startPointCount, List<ToolpathPreviewVertex> vertices, List<int> triangles)
        {
            Vector3d a = toolpath[toolpathIndex - 1].Position;
            Vector3d b = toolpath[toolpathIndex].Position;
            Vector3d c = toolpath[toolpathIndex + 1].Position;
            Vector3d ab = (b - a).Normalized;
            Vector3d bc = (c - b).Normalized;
            Vector3d miterNormal = GetNormalAndSecant(ab, bc, out double miterSecant);
            Vector3d miterTangent = ab + bc;

            var pointCount = startPointCount + toolpathIndex;
            var point = toolpath[toolpathIndex].Position;
            var dimensions = toolpath[toolpathIndex].Dimensions;

            var fillType = GetFillTypeInteger(toolpath[toolpathIndex]);

            var feedRate = toolpath[toolpathIndex].FeedRate;
            ToolpathPreviewJoint joint = new ToolpathPreviewJoint();

            joint.in0 = joint.out0 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0.5f, -0.5f), miterSecant, 0, pointCount);
            joint.in1 = joint.out1 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, 0), miterSecant, 1, pointCount);

            var bevelNormalIn = GetNormalAndSecant(ab, miterTangent, out double bevelSecantIn);
            joint.in2 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, bevelNormalIn, new Vector2d(-0.5f, -0.5f), bevelSecantIn, 0, pointCount);

            var bevelNormalOut = GetNormalAndSecant(miterTangent, bc, out double bevelSecantOut);
            joint.out2 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, bevelNormalOut, new Vector2d(-0.5f, -0.5f), bevelSecantOut, 0, pointCount);

            joint.in3 = joint.out3 = AddVertex(vertices, layerIndex, point, fillType, dimensions, feedRate, miterNormal, new Vector2d(0, -1), miterSecant, 1, pointCount);

            triangles.Add(joint.in2);
            triangles.Add(joint.in3);
            triangles.Add(joint.out2);

            triangles.Add(joint.in2);
            triangles.Add(joint.out2);
            triangles.Add(joint.in1);

            return joint;
        }

        protected virtual int AddVertex(List<ToolpathPreviewVertex> vertices, int layerIndex, Vector3d point,
            int fillType, Vector2d dimensions, double feedrate, Vector3d miterNormal,
            Vector2d crossSectionVertex, double secant, float brightness, int pointCount)
        {
            Vector3d offset = miterNormal * (dimensions.x * crossSectionVertex.x * secant) + new Vector3d(0, 0, dimensions.y * crossSectionVertex.y);
            Vector3d vertex = point + offset;

            Vector3f color = FillTypes[0].Color;
            if (FillTypes.TryGetValue(fillType, out var fillInfo))
            {
                color = fillInfo.Color;
            }

            vertices.Add(VertexFactory(layerIndex, fillType, dimensions, feedrate, brightness, pointCount, vertex, color));

            return vertices.Count - 1;
        }

        protected virtual ToolpathPreviewVertex VertexFactory(int layerIndex, int fillType, Vector2d dimensions, double feedrate, float brightness, int pointCount, Vector3d vertex, Vector3f color)
        {
            // Update adaptive ranges for custom data
            customDataFeedRate.ObserveValue((float)feedrate);
            customDataCompletion.ObserveValue(pointCount);

            return new ToolpathPreviewVertex(vertex,
                fillType, layerIndex, color, brightness,
                (float)dimensions.x, (float)feedrate, pointCount);
        }

        protected static Vector3d GetNormalAndSecant(Vector3d ab, Vector3d bc, out double secant)
        {
            secant = 1 / Math.Cos(Vector3d.AngleR(ab, bc) * 0.5);
            Vector3d tangent = ab + bc;
            return new Vector3d(-tangent.y, tangent.x, 0).Normalized;
        }

        protected static double SignedAngleRad(Vector2d a, Vector2d b)
        {
            var angleB = Math.Atan2(b.y, b.x);
            var angleA = Math.Atan2(a.y, a.x);
            var ret = angleB - angleA;
            if (ret > Math.PI)
                ret -= Math.PI * 2;
            if (ret < -Math.PI)
                ret += Math.PI * 2;
            return ret;
        }

        protected struct ToolpathPreviewJoint
        {
            public int in0;
            public int in1;
            public int in2;
            public int in3;

            public int out0;
            public int out1;
            public int out2;
            public int out3;
        }

        protected void EndEmit(Tuple<ToolpathPreviewVertex[], int[]> mesh, int layerIndex)
        {
            OnMeshGenerated?.Invoke(mesh.Item1, mesh.Item2, layerIndex);
        }

        protected static void ExtractPositionFeedrateAndExtrusion(GCodeLine line, ref Vector3d position, ref double feedrate, ref double extrusion)
        {
            if (line.parameters != null)
            {
                foreach (var param in line.parameters)
                {
                    switch (param.identifier)
                    {
                        case "X": position.x = param.doubleValue; break;
                        case "Y": position.y = param.doubleValue; break;
                        case "Z": position.z = param.doubleValue; break;
                        case "F": feedrate = param.doubleValue; break;
                        case "E": extrusion = param.doubleValue; break;
                    }
                }
            }
        }

        protected virtual void ExtractDimensions(GCodeLine line, ref Vector2d dimensions)
        {
            if (line.comment != null && line.comment.Contains("tool"))
            {
                foreach (var word in line.comment.Split(' '))
                {
                    int i = word.IndexOf('W');
                    if (i >= 0)
                        dimensions.x = double.Parse(word.Substring(i + 1));
                    i = word.IndexOf('H');
                    if (i >= 0)
                        dimensions.y = double.Parse(word.Substring(i + 1));
                }
            }
        }

        public void PrintLayerCompleted(PrintLayerData printLayerData)
        {
            throw new NotImplementedException();
        }
    }
}