using System.Diagnostics;

namespace MicrozoneService
{
    public class ValorantService : BackgroundService
    {
        private readonly string processName = "";
        private bool isValorantRunning = false;
        private const int tickIntervalMS = 1000;

        public ValorantService(IConfiguration configuration)
        {
            processName = configuration["ValorantService:ProcessName"];
        }

        public bool StopValorant()
        {
            bool killedSuccessful = false;
            Process? p = null;
            try
            {
                p = GetValorantProcess();
                if (p != null && !p.HasExited)
                {
                    p.Kill();
                    killedSuccessful = true;
                }
            }
            finally
            {
                p?.Dispose();
            }

            return killedSuccessful;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Process? p = null;
                try
                {
                    p = GetValorantProcess();
                    bool currentIsValorantRunning = p != null && !p.HasExited;

                    if (isValorantRunning != currentIsValorantRunning)
                    {
                        if(currentIsValorantRunning)
                            MouseUtil.SetAcceleration(0);
                        else
                            MouseUtil.SetAcceleration(1);
                        isValorantRunning = currentIsValorantRunning;
                    }
                }
                finally
                {
                    p?.Dispose();
                }

                await Task.Delay(tickIntervalMS, stoppingToken);
            }
        }

        private Process? GetValorantProcess()
        {
            return Process.GetProcessesByName(processName).FirstOrDefault();
        }
    }

    public static class ValorantServiceExtensions
    {
        public static IServiceCollection AddValorantService(this IServiceCollection self)
            => self.AddHostedService<ValorantService>();
    }
}