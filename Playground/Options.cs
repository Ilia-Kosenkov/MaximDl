using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CommandLine;

namespace Playground
{
    internal class Options
    {
        [Value(0, Min = 1, Required = true, HelpText = @"List of input files")]
        public IEnumerable<string> Files { get; set; }

        [Option('o', "out-folder", Default = ".", HelpText = "Output path for CSVs")]
        public string OutFolder { get; set; }

        [Option('h', "header", Default = false, HelpText = "Print commented out header in the csv file")]
        public bool PrintHeader { get; set; }

        [Option("comment-char", Default = "#", HelpText = "CSV header comment sign")]
        public string CommentChar { get; set; }

        public IEnumerable<IFileSystemInfo> EnumerateFiles() =>
            from pathGlob in Files
            from item in Ganss.IO.Glob.Expand(pathGlob)
            select item;

        public string OutFolderPath()
        {
            var path = Path.GetFullPath(OutFolder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}
