using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using CommandLine;
using MaxImDL;

namespace Playground
{
    internal class Program
    {
        private static readonly object Locker = new object();
        private static async Task<int> Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            using var parser = new Parser();
            var result = parser.ParseArguments<Options>(args);

            return await result.MapResult(
                async opts =>
                {
                    await StartApp(opts);
                    return 0;
                },
                async err =>
                {
                    await Console.Error.WriteLineAsync(CommandLine.Text.HelpText.AutoBuild(result));
                    return -1;
                });
        }

        private static async Task StartApp(Options opts)
        {
            using var app = MaxImDlApp.Acquire();

            await Process(app, opts);

            app.CloseAll();
        }

        private static async Task<ConsoleKeyInfo?> ReadKeyAsync(
            CancellationToken token = default,
            TimeSpan delay = default,
            TimeSpan timeOut = default)
        {
            delay = delay == default ? TimeSpan.FromMilliseconds(100) : delay;
            timeOut = timeOut == default ? TimeSpan.MaxValue : timeOut;

            return await SpinOnPropertyAsync(
                () => Console.KeyAvailable,
                token,
                delay,
                timeOut)
                ? Console.ReadKey(true)
                : (ConsoleKeyInfo?) null;
        }

        private static async Task<bool> SpinOnPropertyAsync(Func<bool> condition, CancellationToken token, TimeSpan delay, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;

            if (condition())
                return true;

            while (true)
            {
                if (await Task.Run(() => SpinWait.SpinUntil(condition, delay)))
                    return true;

                var now = DateTime.UtcNow;
                if ((now - start).Ticks > timeout.Ticks || token.IsCancellationRequested)
                    return false;

                await Task.Delay(delay);
            }
        }

        private static async Task Process(MaxImDlApp app, Options opts)
        {
            foreach (var item in opts.EnumerateFiles())
            {
                using var doc = app.CreateDocument();
                doc.OpenFile(item.FullName);
            }

            using var docs = app.Documents;
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

            Info("Press [ESCAPE] to finish selection.");

            var starDescs = await WaitForSelectionAsync(firstDoc);

            Info($"Received {starDescs.Count} stars");
            foreach(var items in starDescs)
                Info($"{items.FirstPosition}; {items.SecondPosition} with {items.Aperture}");

            var results = MeasureAll(docs, starDescs);

            var jobs = GenerateOutPaths(starDescs, opts)
                .Zip(results)
                .Select(async x =>
                {
                    var (first, (cDesc, value)) = x;
                    using var str = new SimpleCsvWriter(first);
                    await str.Dump(value, dates, opts, cDesc);
                })
                .ToArray();

            var id = 0;
            foreach (var item in results)
                ShowResults(++id, item.Value, dates);

            await Task.WhenAll(jobs);
        }

        private static async Task<List<CoordDesc>> WaitForSelectionAsync(MaxImDlDoc firstDoc)
        {
            var i = 1;
            var starDescs = new List<CoordDesc>();

            var cts = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                var key = await ReadKeyAsync(cts.Token);
                while (key?.Key != ConsoleKey.Escape && !cts.IsCancellationRequested)
                {
                    key = await ReadKeyAsync(cts.Token);
                }

                return key;
            }, cts.Token);

            while (true)
            {

                Info($"Awaiting user input: first ray of star # {i}.");
                while (true)
                {
                    var awaitedTask = await Task.WhenAny(firstDoc.AwaitMouseNewClickEventAsync(cts.Token), task);
                    
                    if (awaitedTask is Task<bool> clickTask
                        && clickTask.IsCompletedSuccessfully
                        && firstDoc.MouseUp)
                        break;

                    if (awaitedTask is Task<ConsoleKeyInfo?> cancelTask
                        && cancelTask.IsCompletedSuccessfully
                        && cancelTask.Result?.Key == ConsoleKey.Escape)
                    {
                        cts.Cancel();
                        Warn("Input finished.");
                        return starDescs;
                    }
                }

                var firstRay = firstDoc.MousePosition;
                var firstRing = new Ring(
                    firstDoc.MouseRadius,
                    firstDoc.MouseGapWidth,
                    firstDoc.MouseAnnulusWidth);
                Info($"Aperture {firstRing} at ({firstRay.X},{firstRay.Y})");

                Info($"Awaiting user input: second ray of star # {i}.");
                while (true)
                {
                    var awaitedTask = await Task.WhenAny(firstDoc.AwaitMouseNewClickEventAsync(cts.Token), task);

                    if (awaitedTask is Task<bool> clickTask
                        && clickTask.IsCompletedSuccessfully
                        && firstDoc.MouseUp)
                        break;

                    if (awaitedTask is Task<ConsoleKeyInfo?> cancelTask
                        && cancelTask.IsCompletedSuccessfully
                        && cancelTask.Result?.Key == ConsoleKey.Escape)
                    {
                        cts.Cancel();
                        Warn("Input finished.");
                        return starDescs;
                    }
                }

                var secondRay = firstDoc.MousePosition;
                var secondRing = new Ring(
                    firstDoc.MouseRadius,
                    firstDoc.MouseGapWidth,
                    firstDoc.MouseAnnulusWidth);
                Info($"Aperture {secondRing} at ({secondRay.X},{secondRay.Y})");


                if (secondRing != firstRing)
                    Warn($"Star {i}: aperture settings for different rays are not equal. Using aperture of first ray.");

                starDescs.Add(new CoordDesc(firstRay, secondRay, firstRing));
                i++;
            }
        }

        private static IEnumerable<string> GenerateOutPaths(IReadOnlyList<CoordDesc> starDescs, Options opts)
        {
            var root = opts.OutFolderPath();

            return starDescs
                .Select((x, locId) => $"star_{locId}_{x.Aperture}{opts.Suffix}.csv")
                .Select(x => Path.Combine(root, x));
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
                => $"{info.X, 6:F2} | {info.Y, 6:F2} | {info.FullIntensity, 12:E5} | {info.SNR, 7:F2}";



            var header = $"{("X"), 6} | {("Y"), 6} | {("Int"), 12} | {("SNR"), 7}";
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
                Console.ForegroundColor = ConsoleColor.Red;
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
