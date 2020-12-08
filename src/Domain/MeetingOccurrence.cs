using System;
using System.Globalization;
using Newtonsoft.Json;

namespace ZoomClient.Domain
{
    public class MeetingOccurrence
    {
        public int duration { get; set; }
        public string occurrence_id { get; set; }
        public string start_time { get; set; }
        public string status { get; set; }

        // helper properties
        [JsonIgnore]
        public DateTime StartDateTimeLocal
        {
            get
            {
                return DateTime.Parse(this.start_time, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
            }
        }

        [JsonIgnore]
        public DateTime EndDateTime
        {
            get
            {
                return StartDateTimeLocal.AddMinutes(this.duration);
            }
        }

        [JsonIgnore]
        public double MinutesPastEnd
        {
            get
            {
                return DateTime.Now.TimeOfDay.Subtract(EndDateTime.TimeOfDay).TotalMinutes;
            }
        }
    }
}
