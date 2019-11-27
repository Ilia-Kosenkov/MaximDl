using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MaximDl;

namespace Playground
{
    internal class Program
    {
        private static object _locker = new object();
        private static async Task Main(string[] args)
        {
            var pathGlob = @"C:\NOT_July2019\Correct\Data\Polarization\Night_1\binned_X_3\V\*.fits";
            var n = 2;
            using var app = MaxImDlApp.Acquire();
            var files = Ganss.IO.Glob.Expand(pathGlob);
            foreach(var item in files.Take(8))
            {
                using var doc = app.CreateDocument();
                doc.OpenFile(item.FullName);
            }

            using (var docs = app.Documents)
            {
                using var firstDoc = docs[0];
                firstDoc.BringToTop();

                var starDescs = new List<CoordDesc>(n);


                for(var i = 0; i < n; i++)
                {
                    while(!firstDoc.MouseDown)
                        await firstDoc.AwaitMouseNewClickEventAsync();
                    
                    var firstRay = firstDoc.MousePosition;
                    var firstRing = new Ring(
                        firstDoc.MouseRadius,
                        firstDoc.MouseGapWidth,
                        firstDoc.MouseAnnulusWidth);

                    while (!firstDoc.MouseDown)
                        await firstDoc.AwaitMouseNewClickEventAsync();

                    var secondRay = firstDoc.MousePosition;
                    var secondRing = new Ring(
                        firstDoc.MouseRadius,
                        firstDoc.MouseGapWidth,
                        firstDoc.MouseAnnulusWidth);

                    if (secondRing != firstRing)
                        Warn($"Star {i}: aperture settings for different rays are not equal. Using aperture of first ray.");

                    starDescs.Add(new CoordDesc(firstRay, secondRay, firstRing));
                }

                //var aperture = new Ring(
                //    firstDoc.MouseRadius, 
                //    firstDoc.MouseGapWidth, 
                //    firstDoc.MouseAnnulusWidth);

                //var stats = MeasureAll(docs, firstRay, secondRay, aperture);

                //ShowResults(stats.FirstRay, stats.SecondRay);
            }


            app.CloseAll();
        }

        private static List<ResultItem> MeasureAll(MaxImDlDocCollection docs, CoordDesc desc)
        {
            var result = new List<ResultItem>(docs.Count);

            foreach(var doc in docs) using(doc)
                result.Add(new ResultItem(
                    doc.CalcInformation(desc.FirstPosition.X, desc.FirstPosition.Y, desc.Aperture),
                    doc.CalcInformation(desc.SecondPosition.X, desc.SecondPosition.Y, desc.Aperture)));

            return result;
        }
        
        private static void ShowResults(IReadOnlyList<ObjectInfo> firstRay, IReadOnlyList<ObjectInfo> secondRay)
        {
            static string GenerateString(ObjectInfo info) 
                => $"{info.X, 6:F2} | {info.Y, 6:F2} | {info.FullIntensity, 12:E3} | {info.SNR, 6:F2}";

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            if(firstRay.Count != secondRay.Count)
                Console.Error.WriteLine("Length mismatch!");

            var header = $"{("X"), 6} | {("Y"), 6} | {("Int"), 12} | {("SNR"), 6}";
            Console.WriteLine($"    | {{0, {header.Length}}} | {{1, {header.Length}}} |", "Ray 1", "Ray 2");
            Console.WriteLine(new string('-', 2 * header.Length + 20));
            Console.WriteLine($" Id | {header} | {header} | dMag");
            Console.WriteLine(new string('-', 2 * header.Length + 20));

            for(var i = 0; i < firstRay.Count; i++)
                Console.WriteLine($"{i:000} | {GenerateString(firstRay[i]), 40} | {GenerateString(secondRay[i]), 40} | {-2.5 * Math.Log10(firstRay[i].FullIntensity / secondRay[i].FullIntensity),8:F3}");
        }

        private static void Warn(string s)
        {
            lock (_locker)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] >> {s}");
                Console.ForegroundColor = color;
            }
        }
    }
}
