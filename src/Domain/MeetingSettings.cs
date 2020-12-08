using System;

namespace ZoomClient.Domain
{
    public class MeetingSettings
    {
        public MeetingSettings()
        {
            // fill out defaults
            host_video = false;
            participant_video = false;
            join_before_host = false;
            mute_upon_entry = true;
            approval_type = 2;              // no registration required
            audio = "both";                 // phone / computer
            auto_recording = "cloud";
            meeting_authentication = true;  // users must authenticate
        }

        public bool host_video { get; set; }
        public bool participant_video { get; set; }
        public bool cn_meeting { get; set; }
        public bool in_meeting { get; set; }
        public bool join_before_host { get; set; }
        public bool mute_upon_entry { get; set; }
        public bool watermark { get; set; }
        public bool use_pmi { get; set; }
        public int approval_type { get; set; }          // 0=auto approve 1=manual approve 2=no reg required
        public int registration_type { get; set; }      // 1=register once 2=register each occurrence 3=choose
        public string audio { get; set; }
        public string auto_recording { get; set; }
        public string alternative_hosts { get; set; }
        public bool close_registration { get; set; }
        public bool waiting_room { get; set; }
        public string[] global_dial_in_countries { get; set; }
        public string contact_name { get; set; }
        public string contact_email { get; set; }
        public bool registrants_email_notification { get; set; }
        public bool meeting_authentication { get; set; }
        public string authentication_option { get; set; }
        public string authentication_domains { get; set; }

        // serialization settings
        public bool ShouldSerializeaudio() { return audio != null; }
        public bool ShouldSerializeauto_recording() { return auto_recording != null; }
        public bool ShouldSerializealternative_hosts() { return alternative_hosts != null; }
        public bool ShouldSerializeglobal_dial_in_countries() { return global_dial_in_countries != null; }
        public bool ShouldSerializecontact_name() { return contact_name != null; }
        public bool ShouldSerializecontact_email() { return contact_email != null; }
        public bool ShouldSerializeauthentication_option() { return authentication_option != null; }
        public bool ShouldSerializeauthentication_domains() { return authentication_domains != null; }

    }
}
