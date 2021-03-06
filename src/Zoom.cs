using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using JWT.Algorithms;
using JWT.Builder;
using RestSharp;
using RestSharp.Authenticators;
using ZoomClient.Domain;
using ZoomClient.Extensions;
using System.IO;
using ZoomClient.Domain.Billing;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using RestSharp.Serializers.NewtonsoftJson;

namespace ZoomClient
{
    public class Zoom
    {
        private readonly string BaseUrl = "https://api.zoom.us/v2/";
        private RestClient client = null;
        private int PageSize = 80;
        private Options _zoomOptions;
        private ILogger<Zoom> _logger;

        public Zoom(ILogger<Zoom> logger)
        {
            client = new RestClient(BaseUrl);
            client.UseNewtonsoftJson();
            _logger = logger;
        }

        public Zoom(Options zoomOptions, ILogger<Zoom> logger)
        {
            client = new RestClient(BaseUrl);
            _zoomOptions = zoomOptions;
            _logger = logger;
        }

        public Options Options
        {
            set
            {
                _zoomOptions = value;
            }
        }

        /// <summary>
        /// Gets a specific Zoom User by id (userid or email address)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/users/user</remarks>
        public User GetUser(string userId)
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("users/{userId}", Method.GET, DataFormat.Json)
                .AddParameter("userId", userId, ParameterType.UrlSegment);

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
            {
                return JsonConvert.DeserializeObject<User>(response.Content);
            }
            _logger.LogWarning($"Zoom.GetUser returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }

            return null;
        }

        /// <summary>
        /// Gets all Zoom users in active status on this account.
        /// </summary>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/users/users</remarks>
        public List<User> GetUsers()
        {
            return GetUsers("active");
        }

        /// <summary>
        /// Gets all Zoom users of the specified status on this account.
        /// </summary>
        /// <param name="userStatus">Status of users to return.  "active", "inactive", "pending" are allowed.</param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/users/users</remarks>
        public List<User> GetUsers(string userStatus)
        {
            var page = 0;
            var pages = 1;
            List<User> users = new List<User>();

            do
            {
                page++;

                var request = new RestRequest("users", Method.GET, DataFormat.Json)
                    .AddParameter("status", userStatus)
                    .AddParameter("page_size", PageSize)
                    .AddParameter("page_number", page);

                client.Authenticator = NewToken;
                var response = client.Execute(request);
                Thread.Sleep(RateLimit.Medium);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var result = JsonConvert.DeserializeObject<ZList<User>>(response.Content);

                    if (result?.Results != null)
                    {
                        users.AddRange(result.Results);
                        pages = result.page_count;
                    }
                }
                else
                {
                    _logger.LogWarning($"Zoom.GetUsers pg{page} returned {response.StatusCode} - {response.StatusDescription}");
                    if (!String.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                        _logger.LogWarning($"ErrorException: {response.ErrorException}");
                    }
                }
            }
            while (page < pages);

            // TODO prep for deprecation of page_number in favor of next_page_token (see ZList)

            return users;
        }

