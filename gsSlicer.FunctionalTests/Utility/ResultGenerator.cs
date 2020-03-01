using g3;
using gs;
using Sutro.PathWorks.Plugins.API;
using System;
using System.IO;

namespace gsCore.FunctionalTests.Utility
{
    public class ResultGenerator<TGenerator, TSettings> : IResultGenerator
        where TGenerator : IPrintGenerator<TSettings>, new()
        where TSettings : PlanarAdditiveSettings
    {
        private readonly SinglePartGenerator<TGenerator, TSettings> generator;
        private readonly ILogger logger;
        private readonly TSettings settings;

        public ResultGenerator(SinglePartGenerator<TGenerator, TSettings> generator, TSettings settings, ILogger logger)
        {
            this.generator = generator;
            this.logger = logger;
            this.settings = settings;
        }

        protected void SaveGCode(string path, GCodeFile file)
        {
            logger.WriteLine($"Saving file to {path}");
            using var streamWriter = new StreamWriter(path);
            var gCodeWriter = new StandardGCodeWriter();
            gCodeWriter.WriteFile(file, streamWriter);
        }

        public void GenerateResultFile(string meshFilePath, string outputFilePath)
        {
            var parts = new[]{
                new Tuple<DMesh3, TSettings>(StandardMeshReader.ReadMesh(meshFilePath), null)
            };

            SaveGCode(outputFilePath, generator.GenerateGCode(parts, settings, out var generationReport, null, null));
        }
    }
}