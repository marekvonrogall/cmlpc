using System.Text.Json.Serialization;
using System.Text.Json;

namespace PriceCalculator.Services
{
    public class ConfigService
    {
        public Grundpreise Grundpreise { get; set; }
        public Kilometerpreise Kilometerpreise { get; set; }
        public Mitarbeiterkosten Mitarbeiterkosten { get; set; }
        public Optionen Optionen { get; set; }
        public Entfernungseinstellungen Entfernungseinstellungen { get; set; }
        public RoutenKalkulierung RoutenKalkulierung { get; set; }
        public Aufbauzeiten Aufbauzeiten { get; set; }
        public Abbauzeiten Abbauzeiten { get; set; }
        public Rabatte Rabatte { get; set; }

        public Mehrwertsteuer Mehrwertsteuer { get; set; }
    }

    public class NumericContent
    {
        public double Value { get; set; }
        public string DisplayName { get; set; }
    }

    public class StringContent
    {
        public string Value { get; set; }
        public string DisplayName { get; set; }
    }

    public class Grundpreise
    {
        public NumericContent PriceBase_3hours { get; set; }
        public NumericContent PriceBase_5hours { get; set; }
        public NumericContent PriceBase_8hours { get; set; }
    }

    public class Kilometerpreise
    {
        public NumericContent PricePerKm_Bus { get; set; }
        public NumericContent PricePerKm_Disco { get; set; }
        public NumericContent PricePerKm_Kaleido { get; set; }
    }

    public class Mitarbeiterkosten
    {
        public NumericContent PricePerNight_Hotel { get; set; }
    }

    public class Optionen
    {
        public NumericContent PricePerHour_FeatureXXLPrint { get; set; }
    }

    public class Entfernungseinstellungen
    {
        public NumericContent RouteCalculation_MaxRadiusFor3HourEvent { get; set; }
        public NumericContent RouteCalculation_MinKmForAdditionalHotelCost { get; set; }
    }

    public class RoutenKalkulierung
    {
        public NumericContent RouteCalculation_StartPointLat { get; set; }
        public NumericContent RouteCalculation_StartPointLon { get; set; }
        public NumericContent RouteCalculation_AdditionalKmInPercent { get; set; }
    }

    public class Aufbauzeiten
    {
        public NumericContent TimeInHours_AufbauBus { get; set; }
        public NumericContent TimeInHours_AufbauDisco { get; set; }
        public NumericContent TimeInHours_AufbauKaleido { get; set; }
    }

    public class Abbauzeiten
    {
        public NumericContent TimeInHours_AbbauBus { get; set; }
        public NumericContent TimeInHours_AbbauDisco { get; set; }
        public NumericContent TimeInHours_AbbauKaleido { get; set; }
    }

    public class Rabatte
    {
        public StringContent SaleName_BookingSale { get; set; }
        public NumericContent SaleAmountInPercent_BookingSale { get; set; }
        public StringContent SaleDateStart_BookingSale { get; set; }
        public StringContent SaleDateEnd_BookingSale { get; set; }
        
        public StringContent SaleName_EventSale { get; set; }
        public NumericContent SaleAmountInPercent_EventSale { get; set; }
        public StringContent SaleDateStart_EventSale { get; set; }
        public StringContent SaleDateEnd_EventSale { get; set; }
    }

    public class Mehrwertsteuer
    {
        public NumericContent ValuedAddedTax { get; set; }
    }
}