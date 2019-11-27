using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MaximDl;

namespace Playground
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            using var app = MaxImDlApp.Acquire();
            var pathGlob = @"C:\NOT_July2019\Correct\Data\Polarization\Night_1\binned_X_3\V\*.fits";
            var files = Ganss.IO.Glob.Expand(pathGlob);
            foreach(var item in files.Take(8))
            {
                using (var doc = app.CreateDocument())
                    doc.OpenFile(item.FullName);
            }

            using (var docs = app.Documents)
            {
                using var firstDoc = docs[0];
                firstDoc.BringToTop();
                var count = 0;
                (short X, short Y) firstRay = default;
                (short X, short Y) secondRay = default;

                while(count < 2)
                {
                    // Infinite wait time
                    await firstDoc.AwaitMouseNewClickEventAsync();
                    if(firstDoc.MouseDown)
                    {
                        if(++count == 1)
                            firstRay = firstDoc.MousePosition;
                        else
                            secondRay = firstDoc.MousePosition;
                    }
                }

                var aperture = new Ring(
                    (ushort)firstDoc.MouseRadius, 
                    (ushort)firstDoc.MouseGapWidth, 
                    (ushort)firstDoc.MouseAnnulusWidth);

                var stats = MeasureAll(docs, firstRay, secondRay, aperture);

                ShowResults(stats.FirstRay, stats.SecondRay);
            }


            app.CloseAll();
        }

        private static (List<ObjectInfo> FirstRay, List<ObjectInfo> SecondRay) MeasureAll(MaxImDlDocCollection docs, 
            (short X, short Y) firstRay,
            (short X, short Y) secondRay,
            Ring aperture)
        {
            var firstRayData = new List<ObjectInfo>(docs.Count);
            var secondRayData = new List<ObjectInfo>(docs.Count);

            foreach(var doc in docs) using(doc)
            {
                firstRayData.Add(doc.CalcInformation(firstRay.X, firstRay.Y, aperture));
                secondRayData.Add(doc.CalcInformation(secondRay.X, secondRay.Y, aperture));
            }

            return (firstRayData, secondRayData);
        }
        
        private static void ShowResults(List<ObjectInfo> firstRay, List<ObjectInfo> secondRay)
        {
            static string GenerateString(ObjectInfo info)
            {
                return $"{info.X, 6:F2} | {info.Y, 6:F2} | {info.FullIntensity, 12:E3} | {info.SNR, 6:F2}";
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            if(firstRay.Count != secondRay.Count)
                Console.Error.WriteLine("Length mismatch!");

            var header = $"{("X"), 6} | {("Y"), 6} | {("Int"), 12} | {("SNR"), 6}";
            Console.WriteLine(string.Format($"    | {{0, {header.Length}}} | {{1, {header.Length}}} |", "Ray 1", "Ray 2"));
            Console.WriteLine(new string('-', 2 * header.Length + 20));
            Console.WriteLine($" Id | {header} | {header} | dMag");
            Console.WriteLine(new string('-', 2 * header.Length + 20));

            for(var i = 0; i < firstRay.Count; i++)
                Console.WriteLine($"{i:000} | {GenerateString(firstRay[i]), 40} | {GenerateString(secondRay[i]), 40} | {-2.5 * Math.Log10(firstRay[i].FullIntensity / secondRay[i].FullIntensity),8:F3}");
        }
    }
}
