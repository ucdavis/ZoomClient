using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.Web.Models;
using ZoomClient;
using ZoomClient.Domain;

namespace Sample.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Zoom _zoomClient;
        private readonly ZoomClient.Options _options;

        public HomeController(ILogger<HomeController> logger, ZoomClient.Zoom zoomClient, IOptions<ZoomClient.Options> options)
        {
            _logger = logger;
            _zoomClient = zoomClient;
            _options = options.Value;

            _zoomClient.Options = _options;
        }

        public IActionResult Index()
        {
            var user = _zoomClient.GetUser("me");

            return View(user);
        }

        public IActionResult Privacy()
        {
            var request = new MeetingRequest
            {
                topic = "Test-Meeting",
                start_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                duration = 60,
                agenda = "banner-agenda-link",
                password = "123456",
                recurrence = new Recurrence
                {
                    type = 2,
                    repeat_interval = 1,
                    weekly_days = "2,4",
                    end_date_time = DateTime.Now.AddMonths(4).Date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
                }
            };

            var result = _zoomClient.CreateMeetingForUser(request, "emhenn@ucdavis.edu");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
