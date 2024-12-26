using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PriceCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly GeocodeService _geocodeService;

        public CalculationController(HttpClient httpClient, GeocodeService geocodeService)
        {
            _httpClient = httpClient;
            _geocodeService = geocodeService;
        }

        [HttpGet("getDistance")]
        public async Task<IActionResult> GetDistance(string plzRouteEnd)
        {
            var endCoords = _geocodeService.GetCoordinates(plzRouteEnd);

            if (endCoords == null)
            {
                return BadRequest("Invalid postal code provided.");
            }

            //Alfter: 50.697864, 7.018537
            var startLat = "50.697864";
            var startLon = "7.018537";
            var endLat = endCoords.Value.lat;
            var endLon = endCoords.Value.lon;

            var url = $"http://osrm:5000/route/v1/driving/{startLon},{startLat};{endLon},{endLat}?overview=false";

            try 
            {
                var response = await _httpClient.GetStringAsync(url);
                var jsonResponse = JsonDocument.Parse(response);
                var distance = jsonResponse.RootElement
                    .GetProperty("routes")[0]
                    .GetProperty("legs")[0]
                    .GetProperty("distance")
                    .GetDouble();

                var distanceInKm = distance / 1000;
                
                return Ok(new { distanceInKm });
            }
            catch (HttpRequestException e)
            {
                return StatusCode(500, $"Error: {e.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected Error: {ex.Message}");
            }
        }
    }
}
