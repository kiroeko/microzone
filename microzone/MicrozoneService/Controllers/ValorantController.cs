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
        public ApiResult<bool> Get()
        {
            if (valorantService == null)
                return ApiResult(false);
            return ApiResult(valorantService.IsValorantRunning);
        }
    }
}