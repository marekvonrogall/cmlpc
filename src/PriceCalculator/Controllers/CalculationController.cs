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

        [HttpGet("getPossibleEventDurations")]
        public async Task<IActionResult> GetPossibleEventDurations(string plzRouteEnd)
        {
            var result = await CalculatePossibleEventDurations(plzRouteEnd);

            if (result[0] == 0) return BadRequest("Invalid postal code. Is OSRM running?");

            return Ok(new { PossibleEventDurationsInHours = result });
        }

        private async Task<int[]> CalculatePossibleEventDurations(string plzRouteEnd)
        {
            var distanceInKm = await getDistance(plzRouteEnd);
            if (distanceInKm == null) return [0];

            var config = ReadConfig();

            if (distanceInKm < config.Entfernungseinstellungen.RouteCalculation_MaxRadiusFor3HourEvent.Value)
            {
                return [3, 5, 8];
            }

            return [5, 8];
        }

        [HttpGet("calculateEventCosts")]
        public async Task<IActionResult> CalculateEventCosts(string plzRouteEnd, string eventType)
        {
            var config = ReadConfig();

            double kilometerCost;
            double hotelCosts = 0;

            switch (eventType)
            {
                case "Rollendes-Fotozimmer":
                    kilometerCost = config.Kilometerpreise.PricePerKm_Bus.Value;
                    break;
                case "Foto-Discokugel":
                    kilometerCost = config.Kilometerpreise.PricePerKm_Disco.Value;
                    break;
                case "Video-Kaleidoskop":
                    kilometerCost = config.Kilometerpreise.PricePerKm_Kaleido.Value;
                    break;
                default:
                    return BadRequest("Invalid event type.");
            }

            var distanceInKm = await getDistance(plzRouteEnd);
            if (distanceInKm == null) return BadRequest("Invalid postal code. Is OSRM running?");

            var possibleEventDurations = await CalculatePossibleEventDurations(plzRouteEnd);

            if (distanceInKm >= config.Entfernungseinstellungen.RouteCalculation_MinKmForAdditionalHotelCost.Value)
            {
                hotelCosts = config.Mitarbeiterkosten.PricePerNight_Hotel.Value;
            }

            distanceInKm *= 2; // Hin- und Rückfahrt
            double travelCosts = kilometerCost * distanceInKm.Value;
            travelCosts = Math.Round(travelCosts / 10) * 10; //Auf 10€ runden

            var priceDetails = new List<object>();

            foreach (var duration in possibleEventDurations)
            {
                double basePrice;
                switch (duration)
                {
                    case 3:
                        basePrice = config.Grundpreise.PriceBase_3hours.Value;
                        break;
                    case 5:
                        basePrice = config.Grundpreise.PriceBase_5hours.Value;
                        break;
                    case 8:
                        basePrice = config.Grundpreise.PriceBase_8hours.Value;
                        break;
                    default:
                        return BadRequest("Invalid event duration.");
                }

                var totalCost = basePrice + travelCosts + hotelCosts;
                //var discount = totalCost * config.Rabatte.SaleAmountInPercent.Value;

                //totalCost -=

                priceDetails.Add(new
                {
                    eventDuration = duration,
                    basePrice,
                    travelDistance = distanceInKm, //Hin- und Rückfahrt
                    travelDuration = distanceInKm /100 *1.5,
                    travelCosts,
                    hotelCosts,
                    totalCost,
                    vat = totalCost /100 *config.Mehrwertsteuer.ValuedAddedTax.Value
                });
            }

            return Ok(priceDetails);
        }
    }
}
