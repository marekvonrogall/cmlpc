using Microsoft.AspNetCore.Mvc;
using PriceCalculator.Services;

namespace PriceCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly GeocodeService _geocodeService;
        private readonly DistanceService _distanceService;

        public CalculationController(GeocodeService geocodeService, DistanceService distanceService)
        {
            _geocodeService = geocodeService;
            _distanceService = distanceService;
        }

        [HttpGet("getDistance")]
        public async Task<IActionResult> GetDistance(string plzRouteEnd)
        {
            var distanceInKm = await getDistance(plzRouteEnd);
            if (distanceInKm == null) return BadRequest("Invalid postal code. / failed to calculate distance.");

            return Ok(new { distanceInKm });
        }

        public async Task<int?> getDistance(string plzRouteEnd)
        {
            var endCoords = _geocodeService.GetCoordinates(plzRouteEnd);

            if (endCoords == null)return null;

            var distanceInKm = await _distanceService.GetDistanceInKmAsync(endCoords.Value.lat.ToString(), endCoords.Value.lon.ToString());

            if (distanceInKm == null) return null;

            return (int)Math.Round(distanceInKm.Value);
        }
    }
}
