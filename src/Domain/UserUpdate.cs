using System;
using System.Collections.Generic;
using System.Text;

namespace ZoomClient.Domain
{
    /// <summary>
    /// Represents an update requst to Zoom User Profile.
    /// Only fill out the desired changes, null properties will be ignored.
    /// Custom Attributes not included.
    /// </summary>
    public class UserUpdate : ZObject
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public PlanType? type { get; set; }
        public string pmi { get; set; }
        public bool? use_pmi { get; set; }
        public string timezone { get; set; }
        public string language { get; set; }
        public string dept { get; set; }
        public string vanity_name { get; set; }
        public string host_key { get; set; }
        public string cms_user_id { get; set; }
        public string job_title { get; set; }
        public string company { get; set; }
        public string location { get; set; }
        public string phone_number { get; set; }
        public string phone_country { get; set; }

        // serialization control
        public bool ShouldSerializefirst_name() { return first_name != null; }
        public bool ShouldSerializelast_name() { return last_name != null; }
        public bool ShouldSerializetype() { return type.HasValue; }
        public bool ShouldSerializepmi() { return pmi != null; }
        public bool ShouldSerializeuse_pmi() { return use_pmi.HasValue; }
        public bool ShouldSerializetimezone() { return timezone != null; }
        public bool ShouldSerializelanguage() { return language != null; }
        public bool ShouldSerializedept() { return dept != null; }
        public bool ShouldSerializevanity_name() { return vanity_name != null; }
        public bool ShouldSerializehost_key() { return host_key != null; }
        public bool ShouldSerializecms_user_id() { return cms_user_id != null; }
        public bool ShouldSerializejob_title() { return job_title != null; }
        public bool ShouldSerializecompany() { return company != null; }
        public bool ShouldSerializelocation() { return location != null; }
        public bool ShouldSerializephone_number() { return phone_number != null; }
        public bool ShouldSerializephone_country() { return phone_country != null; }
    }
}

/*
 * Not supported:
{
    "custom_attributes": {
      "type": "object",
      "description": "Custom attribute(s) of the user.",
      "properties": {
        "key": {
          "type": "string",
          "description": "Identifier for the custom attribute."
        },
        "name": {
          "type": "string",
          "description": "Name of the custom attribute."
        },
        "value": {
          "type": "string",
          "description": "Value of the custom attribute."
        }
      }
    }
  }
}
*/