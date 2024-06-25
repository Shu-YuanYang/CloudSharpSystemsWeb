using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryClassLibrary.DateTimeHelper
{
    public class TimestampHelper
    {
        public const string ISO_FORMAT = "yyyyMMddTHHmmssZ";
        public const string PRECISE_FORMAT = "MM/dd/yyyy HH:mm:ss.fff";
        public const string TIMESTAMP_ID_FORMAT = "yyyyMMddHHmmss";

        public static string ToPreciseFormatString(DateTime timestamp) 
        {
            return timestamp.ToString(PRECISE_FORMAT);
        }

        public static string ToTimeStampIDFormatString(DateTime timestamp) 
        {
            return timestamp.ToString(TIMESTAMP_ID_FORMAT);
        }

        public static DateTime ToPreciseFormatDateTime(string timestamp_string)
        {
            return DateTime.ParseExact(timestamp_string, PRECISE_FORMAT, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ToUniversalISOFormatString(DateTime timestamp) {
            return timestamp.ToString("s");
        }

    }

}
