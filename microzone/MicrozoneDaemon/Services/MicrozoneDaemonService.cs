using System.Diagnostics;

namespace MicrozoneDaemon
{
    public class MicrozoneDaemonService : BackgroundService, IDisposable
    {
        private readonly string statusRequest = "";
        private readonly string runtimePath = "";
        private readonly ILogger<MicrozoneDaemonService> logger;

        private readonly HttpClient httpClient = new HttpClient();
        private Process? microzoneServiceProcess = null;

        private bool isDisposed = false;
        private const int tickIntervalMS = 1000;
        private const int waittingForServiceResumeMS = 10000;

        private enum MicrozoneStatus
        {
            EXCEPTION,
            SERVICE_OFFLINE,
            SERVICE_ONLINE
        }

        public MicrozoneDaemonService(IConfiguration configuration, ILogger<MicrozoneDaemonService> logger)
        {
            this.logger = logger;

            httpClient.BaseAddress = new Uri(configuration["MicrozoneCenter:BaseAddress"]);

            statusRequest = configuration["MicrozoneCenter:StatusRequest"];
            runtimePath = configuration["MicrozoneCenter:RuntimePath"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                MicrozoneStatus status = await GetServiceStatus(stoppingToken);
                if (status != MicrozoneStatus.SERVICE_ONLINE)
                {
                    logger.LogWarning($"Microzone service status is {status}");
                    await RebootServiceProcess(stoppingToken);
                }

                await Task.Delay(tickIntervalMS, stoppingToken);
            }
        }

        private async Task<MicrozoneStatus> GetServiceStatus(CancellationToken stoppingToken)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(statusRequest, stoppingToken);
                if (response.IsSuccessStatusCode)
                    return MicrozoneStatus.SERVICE_ONLINE;
                return MicrozoneStatus.SERVICE_OFFLINE;
            }
            catch (Exception ex)
            {
                logger.LogError($"Call status api error: {ex.Message}");
                return MicrozoneStatus.EXCEPTION;
            }
        }

        private async Task RebootServiceProcess(CancellationToken stoppingToken)
        {
            try
            {
                if (microzoneServiceProcess != null &&
                    !microzoneServiceProcess.HasExited)
                {
                    microzoneServiceProcess.Kill();
                }
            }
            catch {}
            microzoneServiceProcess = null;

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = runtimePath,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    UseShellExecute = false
                };

                microzoneServiceProcess = new Process();
                microzoneServiceProcess.StartInfo = psi;
                microzoneServiceProcess.Start();

                await Task.Delay(waittingForServiceResumeMS, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!isDisposed)
            {
                microzoneServiceProcess?.Dispose();
                httpClient?.Dispose();
                isDisposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~MicrozoneDaemonService()
        {
            Dispose();
        }
    }

    public static class MicrozoneDaemonServiceExtensions
    {
        public static IServiceCollection AddMicrozoneDaemonService(this IServiceCollection self)
            => self.AddHostedService<MicrozoneDaemonService>();
    }
}