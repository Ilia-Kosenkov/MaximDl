using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Abstractions;
using System.Collections.Immutable;
using System.Linq;
internal class DataSetInfo
{
    public static DataSetInfo Empty { get; } = new DataSetInfo();
    public ImmutableList<IFileSystemInfo> Files { get; }
    public ImmutableList<IFileSystemInfo> Bias { get; }
    public ImmutableList<IFileSystemInfo> Dark { get; }
    public string OutFolder { get; }
    public string Suffix { get; }
    public BinType Bin { get; }

    internal DataSetInfo()
    {
        Files = ImmutableList<IFileSystemInfo>.Empty;
        Bias = ImmutableList<IFileSystemInfo>.Empty;
        Dark = ImmutableList<IFileSystemInfo>.Empty;
        OutFolder = "calibrated";
        Suffix = "_calib";
        Bin = BinType.NoBin;
    }

    public DataSetInfo(Options opts)
    {
        Bin = Enum.IsDefined(typeof(BinType), opts.Bin)
            ? (BinType)(opts.Bin)
            : throw new ArgumentException("Invalid binning type.", nameof(opts));
        OutFolder = opts.OutFolder;
        Suffix = opts.Suffix;

        if(opts.Darks?.Any() ?? false)
            Dark = 
                (from glob in opts.Darks
                from desc in Ganss.IO.Glob.Expand(glob)
                select desc).ToImmutableList();

        if(opts.Bias?.Any() ?? false)
            Bias = 
                (from glob in opts.Bias
                from desc in Ganss.IO.Glob.Expand(glob)
                select desc).ToImmutableList();

        Regex darkPattern = Dark is null && !string.IsNullOrWhiteSpace(opts.DarkPattern)
            ? new Regex(opts.DarkPattern)
            : null;
        Regex biasPattern = Bias is null && !string.IsNullOrWhiteSpace(opts.BiasPattern)
            ? new Regex(opts.BiasPattern)
            : null;

        var darkBuilder = darkPattern is null 
            ? null
            : ImmutableList.CreateBuilder<IFileSystemInfo>();
        var biasBuilder = darkPattern is null 
            ? null
            : ImmutableList.CreateBuilder<IFileSystemInfo>();
        var filesBuilder = ImmutableList.CreateBuilder<IFileSystemInfo>();

        foreach(var item in 
                from glob in opts.Files
                from desc in Ganss.IO.Glob.Expand(glob)
                select desc)
        {
            if(darkPattern?.IsMatch(item.Name) ?? false)
                darkBuilder.Add(item);
            else if(biasPattern?.IsMatch(item.Name) ?? false)
                biasBuilder.Add(item);
            else
                filesBuilder.Add(item);
        }

        Files = filesBuilder.ToImmutable();
        Dark = Dark is null ? darkBuilder.ToImmutable() : Dark;
        Bias = Bias is null ? biasBuilder.ToImmutable() : Bias;
    }

    public IEnumerable<(string Source, string Target)> EnumeratePaths()
    {
        var isFull = Path.IsPathFullyQualified(OutFolder);
        var extraPath = isFull ? Path.GetFullPath(OutFolder) : OutFolder;
        foreach(var item in Files)
        {
            var fullSrcPath = item.FullName;
            var targetPath = 
                Path.Combine(
                    isFull 
                        ? extraPath 
                        : Path.Combine(Path.GetDirectoryName(fullSrcPath), extraPath), 
                    Path.GetFileNameWithoutExtension(fullSrcPath) + Suffix + Path.GetExtension(fullSrcPath));
            yield return(Source: fullSrcPath, Target: targetPath);
        }
    }
}