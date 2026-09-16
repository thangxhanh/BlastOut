using System;

namespace _Root.Scripts.Pattern
{
    public static class GameUtils
    {
        public static bool IsPlaying = false;
        public static double TimePerDay = 86400; //second
        public static double TimeNow = 0; //second
        public static int DayOfTime(double time) => (int) (time / TimePerDay);
        public static int DayUtc(double time) => (int) (time / TimePerDay);
        public static long SecondToTick(double s) => (long) s * TimeSpan.TicksPerSecond;
        
        public static int CurrentDayOfWeek => (int) ConvertUnixTimeToDateTimeUtc(TimeNow).DayOfWeek == 0
            ? 7 : (int) ConvertUnixTimeToDateTimeUtc(TimeNow).DayOfWeek;
        
        public static double ConvertDateTimeToSeconds(DateTime d)
        {
            var mSecond = d.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            return mSecond;
        }
        
        public static double StartTimeOfDayUtc(DateTime d)
        {
            var startDate = ConvertDateTimeToSeconds(d.Date);
            return (double) startDate;
        }
        
        public static double StartTimeOfDayUtc(double time)
        {
            var t = ConvertUnixTimeToDateTimeUtc(time);
            return ConvertDateTimeToSeconds(t.AddTicks(1).Date);;
        }

        public static DateTime ConvertUnixTimeToDateTimeUtc(double unixTime)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTime).ToUniversalTime();
            return dateTime;
        }
        public static string FormatMoneyK(double value, int dot = 0)
        {
            if (value >= 1000000000)
            {
                value /= 1000000000;
                return Math.Round(value, dot) + "B";
            }

            if (value >= 1000000)
            {
                value /= 1000000;
                return Math.Round(value, dot) + "M";
            }

            if (!(value >= 1000)) return Math.Round(value, dot).ToString();
            value /= 1000;
            return Math.Round(value, dot) + "K";
        }
        
        public static string FormatMoneyK2(double value, int dot = 0)
        {
            if (value >= 1000000000)
            {
                value /= 1000000000;
                return Math.Round(value, dot) + "B";
            }

            if (value >= 1000000)
            {
                value /= 1000000;
                return Math.Round(value, dot) + "M";
            }

            if (!(value >= 10000)) return Math.Round(value, dot).ToString();
            value /= 1000;
            return Math.Round(value, dot) + "K";
        }
    }
}