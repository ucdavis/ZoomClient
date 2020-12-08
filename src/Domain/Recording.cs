using Newtonsoft.Json;
using System;
using System.Globalization;
using ZoomClient.Extensions;

namespace ZoomClient.Domain
{
    public class Recording : ZObject
    {
        public string id { get; set; }
        public string meeting_id { get; set; }
        public string recording_start { get; set; }
        public string recording_end { get; set; }
        public string file_type { get; set; }       // mp4, m4a, timeline, transcript, chat, cc
        public decimal file_size { get; set; }
        public string play_url { get; set; }
        public string download_url { get; set; }
        public string status { get; set; }
        public string deleted_time { get; set; }
        public string recording_type { get; set; }  // see https://marketplace.zoom.us/docs/api-reference/zoom-api/cloud-recording/recordingslist

        [JsonIgnore]
        public DateTime RecordingStartDateTime
        {
            get
            {
                return DateTime.Parse(this.recording_start, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
            }
        }

        [JsonIgnore]
        public DateTime RecordingEndDateTime
        {
            get
            {
                var testDate = DateTime.MaxValue;
                DateTime.TryParse(this.recording_end, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out testDate);

                return testDate;
            }
        }

        /// <summary>
        /// Gets total minutes duration of this recording.  If the recording has no end date, returns 0.
        /// </summary>
        [JsonIgnore]
        public int RecordingDurationMinutes
        {
            get
            {
                if (RecordingEndDateTime == DateTime.MaxValue) { return 0; }
                return (int)Math.Round(RecordingEndDateTime.Subtract(RecordingStartDateTime).TotalMinutes);
            }
        }

        public string GetLocalFileName(Meeting meeting)
        {
            return String.Format("{0}_{1:yyyyMMdd_HHmm}-{2:HHmm}_{3}_{4}",
                meeting.id, meeting.StartDateTimeLocal, meeting.EndDateTime, meeting.topic.FileSafeName(), id);
        }
    }
}
