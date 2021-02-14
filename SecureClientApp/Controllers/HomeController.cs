using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using SampleClientApp.Helper;
using SampleClientApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SampleClientApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfidentialClientApplication _app;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfidentialClientApplication application, IConfiguration configuration)
        {
            _logger = logger;
            _app = application;
            _config = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var settings = _config.GetSection("ServiceAd");
            string[] ResourceIds = new string[] { settings.GetValue<string>("ResourceId") };

            AuthenticationResult result = null;
            result = await _app.AcquireTokenForClient(ResourceIds).ExecuteAsync();

            if (!string.IsNullOrEmpty(result.AccessToken))
            {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null ||
                   !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new
                      MediaTypeWithQualityHeaderValue("application/json"));
                }
                defaultRequestHeaders.Authorization =
                  new AuthenticationHeaderValue("bearer", result.AccessToken);

                HttpResponseMessage response = await httpClient.GetAsync(settings.GetValue<string>("BaseAddress"));
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    base.TempData["message"] = json;
                }
                else
                {
                    string content = await response.Content.ReadAsStringAsync();
                    base.TempData["message"] = content;
                } 
            }
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
