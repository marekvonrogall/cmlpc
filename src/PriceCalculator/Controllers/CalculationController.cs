using Microsoft.AspNetCore.Mvc;

namespace PriceCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        [HttpGet(Name = "calculate")]
        public string Get()
        {
            return "works";
        }
    }
}
