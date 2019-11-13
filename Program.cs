using System;

namespace MaximDl
{
    class Program
    {
        static void Main(string[] args)
        {
            using var app = MaxImDlApp.Acquire();
            using var doc = MaxImDlDoc.Acquire();
            app.CalClear();
            app.CalMedianBias = true;
            app.CalMedianDark = true;
            app.CalAddBias(@"E:\NOT\Dipol-1\20190722_mod\MAXIJ1820-POL\MAXIJ1820-POL_bias_B_0001.fits");
            app.CalAddDark(@"E:\NOT\Dipol-1\20190722_mod\MAXIJ1820-POL\MAXIJ1820-POL_dark_B_0001.fits");
            app.CalSet();

            doc.OpenFile(@"E:\NOT\Dipol-1\20190722_mod\MAXIJ1820-POL\MAXIJ1820-POL_B_0001_test.fits");
            doc.Calibrate();
            doc.Bin(2);
            doc.SaveFile(@"E:\NOT\Dipol-1\20190722_mod\MAXIJ1820-POL\TEST.fits", 3);
        }
    }
}

