using System;

namespace MaximDl
{
    class Program
    {
        static void Main(string[] args)
        {
            using var app = MaxImDlApp.Acquire();
            app.CalClear();
            app.CalAddBias(@"E:\NOT\Dipol-1\20190722_mod\MAXIJ1820-POL");
            app.CalSet();
        }
    }
}

