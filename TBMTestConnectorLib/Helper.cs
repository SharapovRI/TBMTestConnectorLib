using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBMTestConnectorLib
{
    internal static class Helper
    {
        internal static string SecondsToTimeFrame(int seconds)
        {
            return seconds switch
            {
                <= 60 => "1m",
                <= 60 * 5 => "5m",
                <= 60 * 15 => "15m",
                <= 60 * 30 => "30m",
                <= 3600 => "1h",
                <= 3600 * 3 => "3h",
                <= 3600 * 6 => "6h",
                <= 3600 * 12 => "12h",
                <= 3600 * 24 => "1D",
                <= 3600 * 24 * 7 => "1W",
                <= 3600 * 24 * 14 => "14D",
                _ => "1M",
            };
        }

        internal static int TimeFrameToSeconds(string timeFrame)
        {
            return timeFrame switch
            {
                "1m" => 60,
                "5m" => 60 * 5,
                "15m" => 60 * 15,
                "30m" => 60 * 30,
                "1h" => 3600,
                "3h" => 3600 * 3,
                "6h" => 3600 * 6,
                "12h" => 3600 * 12,
                "1D" => 3600 * 24,
                "1W" => 3600 * 24 * 7,
                "14D" => 3600 * 24 * 14,
                "1M" => 3600 * 24 * 30,
                _ => -1,
            };
        }
    }
}
