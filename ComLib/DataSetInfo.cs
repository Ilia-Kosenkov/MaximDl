using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO.Abstractions;
using System.Collections.Immutable;
using System.Linq;
internal class DataSetInfo
{
    public static DataSetInfo Empty { get; } = new DataSetInfo(Enumerable.Empty<IFileSystemInfo>());
    public ImmutableList<IFileSystemInfo> Files {get;}
    public ImmutableList<IFileSystemInfo> Bias {get;}
    public ImmutableList<IFileSystemInfo> Dark {get;}

    public BinType Bin {get;}
    public DataSetInfo(
        IEnumerable<IFileSystemInfo> files, 
        IEnumerable<IFileSystemInfo> bias,
        IEnumerable<IFileSystemInfo> dark,
        BinType bin = BinType.NoBin)
    {
        Files = files?.ToImmutableList() ?? ImmutableList<IFileSystemInfo>.Empty;
        Bias = bias?.ToImmutableList() ?? ImmutableList<IFileSystemInfo>.Empty;
        Dark = dark?.ToImmutableList() ?? ImmutableList<IFileSystemInfo>.Empty;
        Bin = bin;
    }

    public DataSetInfo(
        IEnumerable<IFileSystemInfo> allFiles,
        string biasPattern = "_bias_",
        string darkPattern = "_dark_",
        BinType bin = BinType.NoBin)
    {
        var biasRegex = new Regex(biasPattern, RegexOptions.Compiled);
        var darkRegex = new Regex(darkPattern, RegexOptions.Compiled);

        var filesBuilder = ImmutableList.CreateBuilder<IFileSystemInfo>();
        var biasBuilder = ImmutableList.CreateBuilder<IFileSystemInfo>();
        var darkBuilder = ImmutableList.CreateBuilder<IFileSystemInfo>();

        foreach(var file in allFiles)
        {
            if(biasRegex.IsMatch(file.Name))
                biasBuilder.Add(file);
            else if (darkRegex.IsMatch(file.Name))
                darkBuilder.Add(file);
            else
                filesBuilder.Add(file);
        }

        Files = filesBuilder.ToImmutable();
        Bias = biasBuilder.ToImmutable();
        Dark = darkBuilder.ToImmutable();
        Bin = bin;
    }

    public IEnumerable<(string Source, string Target)> EnumeratePaths(
        string folder = "calibrated", 
        string suffix = "_calib")
    {
        foreach(var item in Files)
        {
            var fullSrcPath = item.FullName;
            var targetPath = 
                System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(fullSrcPath),
                    folder, 
                    System.IO.Path.GetFileNameWithoutExtension(fullSrcPath) + suffix + System.IO.Path.GetExtension(fullSrcPath));
            yield return(Source: fullSrcPath, Target: targetPath);
        }
    }
}