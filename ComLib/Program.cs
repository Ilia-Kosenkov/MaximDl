using System;
using System.IO;
using System.Linq;
using Ganss.IO;

namespace MaximDl
{
    class Program
    {
        static void Main(string[] args)
        {
            ParseArgs(args);
        }

        private static DataSetInfo ParseArgs(string[] args)
            => args switch
            {
                _ when args.Length == 1 => new DataSetInfo(Glob.Expand(args[0])),
                _ when args.Length == 3 => new DataSetInfo(Glob.Expand(args[0]), Glob.Expand(args[1]), Glob.Expand(args[2])),
                _ => DataSetInfo.Empty
            };
        private static void Calibrate()
        {
            using var app = MaxImDlApp.Acquire();
            using var doc = MaxImDlDoc.Acquire();
            app.CalClear();
            app.CalMedianBias = true;
            app.CalMedianDark = true;

            app.CalAddBias(@"C:\NOT_July2019\Correct\Data\Night_1\MAXIJ1820-POL_bias_B_0001.fits");
            app.CalAddDark(@"C:\NOT_July2019\Correct\Data\Night_1\MAXIJ1820-POL_dark_B_0001.fits");
            app.CalSet();

            doc.OpenFile(@"C:\NOT_July2019\Correct\Data\Night_1\MAXIJ1820-POL_B_0001.fits");
            doc.Calibrate();
            doc.Bin(2);
        }

      
    }
}

