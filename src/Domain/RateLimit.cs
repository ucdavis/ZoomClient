using System;
using System.Collections.Generic;
using System.Text;

namespace ZoomClient.Domain
{
    /// <summary>
    /// Milliseconds to wait after each type of Zoom API call,
    /// based on new rate limits described at https://marketplace.zoom.us/docs/api-reference/rate-limits#rate-limits
    /// </summary>
    public class RateLimit
    {
        public const int Light = 13;        // 80 requests per second - single retrieval
        public const int Medium = 17;       // 60 requests per second - list retrieval or meeting creation
        public const int Heavy = 25;        // 40 requests per second - reports and dashboards
        public const int Intensive = 3000;  // 20 requests per minute ** - heavy dashboards / metrics
    }
}
