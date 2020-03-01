using gs;
using gs.FillTypes;
using gs.utility;
using gsCore.FunctionalTests.Models;
using Sutro.PathWorks.Plugins.API;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace gsCore.FunctionalTests.Utility
{
    public class ResultAnalyzer<TFeatureInfo> : IResultAnalyzer where TFeatureInfo : IFeatureInfo, new()
    {
        private readonly IFeatureInfoFactory<TFeatureInfo> featureInfoFactory;

        public ResultAnalyzer(IFeatureInfoFactory<TFeatureInfo> featureInfoFactory)
        {
            this.featureInfoFactory = featureInfoFactory;
        }

        public void CompareResults(string pathExpected, string pathActual)
        {
            var expected = PerLayerInfoFromGCodeFile(pathExpected);
            var actual = PerLayerInfoFromGCodeFile(pathActual);

            if (actual.Count != expected.Count)
            {
                throw new LayerCountMismatch($"Expected {expected.Count} layers but the result has {actual.Count}.");
            }

            for (int layerIndex = 0; layerIndex < actual.Count; layerIndex++)
            {
                actual[layerIndex].AssertEqualsExpected(expected[layerIndex]);
            }
        }

        protected virtual GCodeFile LoadGCodeFileFromDisk(string gcodeFilePath)
        {
            var parser = new GenericGCodeParser();
            using var fileReader = File.OpenText(gcodeFilePath);
            return parser.Parse(fileReader);
        }

        protected virtual List<LayerInfo<TFeatureInfo>> PerLayerInfoFromGCodeFile(string gcodeFilePath)
        {
            return PerLayerInfoFromGCodeFile(LoadGCodeFileFromDisk(gcodeFilePath));
        }

        public virtual bool LineIsNewLayer(GCodeLine line)
        {
            return NewLayerPattern.Match(line?.comment ?? "").Success;
        }

        protected virtual Regex NewLayerPattern => new Regex(@"layer [0-9]+");

        protected virtual List<LayerInfo<TFeatureInfo>> PerLayerInfoFromGCodeFile(GCodeFile gcode)
        {
            featureInfoFactory.Initialize();
            var layers = new List<LayerInfo<TFeatureInfo>>();

            LayerInfo<TFeatureInfo> currentLayer = null;
            string fillType = DefaultFillType.Label;

            foreach (var line in gcode.AllLines())
            {
                if (LineIsNewLayer(line))
                {
                    if (currentLayer != null)
                    {
                        currentLayer.AddFeatureInfo(featureInfoFactory.SwitchFeature(fillType));
                        layers.Add(currentLayer);
                    }
                    currentLayer = new LayerInfo<TFeatureInfo>();
                    continue;
                }

                if (LineIsNewFeatureType(line, fillType, out var newFillType))
                {
                    currentLayer?.AddFeatureInfo(featureInfoFactory.SwitchFeature(newFillType));
                }

                featureInfoFactory.ObserveGcodeLine(line);
            }

            currentLayer?.AddFeatureInfo(featureInfoFactory.SwitchFeature(fillType));
            layers.Add(currentLayer);
            return layers;
        }

        private bool LineIsNewFeatureType(GCodeLine line, string fillType, out string newFillType)
        {
            newFillType = fillType;
            if (GCodeLineUtil.ExtractFillType(line, ref newFillType))
            {
                if (newFillType != fillType)
                {
                    return true;
                }
            }
            return false;
        }
    }
}