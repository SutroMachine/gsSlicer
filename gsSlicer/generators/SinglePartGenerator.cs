using g3;
using Sutro.PathWorks.Plugins.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace gs
{
    public class SinglePartGenerator<TPrintGenerator, TPrintSettings> : IGenerator<TPrintSettings>
            where TPrintGenerator : IPrintGenerator<TPrintSettings>, new()
            where TPrintSettings : IPlanarAdditiveSettings
    {
        public bool AcceptsParts { get; } = true;
        public bool AcceptsPartSettings { get; } = false;

        public SinglePartGenerator(bool acceptsParts = true)
        {
            AcceptsParts = acceptsParts;
        }

        public Version Version
        {
            get
            {
                var assembly = Assembly.GetAssembly(typeof(TPrintGenerator));
                return assembly.GetName().Version;
            }
        }

        public void SaveGCode(TextWriter output, GCodeFile file)
        {
            StandardGCodeWriter writer = new StandardGCodeWriter();
            writer.WriteFile(file, output);
        }

        public GCodeFile LoadGCode(TextReader input)
        {
            GenericGCodeParser parser = new GenericGCodeParser();
            return parser.Parse(input);
        }

        public GCodeFile GenerateGCode(IList<Tuple<DMesh3, TPrintSettings>> parts,
                                       TPrintSettings globalSettings,
                                       out IEnumerable<string> generationReport,
                                       Action<GCodeLine> gcodeLineReadyF = null,
                                       Action<string> progressMessageF = null)
        {
            if (AcceptsParts == false && parts != null && parts.Count > 0)
                throw new Exception("Must pass null or empty list of parts to generator that does not accept parts.");

            // Create print mesh set
            PrintMeshAssembly meshes = new PrintMeshAssembly();

            foreach (var part in parts)
            {
                if (part.Item2 != null)
                    throw new ArgumentException($"Entries for the `parts` arguments must have a null second item since this generator does not handle per-part settings.");
                meshes.AddMesh(part.Item1, PrintMeshOptions.Default());
            }

            progressMessageF?.Invoke("Slicing...");

            // Do slicing
            MeshPlanarSlicer slicer = new MeshPlanarSlicer()
            {
                LayerHeightMM = globalSettings.LayerHeightMM
            };

            slicer.Add(meshes);
            PlanarSliceStack slices = slicer.Compute();

            // Run the print generator
            progressMessageF?.Invoke("Running print generator...");
            var printGenerator = new TPrintGenerator();
            AssemblerFactoryF overrideAssemblerF = globalSettings.AssemblerType();
            printGenerator.Initialize(meshes, slices, globalSettings, overrideAssemblerF);

            if (printGenerator.Generate())
            {
                generationReport = printGenerator.GenerationReport;
                return printGenerator.Result;
            }
            else
            {
                throw new Exception("PrintGenerator failed to generate gcode!");
            }
        }

        public GCodeFile GenerateGCode(IList<Tuple<DMesh3, object>> parts,
                                       object globalSettings,
                                       out IEnumerable<string> generationReport,
                                       Action<GCodeLine> gcodeLineReadyF = null,
                                       Action<string> progressMessageF = null)
        {
            var partsTypedSettings = new List<Tuple<DMesh3, TPrintSettings>>();
            foreach (var part in parts)
            {
                partsTypedSettings.Add(Tuple.Create(part.Item1, (TPrintSettings)(part.Item2)));
            }

            return GenerateGCode(partsTypedSettings, (TPrintSettings)globalSettings, out generationReport,
                                 gcodeLineReadyF, progressMessageF);
        }
    }
}