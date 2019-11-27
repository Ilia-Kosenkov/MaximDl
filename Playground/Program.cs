using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
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
                            return DateTimeOffset.TryParse(
                                d.GetFITSKey(@"DATE-OBS") as string,
                                DateTimeFormatInfo.InvariantInfo,
                                DateTimeStyles.AssumeUniversal,
                                out var dt)
                                ? dt
                                : default;
                        }
                    })
                    .Select((x, j) => (Date: x, Mjd: DateToMjd(x), Id: j))
                    .OrderBy(x => x.Mjd)
                    .ToList();

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

                var jobs = GenerateOutPaths(starDescs)
                    .Zip(results)
                    .Select(async x =>
                    {
                        var (first, (_, value)) = x;
                        using var str = new SimpleCsvWriter(first);
                        await str.Dump(value, dates);
                    })
                    .ToArray();

                var id = 0;
                foreach (var item in results)
                {
                    ShowResults(++id, item.Value, dates);
                }

                await Task.WhenAll(jobs);
            }


            app.CloseAll();
        }

        private static IEnumerable<string> GenerateOutPaths(IReadOnlyList<CoordDesc> starDescs)
        {
            if (!Directory.Exists(@"test"))
                Directory.CreateDirectory(@"test");
            return starDescs.Select((x, locId) => $@"test\{locId}_{x.Aperture.Aperture}.csv").Select(Path.GetFullPath);
        }

        private static Dictionary<CoordDesc, ResultItem> Measure(MaxImDlDoc doc, IEnumerable<CoordDesc> descs) =>
            descs.ToDictionary(key => key,
                desc =>
                    new ResultItem(
                        doc.CalcInformation(desc.FirstPosition.X, desc.FirstPosition.Y, desc.Aperture),
                        doc.CalcInformation(desc.SecondPosition.X, desc.SecondPosition.Y, desc.Aperture)));

        private static Dictionary<CoordDesc, List<ResultItem>> MeasureAll(
            MaxImDlDocCollection docs, 
            IReadOnlyList<CoordDesc> descs)
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

        private static void ShowResults(
            int id, 
            IReadOnlyList<ResultItem> data,
            IReadOnlyList<(DateTimeOffset Date, double Mjd, int Id)> metaData)
        {
            static string GenerateString(ObjectInfo info) 
                => $"{info.X, 6:F2} | {info.Y, 6:F2} | {info.FullIntensity, 12:E5} | {info.SNR, 6:F2}";



            var header = $"{("X"), 6} | {("Y"), 6} | {("Int"), 12} | {("SNR"), 6}";
            var @break = new string('-', 2 * header.Length + 24);
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
                {
                    var localMetaData = metaData[i];
                    Console.WriteLine($"{localMetaData.Id + 1:000} " +
                                      $"| {GenerateString(data[localMetaData.Id].FirstResult)} " +
                                      $"| {GenerateString(data[localMetaData.Id].SecondResult)} " +
                                      $"| {-2.5 * Math.Log10(data[localMetaData.Id].FirstResult.FullIntensity / data[localMetaData.Id].SecondResult.FullIntensity),10:F6}");
                }
            }
        }

        public static void Warn(string s)
        {
            lock (Locker)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] >> {s}");
                Console.ForegroundColor = color;
            }
        }

        public static void Info(string s)
        {
            lock (Locker)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] >> {s}");
                Console.ForegroundColor = color;
            }
        }

        private static readonly DateTimeOffset MjdZero = new DateTimeOffset(1858, 11, 17, 0, 0, 0, TimeSpan.Zero);

        private static double DateToMjd(DateTimeOffset date)
            => (date - MjdZero).TotalDays;
    }
}
