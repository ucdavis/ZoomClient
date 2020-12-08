using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using ZoomClient.Extensions;

namespace ZoomClient.Domain
{
    public class Meeting : ZObject
    {
        public string uuid { get; set; }
        public string id { get; set; }
        public string host_id { get; set; }
        /// <summary>
        /// Email of host. May only be filled after GetCloudRecordingsForAccount.
        /// Not populated by GetCloudRecordingsForUser.
        /// </summary>
        public string host_email { get; set; }

        public string topic { get; set; }
        public int type { get; set; }        // 1=instant, 2=scheduled, 3=recurring without fixed time, 8=recurring with fixed time
        /// <summary>
        /// Status may be populated only when loaded with GET meeting by id.
        /// </summary>
        public string status { get; set; }
        public string start_time { get; set; }
        public int duration { get; set; }
        public string timezone { get; set; }
        public string created_at { get; set; }
        public string join_url { get; set; }
        public string agenda { get; set; }

        public string start_url { get; set; }
        public string password { get; set; }
        public string h323password { get; set; }
        public string pmi { get; set; }

        public MeetingSettings settings { get; set; }
        public List<Recording> recording_files { get; set; }
        public Recurrence recurrence { get; set; }
        public List<MeetingOccurrence> occurrences { get; set; }

        // serialization control
        public bool ShouldSerializeuuid() { return uuid != null; }
        public bool ShouldSerializeid() { return id != null; }
        public bool ShouldSerializehost_id() { return host_id != null; }
        public bool ShouldSerializetopic() { return topic != null; }
        public bool ShouldSerializestart_time() { return start_time != null; }
        public bool ShouldSerializetimezone() { return timezone != null; }
        public bool ShouldSerializecreated_at() { return created_at != null; }
        public bool ShouldSerializejoin_url() { return join_url != null; }
        public bool ShouldSerializeagenda() { return agenda != null; }
        public bool ShouldSerializestart_url() { return start_url != null; }
        public bool ShouldSerializepassword() { return password != null; }
        public bool ShouldSerializeh323password() { return h323password != null; }
        public bool ShouldSerializesettings() { return settings != null; }

        // helper properties
        [JsonIgnore]
        public DateTime StartDateTimeLocal
        {
            get
            {
                if (this.start_time == null) { return DateTime.MinValue; }
                return DateTime.Parse(this.start_time, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
            }
        }

        [JsonIgnore]
        public DateTime EndDateTime
        {
            get
            {
                if (this.start_time == null) { return DateTime.MinValue; }
                return StartDateTimeLocal.AddMinutes(this.duration);
            }
        }

        [JsonIgnore]
        public double MinutesPastEnd
        {
            get
            {
                if (this.start_time == null) { return 0; }
                return DateTime.Now.TimeOfDay.Subtract(EndDateTime.TimeOfDay).TotalMinutes;
            }
        }

        [JsonIgnore]
        public string RecordingFileName
        {
            get
            {
                return String.Format("{0}_{1:yyyyMMdd_HHmm}-{2:HHmm}_{3}",
                    this.id, this.StartDateTimeLocal, this.EndDateTime, this.topic.FileSafeName());
            }
        }

        [JsonIgnore]
        public bool WrongDay
        {
            get
            {
                if (recurrence == null || recurrence.weekly_days == null)
                {
                    return StartDateTimeLocal.DayOfWeek != DateTime.Now.DayOfWeek;
                }
                return !(recurrence.weekly_days.Contains((((int)DateTime.Now.DayOfWeek) + 1).ToString()));
            }
        }

        [JsonIgnore]
        public string RoomName { get; set; }
    }
}
