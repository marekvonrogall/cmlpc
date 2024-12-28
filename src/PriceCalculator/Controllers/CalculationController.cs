using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PriceCalculator.Services;

namespace PriceCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly GeocodeService _geocodeService;
        private readonly DistanceService _distanceService;
        private readonly IHostEnvironment _env;

        public CalculationController(GeocodeService geocodeService, DistanceService distanceService, IHostEnvironment env)
        {
            _geocodeService = geocodeService;
            _distanceService = distanceService;
            _env = env;
        }

        [HttpGet("getPossibleEventDurations")]
        public async Task<IActionResult> GetPossibleEventsDurations(string plzRouteEnd)
        {
            var distanceInKm = await getDistance(plzRouteEnd);
            if (distanceInKm == null) return BadRequest("Invalid postal code. / failed to calculate distance.");

            if(distanceInKm < 150)
            {
                return Ok(new {PossibleEventDurationsInHours = new[] { 3, 5, 8 }});
            } else return Ok(new {PossibleEventDurationsInHours = new[] { 5, 8 }});
        }

        public async Task<int?> getDistance(string plzRouteEnd)
        {
            var endCoords = _geocodeService.GetCoordinates(plzRouteEnd);

            if (endCoords == null)return null;

            var distanceInKm = await _distanceService.GetDistanceInKmAsync(endCoords.Value.lat.ToString(), endCoords.Value.lon.ToString());

            if (distanceInKm == null) return null;

            return (int)Math.Round(distanceInKm.Value);
        }

        private ConfigService ReadConfig()
        {
            var configFilePath = Path.Combine(_env.ContentRootPath, "config", "config.json");
            
            if (!System.IO.File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Config file not found.");
            }

            var configJson = System.IO.File.ReadAllText(configFilePath);
            return JsonSerializer.Deserialize<ConfigService>(configJson);
        }

        [HttpGet("showConfig")]
        public IActionResult ShowConfig()
        {
            var configFilePath = Path.Combine(_env.ContentRootPath, "config", "config.json");
            
            if (!System.IO.File.Exists(configFilePath))
            {
                throw new FileNotFoundException("Config file not found.");
            }

            var configJson = System.IO.File.ReadAllText(configFilePath);
            return Ok(configJson);
        }   

        [HttpGet("getPriceFor3Hours")]
        public IActionResult GetPriceFor3Hours()
        {
            var config = ReadConfig();
            return Ok(new{config.Grundpreise.PriceBase_3hours.Value});
        }
    }
}
