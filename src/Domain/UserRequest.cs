using System;

namespace ZoomClient.Domain
{
    /// <summary>
    /// A request for licensed Zoom User creation
    /// </summary>
    public class UserRequest : ZObject
    {
        public UserRequest()
        {
            action = "create";
        }

        public UserRequest(string email, string firstname, string lastname)
        {
            action = "create";
            user_info = new UserInfo
            {
                email = email,
                first_name = firstname,
                last_name = lastname,
                type = 2    // request a licenesed user
            };
        }

        public string action { get; set; }
        public UserInfo user_info { get; set; }
    }
}
