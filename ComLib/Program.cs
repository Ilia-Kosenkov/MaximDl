
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using CommandLine;
using Ganss.IO;

namespace MaximDl
{
    class Program
    {
        static async Task <int> Main(string[] args)
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
                err => Task.FromResult(-1));

        }

        private static async Task Calibrate(DataSetInfo info)
        {
            using var app = MaxImDlApp.Acquire();
            using var doc = MaxImDlDoc.Acquire();
            app.CalMedianBias = true;
            app.CalMedianDark = true;

            app.CalClear();
            
            foreach(var item in info.Bias)
                app.CalAddBias(item.FullName);
            foreach(var item in info.Dark)
                app.CalAddDark(item.FullName);

            app.CalSet();


            foreach(var item in info.EnumeratePaths())
            {
                var task = Task.Run(() => 
                {
                    doc.OpenFile(item.Source);
                    doc.Calibrate();
                    if(info.Bin != BinType.NoBin)
                        doc.Bin(info.Bin);
                });
                var targetDir = Path.GetDirectoryName(item.Target);
                if(!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);
                
                await task;
                doc.SaveFile(item.Target, 3);
            }
        }

      
    }
}

