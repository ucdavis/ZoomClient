using System;

namespace ZoomClient
{
    public class Options
    {
        public const string SectionName = "ZoomOptions";

        public string AccountId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
