using System;
using System.Threading.Tasks;

namespace MaximDl
{
    class Program
    {
        static async Task Main(string[] args)
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

            while(doc.MouseNewClick == 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }

            Console.WriteLine($"({doc.MouseX}, {doc.MouseY})");
            var result = doc.CalcInformation(doc.MouseX, doc.MouseY, new Ring(10, 2, 10));

        }

      
    }
}

