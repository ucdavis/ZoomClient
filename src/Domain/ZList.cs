using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ZoomClient.JSON;

namespace ZoomClient.Domain
{
    [JsonConverter(typeof(ZListConverter))]
    public class ZList<T> where T : ZObject
    {
        public int page_count { get; set; }
        public int page_number { get; set; }
        public int page_size { get; set; }
        public int total_records { get; set; }

        // different object style for GET RECORDINGS, needs these extra properties.  page_number not filled!

        /// <summary>
        /// Start Date
        /// </summary>
        public string from { get; set; }
        /// <summary>
        /// End Date
        /// </summary>
        public string to { get; set; }
        /// <summary>
        /// Next Page Token
        /// </summary>
        public string next_page_token { get; set; }

        public IList<T> Results { get; set; }
    }
}
