using System;

namespace ZoomClient.Domain
{
    public class MeetingRequest : ZObject
    {
        public MeetingRequest()
        {
            // fill out defaults
            timezone = "America/Los_Angeles";
            settings = new MeetingSettings();
        }

        public string topic { get; set; }

        /// <summary>
        /// Recurrence Type: 1=instant, 2=scheduled, 3=recurring without fixed time, 8=recurring with fixed time
        /// </summary>
        public int type
        {
            get
            {
                return recurrence == null ? 2 : 8;
            }
        }
        public string start_time { get; set; }
        public int duration { get; set; }
        public string timezone { get; set; }
        public string password { get; set; }
        public string agenda { get; set; }

        public Recurrence recurrence { get; set; }
        public MeetingSettings settings { get; set; }

        // serialization control
        public bool ShouldSerializetopic() { return topic != null; }
        public bool ShouldSerializestart_time() { return start_time != null; }
        public bool ShouldSerializetimezone() { return timezone != null; }
        public bool ShouldSerializeagenda() { return agenda != null; }
        public bool ShouldSerializepassword() { return password != null; }
        public bool ShouldSerializesettings() { return settings != null; }
        public bool ShouldSerializerecurrence() { return recurrence != null; }

    }
}
