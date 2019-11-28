using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    internal sealed class SimpleCsvWriter : IDisposable
    {

        private readonly TextWriter _internalWriter;
        private readonly string? _path;

        public SimpleCsvWriter(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be empty/null.", nameof(path));

            _internalWriter = new StreamWriter(
                new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write),
                Encoding.UTF8);
            _path = path;
        }

        public SimpleCsvWriter(Stream str) 
            => _internalWriter = new StreamWriter(str, Encoding.UTF8, leaveOpen: true);

        public async Task Dump(
            IReadOnlyList<ResultItem> data,
            IReadOnlyList<(DateTimeOffset Date, double Mjd, int Id)> metaData,
            Options opts,
            CoordDesc desc)
        {
            Program.Info($"Saving {(_path ?? "file")}");
            if (opts.PrintHeader)
            {
                await _internalWriter.WriteLineAsync($"{opts.CommentChar} First ray approx. coords: {desc.FirstPosition}");
                await _internalWriter.WriteLineAsync($"{opts.CommentChar} Second ray approx. coords: {desc.SecondPosition}");
                await _internalWriter.WriteLineAsync($"{opts.CommentChar} Aperture settings: {desc.Aperture}");
            }

            var header = new[]
            {
                "Id",
                "MJD",
                "Flux_1",
                "SNR_1",
                "StDev_1",
                "Flux_2",
                "SNR_2",
                "StDev_2",
                //"Obj1" // For compatibility with older {Dipol2Red} package
                "DeltaMag"
            };

            try
            {
                await _internalWriter.WriteAsync(header.Aggregate((old, @new) => old + "," + @new));

            }
            catch (Exception e)
            {
                Program.Warn($"Failed to generate CSV string: {e.Message}");
            }

            for (var i = 0; i < data.Count; i++)
            {
                try
                {
                    var item = metaData[i];
                    var dataItem = data[i];
                    var strs = new[]
                    {
                        $"{Environment.NewLine}{item.Id:D4}",
                        $"{item.Mjd:F6}",
                        $"{dataItem.FirstResult.FullIntensity:E10}",
                        $"{dataItem.FirstResult.SNR:F6}",
                        $"{dataItem.FirstResult.StdDev:E6}",
                        $"{dataItem.SecondResult.FullIntensity:E10}",
                        $"{dataItem.SecondResult.SNR:F6}",
                        $"{dataItem.FirstResult.StdDev:E6}",
                        $"{-2.5 * Math.Log10(dataItem.FirstResult.FullIntensity / dataItem.SecondResult.FullIntensity):E10}"
                    };
                    await _internalWriter.WriteAsync(strs.Aggregate((old, @new) => old + "," + @new));
                }
                catch (Exception e)
                {
                    Program.Warn($"Failed to generate CSV string: {e.Message}");
                }
            }

            await _internalWriter.FlushAsync();
        }

        public void Dispose()
        {
            _internalWriter?.Dispose();
        }
    }
}
