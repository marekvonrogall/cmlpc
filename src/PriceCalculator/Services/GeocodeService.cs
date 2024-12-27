using System.Globalization;

namespace PriceCalculator.Services
{
    public class GeocodeService
    {
        private readonly Dictionary<string, (double, double)> _postalCodeMap;

        public GeocodeService(string csvFilePath)
        {
            _postalCodeMap = File.ReadAllLines(csvFilePath)
                .Skip(1)
                .Select(line => line.Split(','))
                .ToDictionary(
                    fields => fields[0], // Postal code
                    fields => (double.Parse(fields[1], CultureInfo.InvariantCulture), double.Parse(fields[2], CultureInfo.InvariantCulture)) // Lat, Lon
                );
        }

        public (double lat, double lon)? GetCoordinates(string postalCode)
        {
            if (_postalCodeMap.TryGetValue(postalCode, out var coords))
            {
                return coords;
            }
            return null;
        }
    }
}
