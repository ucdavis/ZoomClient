using System;

namespace ZoomClient
{
    public class Options
    {
        public const string SectionName = "ZoomOptions";

        public string ApiSecret { get; set; }
        public string ApiKey { get; set; }
    }
}
