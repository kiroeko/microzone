using System.Diagnostics;

namespace MicrozoneDaemon
{
    public enum MicrozoneServiceStatus
    {
        EXCEPTION,
        SERVICE_OFFLINE,
        SERVICE_ONLINE
    }

    public class MicrozoneDaemonWorker : BackgroundService, IDisposable
    {
        private readonly string statusRequest = "";
        private readonly string runtimePath = "";
        private readonly string runtimeArguments = "";
        private readonly ILogger<MicrozoneDaemonWorker> logger;

        private readonly HttpClient httpClient = new HttpClient();
        private Process? microzoneServiceProcess = null;

        private bool isDisposed = false;
        private const int tickIntervalMS = 1000;
        private const int waittingForServiceResumeMS = 10000;

        public MicrozoneDaemonWorker(IConfiguration configuration, ILogger<MicrozoneDaemonWorker> logger)
        {
            this.logger = logger;

            httpClient.BaseAddress = new Uri(configuration["MircrozoneCenter:BaseAddress"]);

            statusRequest = configuration["MicrozoneCenter:StatusRequest"];
            runtimePath = configuration["MicrozoneCenter:RuntimePath"];
            runtimeArguments = configuration["MicrozoneCenter:RuntimeArguments"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                MicrozoneServiceStatus status = await GetServiceStatus();
                if (status != MicrozoneServiceStatus.SERVICE_ONLINE)
                {
                    logger.LogWarning($"Microzone service status is {status}");
                    await RebootServiceProcess(stoppingToken);
                }

                await Task.Delay(tickIntervalMS, stoppingToken);
            }
        }

        private async Task<MicrozoneServiceStatus> GetServiceStatus()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(statusRequest);
                if (response.IsSuccessStatusCode)
                    return MicrozoneServiceStatus.SERVICE_ONLINE;
                return MicrozoneServiceStatus.SERVICE_OFFLINE;
            }
            catch (Exception ex)
            {
                logger.LogError($"Call status api error: {ex.Message}");
                return MicrozoneServiceStatus.EXCEPTION;
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
                    Arguments = runtimeArguments,
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

        ~MicrozoneDaemonWorker()
        {
            Dispose();
        }
    }
}