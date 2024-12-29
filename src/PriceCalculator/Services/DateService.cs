namespace PriceCalculator.Services
{
    public class DateService
    {
        // convert date to int (1-366).
        public static int GetDayOfYear(object date)
        {
            DateTime adjustedDate;
            if (date is DateTime dateTime)
            {
                adjustedDate = new DateTime(2000, dateTime.Month, dateTime.Day);
            }
            else if (date is string dateStr)
            {
                dateStr = dateStr.Trim().TrimEnd('.');
                string fullDateStr = dateStr + ".2000";
                adjustedDate = DateTime.ParseExact(fullDateStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                throw new Exception("Invalid date format. Expected DateTime or string in dd.MM format.");
            }
            return adjustedDate.DayOfYear;
        }

        public static bool IsDateBetween(int targetDay, int startDay, int endDay)
        {
            if (startDay <= endDay)
            {
                // No wrap
                return targetDay >= startDay && targetDay <= endDay;
            }
            else
            {
                // Wrap
                return targetDay >= startDay || targetDay <= endDay;
            }
        }

        public static DateTime GetDateTimeWithYear(string date, string type, DateTime eventDate)
        {
            date = date.Trim().TrimEnd('.');
            string fullDateStr = date + $".{eventDate.Year}";
            DateTime adjustedDate = DateTime.ParseExact(fullDateStr, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);

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
