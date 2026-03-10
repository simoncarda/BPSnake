using System;
using System.Collections.Generic;
using System.Text;

namespace BPSnake.Services
{
    internal sealed class GameLoopService: IDisposable
    {
        /// <summary>
        /// Služba pro řízení hlavní herní smyčky.
        /// Implementuje PeriodicTimer a umožňuje bezpečné spuštění/zastavení smyčky.
        /// Zároveň brání překrývání ticků a zbytečně nezatěžuje vlákna.
        /// </summary>
        private PeriodicTimer? _timer;
        private CancellationTokenSource? _cts;

        public async Task StartAsync(Func<Task> tickAsync, int intervalMs)
        {
            // Zastaví běžící smyčku, pokud existuje, aby nedošlo k duplikaci timerů
            Stop();

            if (tickAsync is null) {
                throw new ArgumentNullException(nameof(tickAsync));
            }

            if (intervalMs <= 0) {
                throw new ArgumentOutOfRangeException(nameof(intervalMs));
            }

            // Jednoduchá implementace smyčky: PeriodicTimer + CancellationTokenSource
            var cts = new CancellationTokenSource();
            var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMs));
            _cts = cts;
            _timer = timer;

            try {
                // Jádro smyčky. Vlákno zde asynchronně "spí" (neblokuje se) do dalšího ticku.
                // Při zavolání cts.Cancel() vyvolá WaitForNextTickAsync výjimku OperationCanceledException.
                while (await timer.WaitForNextTickAsync(cts.Token)) {
                    await tickAsync();
                }
            }
            catch (OperationCanceledException) {
                // Smyčka byla zrušena, není třeba nic dělat
            }
            finally {
                // Uklidíme místní proměnné, ale pouze pokud se shodují s aktuálními instancemi, aby nedošlo k nechtěnému zrušení nově vytvořených timerů/cts
                if (ReferenceEquals(_timer, timer)) {
                    _timer = null;
                }

                if (ReferenceEquals(_cts, cts)) {
                    _cts = null;
                }

                timer.Dispose();
                cts.Dispose();
            }
        }

        /// <summary>
        /// Zastaví herní smyčku a uvolní všechny související prostředky.
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _timer?.Dispose();
            _timer = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
