using System.Diagnostics;

namespace MicrozoneService
{
    public class ValorantService : BackgroundService
    {
        public bool IsValorantRunning { get; private set; }
        private readonly string[] targetProcessNames;
        private const int tickIntervalMS = 1000;

        public ValorantService(IConfiguration configuration)
        {
            targetProcessNames = configuration.GetSection("ValorantService:ProcessNames").Get<string[]>();
        }

        public bool StopValorant()
        {
            bool killedSuccessful = true;
            foreach (var processName in targetProcessNames)
            {
                Process[] ps = Process.GetProcessesByName(processName);
                try
                {
                    foreach (Process p in ps)
                    {
                        if (p != null && !p.HasExited)
                        {
                            try
                            {
                                p.Kill();
                            }
                            catch
                            {
                                killedSuccessful = false;
                            }
                        }
                    }
                }
                finally
                {
                    foreach (Process p in ps)
                    {
                        p?.Dispose();
                    }
                }
            }

            return killedSuccessful;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool isRunning = IfValorantRunning();
                    if (IsValorantRunning != isRunning)
                    {
                        if (isRunning)
                            MouseUtil.SetAcceleration(0);
                        else
                            MouseUtil.SetAcceleration(1);
                        IsValorantRunning = isRunning;
                    }
                }
                catch {}

                await Task.Delay(tickIntervalMS, stoppingToken);
            }
        }

        private bool IfValorantRunning()
        {
            foreach (var processName in targetProcessNames)
            {
                Process[] ps = Process.GetProcessesByName(processName);
                try
                {
                    foreach (Process p in ps)
                    {
                        if (p != null && !p.HasExited)
                            return true;
                    }
                }
                finally
                {
                    foreach (Process p in ps)
                    {
                        p?.Dispose();
                    }
                }
            }

            return false;
        }
    }

    public static class ValorantServiceExtensions
    {
        public static IServiceCollection AddValorantService(this IServiceCollection self)
        {
            return self.AddSingleton<ValorantService>().
                AddHostedService(provider => provider.GetService<ValorantService>());
        }
    }
}