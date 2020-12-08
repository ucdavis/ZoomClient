using Newtonsoft.Json;
using System;

namespace ZoomClient.Domain
{
    public class User : ZObject
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public PlanType type { get; set; }   // enum 1=basic 2=licensed 3=onprem
        public string role_name { get; set; }
        public string pmi { get; set; }
        public bool use_pmi { get; set; }
        public string timezone { get; set; }
        public string dept { get; set; }
        public string created_at { get; set; }
        public string last_login_time { get; set; }
        public string last_client_version { get; set; }
        public string language { get; set; }
        public string phone_country { get; set; }
        public string phone_number { get; set; }
        public string vanity_url { get; set; }
        public int verified { get; set; }   // 1=verified 0=not verified
        public string pic_url { get; set; }
        public string cms_user_id { get; set; }
        public string account_id { get; set; }
        public string host_key { get; set; }
        public string status { get; set; }
        public string[] group_ids { get; set; }
        public string[] im_group_ids { get; set; }
        public string jid { get; set; }
        public string job_title { get; set; }
        public string company { get; set; }
        public string location { get; set; }

        [JsonIgnore]
        public string EmailBeforeAt
        {
            get
            {
                if (!this.email.Contains("@"))
                {
                    return this.email;
                }
                return this.email.Substring(0, this.email.IndexOf("@"));
            }
        }
    }
}
