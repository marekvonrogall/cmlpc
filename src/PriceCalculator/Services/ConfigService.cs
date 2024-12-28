namespace PriceCalculator.Services
{
    public class ConfigService
    {
        public Grundpreise Grundpreise { get; set; }
        public Kilometerpreise Kilometerpreise { get; set; }
        public Mitarbeiterkosten Mitarbeiterkosten { get; set; }
        public Featurekosten Featurekosten { get; set; }
        public Entfernungseinstellungen Entfernungseinstellungen { get; set; }
        public RoutenKalkulierung RoutenKalkulierung { get; set; }
        public Aufbauzeiten Aufbauzeiten { get; set; }
        public Abbauzeiten Abbauzeiten { get; set; }
        public Rabatte Rabatte { get; set; }
    }

    public class Content
    {
        public double Value { get; set; }
        public string DisplayName { get; set; }
    }

    public class Grundpreise
    {
        public Content PriceBase_3hours { get; set; }
        public Content PriceBase_5hours { get; set; }
    }

    public class Kilometerpreise
    {
        public Content PricePerKm_Bus { get; set; }
        public Content PricePerKm_Disco { get; set; }
        public Content PricePerKm_Kaleido { get; set; }
    }

    public class Mitarbeiterkosten
    {
        public Content PricePerHour_1Employee { get; set; }
        public Content PricePerHour_2Employees { get; set; }
        public Content PricePerHour_Standby { get; set; }
        public Content PricePerNight_Hotel1Employee { get; set; }
        public Content PricePerNight_Hotel2Employees { get; set; }
    }

    public class Featurekosten
    {
        public Content PricePerHour_FeatureXXLPrint { get; set; }
    }

    public class Entfernungseinstellungen
    {
        public Content RouteCalculation_MaxRadiusFor3HourEvent { get; set; }
        public Content RouteCalculation_MinKmForAdditionalHotelCost { get; set; }
    }

    public class RoutenKalkulierung
    {
        public Content RouteCalculation_StartPointLat { get; set; }
        public Content RouteCalculation_StartPointLon { get; set; }
        public Content RouteCalculation_AdditionalKmInPercent { get; set; }
    }

    public class Aufbauzeiten
    {
        public Content TimeInHours_AufbauBus { get; set; }
        public Content TimeInHours_AufbauDisco { get; set; }
        public Content TimeInHours_AufbauKaleido { get; set; }
    }

    public class Abbauzeiten
    {
        public Content TimeInHours_AbbauBus { get; set; }
        public Content TimeInHours_AbbauDisco { get; set; }
        public Content TimeInHours_AbbauKaleido { get; set; }
    }

    public class Rabatte
    {
        public Content SaleAmountInPercent_RightNow { get; set; }
        public Content SaleAmountInPercent_OffSeason { get; set; }
    }
}