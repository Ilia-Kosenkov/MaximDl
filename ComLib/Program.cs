using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Ganss.IO;

namespace MaximDl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var prasedArgs = ParseArgs(args);
            await Calibrate(prasedArgs);
        }

        private static DataSetInfo ParseArgs(string[] args)
            => args switch
            {
                _ when args.Length == 1 => new DataSetInfo(Glob.Expand(args[0])),
                _ when args.Length == 3 => new DataSetInfo(Glob.Expand(args[0]), Glob.Expand(args[1]), Glob.Expand(args[2])),
                _ => DataSetInfo.Empty
            };
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
                });
                var targetDir = Path.GetDirectoryName(item.Target);
                if(!Directory.Exists(targetDir))
                    Directory.CreateDirectory(targetDir);
                // doc.Bin(2);
                await task;
                doc.SaveFile(item.Target, 3);
            }
        }

      
    }
}

