using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MaximDl;

namespace Playground
{
    internal class Program
    {
        private static readonly object Locker = new object();
        private static async Task Main()
        {
            const string pathGlob = @"C:\NOT_July2019\Correct\Data\Polarization\Night_1\binned_X_3\V\*.fits";
            const int n = 2;

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

                // DATE-OBS
                var dates = docs.Select(d =>
                {
                    using (d)
                    {
                        return d.GetFITSKey(@"DATE=OBS");
                    }
                }).ToList();

                var starDescs = new List<CoordDesc>(n);


                for(var i = 0; i < n; i++)
                {
                    Info($"Awaiting user input: first ray of star {i+1}.");
                    while(true)
                    {
                        await firstDoc.AwaitMouseNewClickEventAsync();
                        if(firstDoc.MouseDown)
                            break;
                    }

                    var firstRay = firstDoc.MousePosition;
                    var firstRing = new Ring(
                        firstDoc.MouseRadius,
                        firstDoc.MouseGapWidth,
                        firstDoc.MouseAnnulusWidth);
                    Info("Input recorded.");

                    Info($"Awaiting user input: second ray of star {i + 1}.");
                    while (true)
                    {
                        await firstDoc.AwaitMouseNewClickEventAsync();
                        if(firstDoc.MouseDown)
                            break;
                    }
                    

                    var secondRay = firstDoc.MousePosition;
                    var secondRing = new Ring(
                        firstDoc.MouseRadius,
                        firstDoc.MouseGapWidth,
                        firstDoc.MouseAnnulusWidth);
                    Info("Input recorded.");


                    if (secondRing != firstRing)
                        Warn($"Star {i}: aperture settings for different rays are not equal. Using aperture of first ray.");

                    starDescs.Add(new CoordDesc(firstRay, secondRay, firstRing));
                }

                var results = MeasureAll(docs, starDescs);

                var id = 0;
                foreach (var item in results)
                {
                    ShowResults(++id, item.Value);
                }
            }


            app.CloseAll();
        }

        private static Dictionary<CoordDesc, ResultItem> Measure(MaxImDlDoc doc, IEnumerable<CoordDesc> descs) =>
            descs.ToDictionary(key => key,
                desc =>
                    new ResultItem(
                        doc.CalcInformation(desc.FirstPosition.X, desc.FirstPosition.Y, desc.Aperture),
                        doc.CalcInformation(desc.SecondPosition.X, desc.SecondPosition.Y, desc.Aperture)));

        private static Dictionary<CoordDesc, List<ResultItem>> MeasureAll(MaxImDlDocCollection docs, IReadOnlyList<CoordDesc> descs)
        {
            var result = descs.ToDictionary(x => x, y => new List<ResultItem>(docs.Count));
            foreach(var doc in docs)
                using (doc)
                {
                    var measurements = Measure(doc, descs);
                    foreach (var desc in descs)
                        result[desc].Add(measurements[desc]);
                }

            return result;
        }

        private static void ShowResults(int id, IReadOnlyList<ResultItem> data)
        {
            static string GenerateString(ObjectInfo info) 
                => $"{info.X, 6:F2} | {info.Y, 6:F2} | {info.FullIntensity, 12:E5} | {info.SNR, 6:F2}";

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            var header = $"{("X"), 6} | {("Y"), 6} | {("Int"), 12} | {("SNR"), 6}";
            var @break = new string('-', 2 * header.Length + 22);
            lock (Locker)
            {
                Console.WriteLine(@break);
                Console.WriteLine($">> Star {id:000}");
                Console.WriteLine(@break);
                Console.WriteLine($"    | {{0, {header.Length}}} | {{1, {header.Length}}} |", "Ray 1", "Ray 2");
                Console.WriteLine(@break);
                Console.WriteLine($" Id | {header} | {header} | dMag");
                Console.WriteLine(@break);

                for (var i = 0; i < data.Count; i++)
                    Console.WriteLine($"{i + 1:000} " +
                                      $"| {GenerateString(data[i].FirstResult)} " +
                                      $"| {GenerateString(data[i].SecondResult)} " +
                                      $"| {-2.5 * Math.Log10(data[i].FirstResult.FullIntensity / data[i].SecondResult.FullIntensity),8:F3}");
            }
        }

        private static void Warn(string s)
        {
            lock (Locker)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] >> {s}");
                Console.ForegroundColor = color;
            }
        }

        private static void Info(string s)
        {
            lock (Locker)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] >> {s}");
                Console.ForegroundColor = color;
            }
        }
    }
}
