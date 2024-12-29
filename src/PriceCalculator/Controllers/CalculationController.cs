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
        public async Task<IActionResult> CalculateEventCosts(string plzRouteEnd, string eventType, DateTime eventDate)
        {
            var config = ReadConfig();
            if (eventDate == default) return BadRequest("No event date specified.");
            if (eventDate < DateTime.Now) return BadRequest("Event date is in the past.");

            int eventDayOfYear = DateService.GetDayOfYear(eventDate);

            int saleDateStartBookingSaleDay = DateService.GetDayOfYear(config.Rabatte.SaleDateStart_BookingSale.Value);
            int saleDateEndBookingSaleDay = DateService.GetDayOfYear(config.Rabatte.SaleDateEnd_BookingSale.Value);
            int saleDateStartEventSaleDay = DateService.GetDayOfYear(config.Rabatte.SaleDateStart_EventSale.Value);
            int saleDateEndEventSaleDay = DateService.GetDayOfYear(config.Rabatte.SaleDateEnd_EventSale.Value);

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

                double totalCostWithoutDiscount = basePrice + travelCosts + hotelCosts;
                double totalCost = totalCostWithoutDiscount;

                var discounts = new List<object>();

                double eventBasedDiscount = 0;
                if (DateService.IsDateBetween(eventDayOfYear, saleDateStartEventSaleDay, saleDateEndEventSaleDay)) //Eventdatum liegt im Rabattzeitraum
                {
                    eventBasedDiscount = totalCost / 100 * config.Rabatte.SaleAmountInPercent_EventSale.Value;
                    discounts.Add(new
                    {
                        eventBasedDiscountName = config.Rabatte.SaleName_EventSale.Value,
                        eventBasedDiscountStart = DateService.GetDateTimeWithYear(config.Rabatte.SaleDateStart_EventSale.Value, "start", eventDate),
                        eventBasedDiscountEnd = DateService.GetDateTimeWithYear(config.Rabatte.SaleDateEnd_EventSale.Value, "end", eventDate),
                        totalCostBeforeDiscount = totalCost,
                        totalCostAfterDiscount = totalCost - eventBasedDiscount,
                        eventBasedDiscountPercent = config.Rabatte.SaleAmountInPercent_EventSale.Value,
                        eventBasedDiscount
                    });
                }

                totalCost -= eventBasedDiscount;

                double bookingBasedDiscount = 0;
                if (DateService.IsDateBetween(eventDayOfYear, saleDateStartBookingSaleDay, saleDateEndBookingSaleDay)) //Buchung liegt im Rabattzeitraum
                {
                     bookingBasedDiscount = totalCost / 100 * config.Rabatte.SaleAmountInPercent_BookingSale.Value;
                     discounts.Add(new
                     {
                         bookingBasedDiscountName = config.Rabatte.SaleName_BookingSale.Value,
                         bookingBasedDiscountEnd = DateService.GetDateTimeWithYear(config.Rabatte.SaleDateEnd_BookingSale.Value, "end", eventDate), //Buchungen bis DATUM erhalten Rabatt
                         totalCostBeforeDiscount = totalCost,
                         totalCostAfterDiscount = totalCost - bookingBasedDiscount,
                         bookingBasedDiscountPercent = config.Rabatte.SaleAmountInPercent_BookingSale.Value,
                         bookingBasedDiscount
                     });
                }

                totalCost -= bookingBasedDiscount;

                priceDetails.Add(new
                {
                    eventDuration = duration,
                    basePrice,
                    travelDistance = distanceInKm,
                    travelDuration = distanceInKm /100 *1.5,
                    travelCosts,
                    hotelCosts,
                    totalCostWithoutDiscount,
                    totalCost,
                    discounts,
                    vat = totalCost /100 *config.Mehrwertsteuer.ValuedAddedTax.Value
                });
            }

            return Ok(priceDetails);
        }
    }
}
