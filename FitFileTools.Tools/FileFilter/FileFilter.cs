using System.IO;
using System.Reflection;
using Dynastream.Fit;
using FitFileTools.Tools.MergeTool;
using File = System.IO.File;

namespace FitFileTools.Tools.FileFilter
{
    public class FileFilter
    {
        private readonly IFilter _filter;

        public FileFilter(IFilter filter)
        {
            _filter = filter;
        }

        public void FilterFolder(string inputPath, string outputPath)
        {
            ProcessDirectory(inputPath, outputPath);
        }

        private void ProcessDirectory(string inputDir, string outputDir)
        {
            foreach (var file in Directory.EnumerateFiles(inputDir))
            {
                if (!FileAlreadyFiltered(file, outputDir) && IncludeFile(file))
                {
                    CopyFile(file, outputDir);
                }
            }
        }

        private bool FileAlreadyFiltered(string file, string outputDir)
        {
            var outputFile = Path.Join(outputDir, Path.GetFileName(file));
            return File.Exists(outputFile);
        }

        private void CopyFile(string file, string outputDir)
        {
            var outputFile = Path.Join(outputDir, Path.GetFileName(file));
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            File.Copy(file, outputFile);
        }

        private bool IncludeFile(string file)
        {
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            var reader = new FitReader(fs);
            if (reader.IsValid())
            {
                return _filter.FileMatchesFilter(reader);
            }

            return false;
        }
    }

    public interface IFilter
    {
        bool FileMatchesFilter(FitReader reader);
    }
}