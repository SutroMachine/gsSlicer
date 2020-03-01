using System;
using System.IO;
using System.Reflection;

namespace gsCore.FunctionalTests.Utility
{
    public static class TestDataPaths
    {
        public static DirectoryInfo GetTestDataDirectory(string name)
        {
            var searchDirectory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            do
            {
                foreach (var subdirectory in searchDirectory.GetDirectories())
                {
                    if (subdirectory.Name == "TestData")
                    {
                        return new DirectoryInfo(Path.Combine(subdirectory.FullName, name));
                    }
                }
            } while ((searchDirectory = searchDirectory.Parent) != null);

            throw new FileNotFoundException("No TestData directory found.");
        }

        public static string GetMeshFilePath(DirectoryInfo directory)
        {
            var meshFiles = directory.GetFiles("*.stl");
            if (meshFiles.Length != 1) throw new ArgumentException("Expected single STL file in directory");
            return meshFiles[0].FullName;
        }

        public static string GetResultFilePath(DirectoryInfo directory)
        {
            return Path.Combine(directory.FullName, directory.Name + ".Result.gcode");
        }

        public static string GetExpectedFilePath(DirectoryInfo directory)
        {
            return Path.Combine(directory.FullName, directory.Name + ".Expected.gcode");
        }
    }
}