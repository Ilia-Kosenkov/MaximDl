using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using MaximDl;

namespace Calibrator
{
    internal class Program
    {
        private static async Task <int> Main(string[] args)
        {
            using var parser = new Parser(x => x.EnableDashDash = true);
            var result = parser.ParseArguments<Options>(args);
            return await result.MapResult(
                async opts => 
                    {
                        var info = new DataSetInfo(opts);
                        await Calibrate(info);
                        return 0;
                    },
                async err => 
                {
                    await Console.Error.WriteLineAsync(CommandLine.Text.HelpText.AutoBuild(result));
                    return -1;
                });

        }

        private static async Task Calibrate(DataSetInfo info)
        {
            using var app = MaxImDlApp.Acquire() ?? throw new InvalidOperationException("Failed to create app.");
            using var doc = app.CreateDocument();
            app.CalMedianBias = true;
            app.CalMedianDark = true;

            app.CalClear();
            
            foreach(var item in info.Bias)
                app.CalAddBias(item.FullName);
            foreach(var item in info.Dark)
                app.CalAddDark(item.FullName);

            app.CalSet();

            using var progressBar = new ShellProgressBar.ProgressBar(
                info.Files.Count,
                "Calibrating files...",
                new ShellProgressBar.ProgressBarOptions()
                {
                    DisplayTimeInRealTime = true,
                    EnableTaskBarProgress = true
                }
            );
            foreach(var item in info.EnumeratePaths())
            {
                var task = Task.Run(() => 
                {
                    // ReSharper disable AccessToDisposedClosure
                    doc.OpenFile(item.Source);
                    doc.Calibrate();
                    if(info.Bin != BinType.NoBin)
                        doc.Bin(info.Bin);
                    // ReSharper restore AccessToDisposedClosure
                });
                var targetDir = Path.GetDirectoryName(item.Target);
                if(!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);
                
                var fileName = Path.GetFileNameWithoutExtension(item.Source);
                await task;
                doc.SaveFile(item.Target, 3);
                progressBar.Tick(fileName);
            }
        }

      
    }
}

