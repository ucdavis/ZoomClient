using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ZoomClient.Extensions
{
    public static class MyExtensions
    {
        /// <summary>
        /// Returns first N chars of the given string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static String FirstChars(this String str, int chars)
        {
            if (str == null)
            {
                return null;
            }
            else if (str.Length < chars)
            {
                return str;
            }
            return str.Substring(0, chars);
        }

        /// <summary>
        /// Returns a safe filename for saving to filesystem.
        /// Replaces & with 'and'.  Replaces space and dash with '_'.
        /// Removes everything else that's not a number, letter, or underscore.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static String FileSafeName(this String str)
        {
            str = str.Replace("&", "and")
                .Replace(" ", "_")
                .Replace("-", "_");

            return Regex.Replace(str, "[^a-zA-Z0-9_]", "");
        }

        /// <summary>
        /// Returns 9 to 11-digit meeting id prefix of passed in string (a filename).
        /// If string does not start with 9 to 11 digit id followed by underscore (_), returns empty string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MeetingIdPrefix(this String str)
        {
            var firstUnderscore = str.IndexOf("_");
            if (firstUnderscore < 9 || firstUnderscore > 11)
            {
                return "";
            }
            var meetingIdString = str.Substring(0, firstUnderscore);
            var meetingId = 0L;
            if (long.TryParse(meetingIdString, out meetingId))
            {
                return meetingIdString;
            }
            return "";
        }

        /// <summary>
        /// Returns 18-char meeting id prefix of passed in string (a filename).
        /// If string does not start with 9 to 11-digit id, underscore, 8-digit date, underscore, it returns empty string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SessionIdPrefix(this String str)
        {
            var match = Regex.Match(str, "^([0-9]{9,11})_([0-9]{8})_.*");
            if (!match.Success)
            {
                return "";
            }

            var meetingId = 0L;
            if (!long.TryParse(match.Groups[1].Value, out meetingId))
            {
                return "";
            }

            var meetingDate = DateTime.MinValue;
            if (!DateTime.TryParseExact(match.Groups[2].Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out meetingDate))
            {
                return "";
            }

            return $"{match.Groups[1].Value}_{match.Groups[2].Value}";
        }

        /// <summary>
        /// Extracts recording date from downloaded cloud recording filename.
        /// Expects MEETINGID_yyyyMMdd_hhmm_, otherwise returns null.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime? ExtractRecordingDatePart(this String str)
        {
            var match = Regex.Match(str, "^[0-9]{9,11}_([0-9]{8}_[0-9]{4})-.*");
            if (!match.Success)
            {
                return null;
            }

            var dateValue = DateTime.MinValue;
            if (DateTime.TryParseExact(match.Groups[1].Value, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
            {
                return dateValue;
            }

            return null;
        }

        /// <summary>
        /// Returns total minutes since midnight of the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int TotalMinutes(this DateTime date)
        {
            return date.Hour * 60 + date.Minute;
        }

        /// <summary>
        /// Zoom formatted datetime in local time zone
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToZoomLocalTime(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss");
        }
        /// <summary>
        /// Zoom formatted datetime in UTC (converts the date to UTC first, so it should be passed in as local time)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToZoomUTCFormat(this DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        /// <summary>
        /// Turns decimal file size into the nearest short string such as "12.5 GB" or "100 KB"
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string ShortFileSize(this decimal size)
        {
            var cutoffs = new[] {
                new { name = "GB", div = 1073741824 },
                new { name = "MB", div = 1048576 },
                new { name = "KB", div = 1024 },
                new { name = "B", div = 0 } };

            foreach (var cutoff in cutoffs)
            {
                if (size > cutoff.div)
                {
                    return String.Format("{0:0.0} {1}",
                        cutoff.div > 0 ? size / cutoff.div : size,
                        cutoff.name);
                }
            }

            return "";
        }

        /// <summary>
        /// Renames the File with a char prefix, or deletes if that filename already exists in this directory.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static bool RenameOrDeleteWithPrefix(this FileInfo fileInfo, char prefix)
        {
            var newName = Path.Combine(fileInfo.Directory.FullName, String.Format("{0}{1}", prefix, fileInfo.Name));
            if (File.Exists(newName))
            {
                fileInfo.Delete();
                return false;
            }

            fileInfo.MoveTo(newName);
            return true;
        }
    }
}
