using System;

namespace ZoomClient.Domain
{
    public class UserInfo : ZObject
    {
        public string id { get; set; }
        public string email { get; set; }
        public int type { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }
}
