using System.Collections.Generic;
using CommandLine;
internal class Options
{
    [Value(0, Min = 1, Required = true, HelpText = @"List of input files")]
    public IEnumerable<string> Files { get; set; }

    [Option('d', "dark", Separator=';', HelpText = "Explicitly specified dark files")]
    public IEnumerable<string> Darks { get; set; }

    [Option('b', "bias", Separator=';', HelpText = "Explicitly specified bias files")]
    public IEnumerable<string> Bias { get; set; }

    [Option("bin", Default = (ushort)0, HelpText = "Bining, 0 | 2 | 3")]
    public ushort Bin { get; set; }

    [Option('o', "out-folder", Default = "calibrated", HelpText = "Folder to put calibrated frames into")]
    public string OutFolder { get; set; }

    [Option("dark-pattern", Default = "_dark_", HelpText = "When no darks are provided, used to match darks in the file collection")]
    public string DarkPattern { get; set; }

    [Option("bias-pattern", Default = "_bias_", HelpText = "When no bias are provided, used to match bias in the file collection")]
    public string BiasPattern { get; set; }
    [Option("suffix", Default = "_calib", HelpText = "Suffix appended to each file (before the extension)")]
    public string Suffix { get; set; }
}