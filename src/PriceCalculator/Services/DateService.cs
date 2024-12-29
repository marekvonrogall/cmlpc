using System.Globalization;

namespace PriceCalculator.Services
{
    public class DateService
    {
        private static int _placeHolderLeapYear = 2000; //Placeholder year (leap year)
        private static string _dateFormat = "dd.MM.yyyy";

        public static bool IsDateBetween(DateTime targetDate, string startDate, string endDate)
        {
            startDate = startDate.Trim().TrimEnd('.');
            endDate = endDate.Trim().TrimEnd('.');

            DateTime start;
            DateTime end;
            targetDate = new DateTime(_placeHolderLeapYear, targetDate.Month, targetDate.Day);
            DateTime.TryParseExact($"{startDate}.{_placeHolderLeapYear}", _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start);
            DateTime.TryParseExact($"{endDate}.{_placeHolderLeapYear}", _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end);

            if (start > end)
            {
                if (start < targetDate)
                {
                    end = start.AddYears(1);
                }
                else start = start.AddYears(-1);
            }

            return targetDate >= start && targetDate <= end;
        }

        public static DateTime GetDateTimeWithYear(string date, string type, DateTime eventDate)
        {
            //If this method gets called, we must already be sure that the event date is in range of the sale date (start-event-end)

            date = date.Trim().TrimEnd('.');

            DateTime adjustedDate;
            DateTime.TryParseExact($"{date}.{_placeHolderLeapYear}", _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out adjustedDate);

            if (type == "end")
            {
                if (adjustedDate < eventDate) //If the date is in the past, add a year
                {
                    adjustedDate = adjustedDate.AddYears(1);
                }
            }
            else //"start"
            {
                if (adjustedDate > eventDate) //If the date is in the future, subtract a year
                {
                    adjustedDate = adjustedDate.AddYears(-1);
                }
            }

            return adjustedDate;
        }
    }
}