        /// <summary>
        /// Create a new Zoom User as licensed user including name and email address
        /// </summary>
        /// <param name="userRequest">user request object with name and email address</param>
        /// <returns>UserInfo with new id included</returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/users/usercreate</remarks>
        public UserInfo CreateUser(UserRequest userRequest)
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("users", Method.POST, DataFormat.Json)
                .AddJsonBody(userRequest);

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return JsonConvert.DeserializeObject<UserInfo>(response.Content);
            }
            _logger.LogWarning($"Zoom.CreateUser returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }

            return null;
        }

        /// <summary>
        /// Update a Zoom User's profile
        /// </summary>
        /// <param name="userId">Id of user to update</param>
        /// <param name="profileChanges">Changes to user's profile.
        /// Only fill out properties you want changed, others will be ignored.</param>
        /// <returns>true if updated successfully</returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/users/userupdate</remarks>
        public bool UpdateUserProfile(string userId, UserUpdate profileChanges)
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("users/{userId}", Method.PATCH, DataFormat.Json)
                .AddParameter("userId", userId, ParameterType.UrlSegment)
                .AddJsonBody(profileChanges);

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            _logger.LogWarning($"Zoom.UpdateUserProfile returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return false;
        }

        /// <summary>
        /// Gets details of a Zoom Meeting
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meeting</remarks>
        public Meeting GetMeetingDetails(string meetingId)
        {
            return GetMeetingDetails(meetingId, null);
        }

        /// <summary>
        /// Gets details of a Zoom Meeting
        /// </summary>
        /// <param name="meetingId"></param>
        /// <param name="occurrenceId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meeting</remarks>
        public Meeting GetMeetingDetails(string meetingId, string occurrenceId)
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("/meetings/{meetingId}", Method.GET, DataFormat.Json)
                .AddParameter("meetingId", meetingId, ParameterType.UrlSegment);

            // TODO add new parameter:  bool show_previous_occurrences

            if (!String.IsNullOrEmpty(occurrenceId))
            {
                request.AddParameter("occurrence_id", occurrenceId, ParameterType.QueryString);
            }

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Meeting>(response.Content);
            }
            _logger.LogWarning($"Zoom.GetMeetingDetails returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }

            return null;
        }

        /// <summary>
        /// All upcoming meetings for Zoom user by userid or email.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meetings</remarks>
        public List<Meeting> GetMeetingsForUser(string userId)
        {
            return GetMeetingsForUser(userId, "upcoming");
        }

        /// <summary>
        /// All meetings for Zoom user by meetingType and userId or email.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="meetingType"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meetings</remarks>
        public List<Meeting> GetMeetingsForUser(string userId, string meetingType)
        {
            var page = 0;
            var pages = 1;
            List<Meeting> meetings = new List<Meeting>();

            do
            {
                page++;

                var request = new RestRequest("users/{userId}/meetings", Method.GET, DataFormat.Json)
                    .AddParameter("userId", userId, ParameterType.UrlSegment)
                    .AddParameter("type", meetingType)
                    .AddParameter("page_size", PageSize)
                    .AddParameter("page_number", page);

                client.Authenticator = NewToken;
                var response = client.Execute(request);
                Thread.Sleep(RateLimit.Medium);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var result = JsonConvert.DeserializeObject<ZList<Meeting>>(response.Content);
                    if (result != null && result.Results != null)
                    {
                        meetings.AddRange(result.Results);
                        pages = result.page_count;
                    }
                }
                else
                {
                    _logger.LogWarning($"Zoom.GetMeetingsForUser for userId '{userId}' pg{page} returned {response.StatusCode} - {response.StatusDescription}");
                    if (!String.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                        _logger.LogWarning($"ErrorException: {response.ErrorException}");
                    }
                }
            }
            while (page < pages);

            // TODO prep for deprecation of page_number in favor of next_page_token (see ZList)

            return meetings;
        }

        /// <summary>
        /// Get list of ended meeting instances by meeting id
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/pastmeetings</remarks>
        public List<Meeting> GetPastMeetingInstances(string meetingId)
        {
            var request = new RestRequest("/past_meetings/{meetingId}/instances", Method.GET, DataFormat.Json)
                .AddParameter("meetingId", meetingId, ParameterType.UrlSegment);

            client.Authenticator = NewToken;
            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Medium);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = JsonConvert.DeserializeObject<ZList<Meeting>>(response.Content);
                return result.Results.ToList();
            }
            _logger.LogWarning($"Zoom.GetPastMeetingInstances for meetingId '{meetingId}' returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return null;
        }

        /// <summary>
        /// Get details of past meeting by UUID
        /// </summary>
        /// <param name="meetingUUID"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/pastmeetingdetails</remarks>
        public Meeting GetPastMeetingDetails(string meetingUUID)
        {
            var request = new RestRequest("/past_meetings/{meetingUUID}", Method.GET, DataFormat.Json)
                .AddParameter("meetingUUID", meetingUUID.FixUUIDSlashEncoding(), ParameterType.UrlSegment);

            client.Authenticator = NewToken;
            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Meeting>(response.Content);
            }
            _logger.LogWarning($"Zoom.GetPastMeetingDetails for meetingUUID '{meetingUUID}' returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return null;
        }

        /// <summary>
        /// Create meeting for user
        /// </summary>
        /// <param name="meeting"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meetingcreate</remarks>
        public Meeting CreateMeetingForUser(MeetingRequest meeting, string userId)
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("users/{userId}/meetings", Method.POST, DataFormat.Json)
                .AddParameter("userId", userId, ParameterType.UrlSegment)
                .AddJsonBody(meeting);

            _logger.LogDebug($"Create Meeting For User JSON: {JsonConvert.SerializeObject(meeting)}; Request Body {request.Body}");

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Medium);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return JsonConvert.DeserializeObject<Meeting>(response.Content);
            }
            _logger.LogWarning($"Zoom.CreateMeetingForUser userId '{userId}' returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return null;
        }

        /// <summary>
        /// End a meeting by meeting id.
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meetingstatus</remarks>
        public bool EndMeeting(string meetingId)
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("meetings/{meetingId}/status", Method.PUT, DataFormat.Json)
                .AddParameter("meetingId", meetingId, ParameterType.UrlSegment)
                .AddJsonBody(new EndAction());

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }

            _logger.LogWarning($"Zoom.EndMeeting '{meetingId}' returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return false;
        }

        /// <summary>
        /// Deletes a meeting by meeting id
        /// </summary>
        /// <param name="meetingId">Id of meeting to delete</param>
        /// <param name="sendReminder">True to send reminder to hosts about deletion.  Defaults to false.</param>
        /// <returns>true if deleted successfully</returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meetingdelete</remarks>
        public bool DeleteMeeting(string meetingId, bool sendReminder = false)
        {
            return DeleteMeeting(meetingId, null, sendReminder);
        }

        /// <summary>
        /// Deletes a meeting by meeting id and occurrenceId
        /// </summary>
        /// <param name="meetingId">Id of meeting to delete</param>
        /// <param name="occurrenceId">Id of occurrence to delete.  If null or blank, entire meeting will be deleted.</param>
        /// <param name="sendReminder">True to send reminder to hosts about deletion.  Defaults to false.</param>
        /// <returns>true if deleted successfully</returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/meetings/meetingdelete</remarks>
        public bool DeleteMeeting(string meetingId, string occurrenceId, bool sendReminder = false)
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("meetings/{meetingId}", Method.DELETE, DataFormat.Json)
                .AddParameter("meetingId", meetingId, ParameterType.UrlSegment)
                .AddParameter("schedule_for_reminder", sendReminder, ParameterType.QueryString);

            if (!String.IsNullOrEmpty(occurrenceId))
            {
                request = request.AddParameter("occurrence_id", occurrenceId, ParameterType.QueryString);
            }

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }

            _logger.LogWarning($"Zoom.DeleteMeeting '{meetingId}' returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return false;
        }

        /// <summary>
        /// Gets recordings for the given user in the last 3 months.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/cloud-recording/recordingslist</remarks>
        public List<Meeting> GetCloudRecordingsForUser(string userId)
        {
            var page = 0;
            var pages = 1;
            List<Meeting> meetings = new List<Meeting>();

            do
            {
                page++;

                var request = new RestRequest("users/{userId}/recordings", Method.GET, DataFormat.Json)
                    .AddParameter("userId", userId, ParameterType.UrlSegment)
                    .AddParameter("page_size", PageSize)
                    .AddParameter("page_number", page)
                    .AddParameter("from", DateTime.Now.AddMonths(-3).ToZoomUTCFormat())
                    .AddParameter("to", DateTime.Now.ToZoomUTCFormat());

                client.Authenticator = NewToken;
                var response = client.Execute(request);
                Thread.Sleep(RateLimit.Medium);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var result = JsonConvert.DeserializeObject<ZList<Meeting>>(response.Content);
                    if (result?.Results != null)
                    {
                        meetings.AddRange(result.Results);
                        pages = result.page_count;
                    }
                }
                else
                {
                    _logger.LogWarning($"Zoom.GetCloudRecordingsForUser for userId '{userId}' pg{page} returned {response.StatusCode} - {response.StatusDescription}");
                    if (!String.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                        _logger.LogWarning($"ErrorException: {response.ErrorException}");
                    }
                }
            }
            while (page < pages);

            return meetings;
        }

        /// <summary>
        /// Gets recordings for the given account id ("me" by default) for the past 7 days.
        /// </summary>
        /// <param name="accountId">Defaults to "me". Calling account must have rights.</param>
        /// <returns></returns>
        public List<Meeting> GetCloudRecordingsForAccount(string accountId = "me")
        {
            List<Meeting> meetings = new List<Meeting>();
            string nextPageToken = "";

            do
            {
                var request = new RestRequest("accounts/{accountId}/recordings", Method.GET, DataFormat.Json)
                    .AddParameter("accountId", accountId, ParameterType.UrlSegment)
                    .AddParameter("page_size", PageSize)
                    .AddParameter("from", DateTime.Now.AddDays(-7).ToZoomUTCFormat())
                    .AddParameter("to", DateTime.Now.ToZoomUTCFormat());

                if (nextPageToken != "")
                {
                    request = request.AddParameter("next_page_token", nextPageToken);
                }

                client.Authenticator = NewToken;
                var response = client.Execute(request);
                Thread.Sleep(RateLimit.Medium);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var result = JsonConvert.DeserializeObject<ZList<Meeting>>(response.Content);
                    if (result?.Results != null)
                    {
                        meetings.AddRange(result.Results);
                        nextPageToken = result.next_page_token;
                    }
                    else
                    {
                        nextPageToken = "";
                    }
                }
                else
                {
                    _logger.LogWarning($"Zoom.GetCloudRecordingsForAccount for accountId '{accountId}' returned {response.StatusCode} - {response.StatusDescription}");
                    if (!String.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                        _logger.LogWarning($"ErrorException: {response.ErrorException}");
                    }
                }
            }
            while (nextPageToken != "");

            return meetings;
        }

        /// <summary>
        /// Downloads zoom cloud recording from url, using memory instead of stream.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/cloud-recording/recordingget</remarks>
        public IRestResponse DownloadRecording(string url)
        {
            var downloadClient = new RestClient("https://api.zoom.us/");

            var request = new RestRequest(url, Method.GET)
                .AddParameter("access_token", NewTokenString, ParameterType.QueryString);

            return downloadClient.Execute(request);
        }

        /// <summary>
        /// Downloads zoom cloud recording from url, using stream instead of memory (preferred).
        /// </summary>
        /// <param name="url"></param>
        /// <param name="saveToPath"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/cloud-recording/recordingget</remarks>
        public IRestResponse DownloadRecordingStream(string url, string saveToPath)
        {
            var downloadClient = new RestClient("https://api.zoom.us/");

            using (var writer = new FileStream(saveToPath, FileMode.Create, FileAccess.Write, FileShare.None, 128000, false))
            {
                using (var reader = new MemoryStream())
                {
                    var request = new RestRequest(url, Method.GET)
                        .AddParameter("access_token", NewTokenString, ParameterType.QueryString);

                    request.ResponseWriter = responseStream =>
                    {
                        using (responseStream)
                        {
                            responseStream.CopyTo(writer);
                        }
                    };

                    return downloadClient.Execute(request);
                }
            }
        }

        /// <summary>
        /// Deletes a specific recording file by meeting id and recording id.
        /// </summary>
        /// <param name="meetingId"></param>
        /// <param name="recordingId"></param>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/cloud-recording/recordingdeleteone</remarks>
        public bool DeleteRecording(string meetingId, string recordingId)
        {
            client.Authenticator = NewToken;
            meetingId = meetingId.FixUUIDSlashEncoding();

            var request = new RestRequest("/meetings/{meetingId}/recordings/{recordingId}", Method.DELETE, DataFormat.Json)
                .AddParameter("meetingId", meetingId, ParameterType.UrlSegment)
                .AddParameter("recordingId", recordingId, ParameterType.UrlSegment)
                .AddParameter("action", "trash", ParameterType.QueryString);

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Light);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }

            _logger.LogWarning($"Zoom.DeleteRecording meetingId '{meetingId}' recordingId '{recordingId}' returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return false;
        }

        /// <summary>
        /// Gets a Plan Usage report for the entire account
        /// </summary>
        /// <returns></returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/billing/getplanusage</remarks>
        public PlanUsage GetPlanUsage()
        {
            client.Authenticator = NewToken;

            var request = new RestRequest("/accounts/{accountId}/plans/usage", Method.GET, DataFormat.Json)
                .AddParameter("accountId", "me", ParameterType.UrlSegment);

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Heavy);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<PlanUsage>(response.Content);
            }

            _logger.LogWarning($"Zoom.GetPlanUsage returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }
            return null;
        }

        /// <summary>
        /// Get Participant Report for a meeting
        /// </summary>
        /// <param name="meetingId"></param>
        /// <remarks>
        /// https://marketplace.zoom.us/docs/api-reference/zoom-api/reports/reportmeetingparticipants
        /// Compare to Zoom Reports : Active Host Report
        /// </remarks>
        public List<Participant>GetParticipantReport(string meetingId)
        {
            string nextPageToken = "";
            var participants = new List<Participant>();

            do
            {
                var request = new RestRequest("/report/meetings/{meetingId}/participants", Method.GET, DataFormat.Json)
                    .AddParameter("meetingId", meetingId.FixUUIDSlashEncoding(), ParameterType.UrlSegment);

                if (nextPageToken != "")
                {
                    request = request.AddParameter("next_page_token", nextPageToken);
                }

                client.Authenticator = NewToken;
                var response = client.Execute(request);
                Thread.Sleep(RateLimit.Heavy);

                if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
                {
                    var result = JsonConvert.DeserializeObject<ZList<Participant>>(response.Content);
                    if (result?.Results != null)
                    {
                        participants.AddRange(result.Results);
                        nextPageToken = result.next_page_token;
                    }
                    else
                    {
                        nextPageToken = "";
                    }
                }
                else
                {
                    _logger.LogWarning($"Zoom.GetParticipantReport for meetingId '{meetingId}' returned {response.StatusCode} - {response.StatusDescription}");
                    if (!String.IsNullOrEmpty(response.ErrorMessage))
                    {
                        _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                        _logger.LogWarning($"ErrorException: {response.ErrorException}");
                    }
                }
            }
            while (nextPageToken != "");

            return participants;
        }

        /// <summary>
        /// Upload a user's profile picture
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>true if successful</returns>
        /// <remarks>https://marketplace.zoom.us/docs/api-reference/zoom-api/users/userpicture</remarks>
        public bool UploadProfilePicture(string userId, string imagePath)
        {
            // fail if no image or no userid
            if (String.IsNullOrEmpty(userId) || !File.Exists(imagePath)) { return false; }

            client.Authenticator = NewToken;

            var request = new RestRequest("/users/{userId}/picture", Method.POST, DataFormat.None)
                .AddParameter("userId", userId, ParameterType.UrlSegment);

            request.AddFile("pic_file", imagePath, "multipart/form-data");

            var response = client.Execute(request);
            Thread.Sleep(RateLimit.Medium);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                return true;
            }

            _logger.LogWarning($"Zoom.UploadProfilePicture returned {response.StatusCode} - {response.StatusDescription}");
            if (!String.IsNullOrEmpty(response.ErrorMessage))
            {
                _logger.LogWarning($"ErrorMessage: {response.ErrorMessage}");
                _logger.LogWarning($"ErrorException: {response.ErrorException}");
            }

            return false;
        }

        private JwtAuthenticator NewToken
        {
            get
            {
                return new JwtAuthenticator(NewTokenString);
            }
        }

        private string NewTokenString
        {
            get
            {
                if (_zoomOptions == null)
                {
                    throw new ArgumentException("Options not set.");
                }

                return new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(_zoomOptions.ApiSecret)
                    .AddClaim("iss", _zoomOptions.ApiKey)
                    .AddClaim("exp", DateTimeOffset.UtcNow.AddSeconds(5).ToUnixTimeSeconds())
                    .Encode();
            }
        }
    }
}
