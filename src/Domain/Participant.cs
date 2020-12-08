using System;

namespace ZoomClient.Domain
{
    public class Participant : ZObject
    {
        /// <summary>
        /// Participant uuid
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Participant id
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// Participant display name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Participant email
        /// </summary>
        public string user_email { get; set; }
        /// <summary>
        /// Participant join time
        /// </summary>
        public string join_time { get; set; }
        /// <summary>
        /// Participant leave time
        /// </summary>
        public string leave_time { get; set; }
        /// <summary>
        /// Participant duration in seconds
        /// </summary>
        public int duration { get; set; }
    }
}
