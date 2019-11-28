using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaxImDL
{
    internal static class AsyncHelper
    {
        public static async Task<bool> SpinOnPropertyAsync(Func<bool> condition, CancellationToken token, TimeSpan delay, TimeSpan timeout)
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
    }
}
