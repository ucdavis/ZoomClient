using System;

namespace ZoomClient.Domain
{
    public class Recurrence : ZObject
    {
        public int type { get; set; }   // 1=daily 2=weekly 3=monthly
        public int repeat_interval { get; set; }    // max 90 days; 12 weeks; 3 months
        public string weekly_days { get; set; }     // 1=sunday...7=saturday
        public int monthly_day { get; set; }
        public int monthly_week { get; set; }       // -1=last week 1=first week 2=second week 3=third week 4=fourth week
        public int monthly_week_day { get; set; }   // 1=sunday...7=saturday
        public int end_times { get; set; }          // number of recurrences
        public string end_date_time { get; set; }

        // serialization settings
        public bool ShouldSerializeweekly_days() { return type == 2; }
        public bool ShouldSerializemonthly_day() { return type == 3; }
        public bool ShouldSerializemonthly_week() { return type == 3; }
        public bool ShouldSerializemonthly_week_day() { return type == 3; }
        public bool ShouldSerializeend_times() { return end_date_time == null; }
        public bool ShouldSerializeend_date_time() { return end_date_time != null; }
    }
}
