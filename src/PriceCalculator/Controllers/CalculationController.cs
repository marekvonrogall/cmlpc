using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PriceCalculator.Services;

namespace PriceCalculator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculationController : ControllerBase
    {
        private readonly DistanceService _distanceService;
        private readonly IHostEnvironment _env;

        public CalculationController(DistanceService distanceService, IHostEnvironment env)
        {
            _distanceService = distanceService;
            _env = env;
        }

        private ConfigService? ReadConfig()
        {   
            var configFilePath = Path.Combine(_env.ContentRootPath, "config", "config.json");
            
            if (!System.IO.File.Exists(configFilePath)) return null;

            var configJson = System.IO.File.ReadAllText(configFilePath);
            var config = JsonSerializer.Deserialize<ConfigService>(configJson);

            if (config == null) return null;

            return config;
        }

        private int[] CalculatePossibleEventDurations(ConfigService config, int distanceInKm)
        {
            if (distanceInKm < config.Entfernungseinstellungen.RouteCalculation_MaxRadiusFor3HourEvent.Value)
            {
                return [3, 5, 8];
            }

            return [5, 8];
        }

        [HttpGet("calculateEventCosts")]
        public async Task<IActionResult> CalculateEventCosts(string plzRouteEnd, string eventType, DateTime eventDate, string? additionalOptions)
        {
            var config = ReadConfig();
            if(config == null) return BadRequest("Config file not found / invalid format.");

            if (eventDate == default) return BadRequest("No event date specified.");
            if (eventDate < DateTime.Now) return BadRequest("Event date is in the past.");

            var salePeriods = new
            {
                BookingSale = new { Start = config.Rabatte.SaleDateStart_BookingSale.Value, End = config.Rabatte.SaleDateEnd_BookingSale.Value },
                EventSale = new { Start = config.Rabatte.SaleDateStart_EventSale.Value, End = config.Rabatte.SaleDateEnd_EventSale.Value }
            };

            double? kilometerCost = eventType switch
            {
                "Rollendes-Fotozimmer" => config.Kilometerpreise.PricePerKm_Bus.Value,
                "Foto-Discokugel" => config.Kilometerpreise.PricePerKm_Disco.Value,
                "Video-Kaleidoskop" => config.Kilometerpreise.PricePerKm_Kaleido.Value,
                _ => null
            };
            if(kilometerCost == null) return BadRequest("Invalid event type.");

            int distanceInKm = await _distanceService.GetDistance(config, plzRouteEnd);
            if (distanceInKm == 0) return BadRequest("Invalid postal code. Is OSRM running?");

            var possibleEventDurations = CalculatePossibleEventDurations(config, distanceInKm);

            double hotelCosts = 0;
            if (distanceInKm >= config.Entfernungseinstellungen.RouteCalculation_MinKmForAdditionalHotelCost.Value)
            {
                hotelCosts = config.Mitarbeiterkosten.PricePerNight_Hotel.Value;
            }

            distanceInKm = (int)(distanceInKm * 2 * (1 + config.Routenkalkulierung.RouteCalculation_AdditionalKmInPercent.Value / 100));
            double travelCosts = distanceInKm * kilometerCost.Value;

            var priceDetails = possibleEventDurations.Select(duration =>
            {
                double basePrice = GetBasePrice(duration, config);
                var additionalOptionsDetails = CalculateAdditionalOptions(additionalOptions, duration, config);

                double totalCostWithoutDiscount = basePrice + travelCosts + hotelCosts + additionalOptionsDetails.TotalCost;
                var discounts = CalculateDiscounts(totalCostWithoutDiscount, eventDate, salePeriods, config);

                double totalCost = totalCostWithoutDiscount - discounts.TotalDiscount;

                return new
                {
                    eventDuration = duration,
                    basePrice,
                    travelDistance = distanceInKm,
                    travelDuration = (double)distanceInKm / 100 * 1.5,
                    travelCosts,
                    hotelCosts,
                    additionalOptions = additionalOptionsDetails.Details,
                    totalCostWithoutDiscount,
                    totalCost,
                    discounts.Applied,
                    vat = totalCost / 100 * config.Mehrwertsteuer.ValuedAddedTax.Value
                };
            }).ToList();

            return Ok(priceDetails);
        }

        private double GetBasePrice(int duration, dynamic config)
        {
            return duration switch
            {
                3 => config.Grundpreise.PriceBase_3hours.Value,
                5 => config.Grundpreise.PriceBase_5hours.Value,
                8 => config.Grundpreise.PriceBase_8hours.Value,
                _ => throw new ArgumentException("Invalid event duration.")
            };
        }

        private (List<object> Details, double TotalCost) CalculateAdditionalOptions(string? additionalOptions, int duration, dynamic config)
        {
            var details = new List<object>();
            double totalCost = 0;

            if (!string.IsNullOrEmpty(additionalOptions))
            {
                foreach (var option in additionalOptions.Split(','))
                {
                    if (option == "XXL-Druck")
                    {
                        double cost = config.Optionen.PricePerHour_FeatureXXLPrint.Value * duration;
                        totalCost += cost;
                        details.Add(new
                        {
                            optionName = "XXL-Druck",
                            pricePerHour = config.Optionen.PricePerHour_FeatureXXLPrint.Value,
                            priceTotal = cost
                        });
                    }
                }
            }

            return (details, totalCost);
        }

        private (List<object> Applied, double TotalDiscount) CalculateDiscounts(double totalCost, DateTime eventDate, dynamic salePeriods, dynamic config)
        {
            var discounts = new List<object>();
            double totalDiscount = 0;

            // Eventbasierter Rabatt
            if (DateService.IsDateBetween(eventDate, salePeriods.EventSale.Start, salePeriods.EventSale.End))
            {
                double discount = totalCost * config.Rabatte.SaleAmountInPercent_EventSale.Value / 100;
                discounts.Add(new
                {
                    name = config.Rabatte.SaleName_EventSale.Value,
                    periodStart = DateService.GetDateTimeWithYear(salePeriods.EventSale.Start, "start", eventDate),
                    periodEnd = DateService.GetDateTimeWithYear(salePeriods.EventSale.End, "end", eventDate),
                    discountAmount = discount
                });
                totalCost -= discount;
                totalDiscount += discount;
            }

            // Buchungsbasierter Rabatt
            if (DateService.IsDateBetween(DateTime.Now, salePeriods.BookingSale.Start, salePeriods.BookingSale.End))
            {
                double discount = totalCost * config.Rabatte.SaleAmountInPercent_BookingSale.Value / 100;
                discounts.Add(new
                {
                    name = config.Rabatte.SaleName_BookingSale.Value,
                    periodEnd = DateService.GetDateTimeWithYear(salePeriods.BookingSale.End, "end", eventDate),
                    discountAmount = discount
                });
                totalDiscount += discount;
            }

            return (discounts, totalDiscount);
        }
    }
}
