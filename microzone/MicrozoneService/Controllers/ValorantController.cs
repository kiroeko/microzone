using Microsoft.AspNetCore.Mvc;

namespace MicrozoneService
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValorantController : Controller
    {
        private ValorantService valorantService;

        public ValorantController(ValorantService valorantService)
        {
            this.valorantService = valorantService;
        }

        [HttpGet]
        public ApiResult<bool> GetIsValorantRunning()
        {
            if (valorantService == null)
                return ApiResult(false);
            return ApiResult(valorantService.IsValorantRunning);
        }

        [HttpGet("stop")]
        public ApiResult<bool> Stop()
        {
            if (valorantService == null)
                return ApiResult(false);
            return ApiResult(valorantService.StopValorant());
        }
    }
}