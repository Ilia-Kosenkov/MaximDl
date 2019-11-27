using System;
using MaximDl;

namespace Playground
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using var app = MaxImDlApp.Acquire();

            using var doc = app.CreateDocument();
            doc.OpenFile("");
            Console.ReadKey();
            doc.Close();
        }
    }
}
