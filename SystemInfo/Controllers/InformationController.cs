using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DarkSky.Models;
using DarkSky.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static System.Math;

namespace SystemInfo.Controllers
{
    public class InformationController : Controller
    {
        public string PublicIP { get; set; } = "IP Lookup Failed";
        public double Long { get; set; }
        public double Latt { get; set; }
        public string City { get; set; }
        public string CurrentWeatherIcon { get; set; }
        public string WeatherAttribution { get; set; }
        public string CurrentTemp { get; set; } = "undetermined";
        public string DayWeatherSummary { get; set; }
        public string TempUnitOfMeasure { get; set; }
        public readonly IHostingEnvironment _hostEnv;

        public InformationController(IHostingEnvironment hostingEnvironment)
        {
            _hostEnv = hostingEnvironment;
        }

        public IActionResult GetInfo()
        {
            Models.InformationModel model = new Models.InformationModel()
            {
                OperatingSystem = RuntimeInformation.OSDescription,
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString()
            };

            string title = string.Empty;
            string OSArchitecture = string.Empty;

            if (model.OSArchitecture.ToUpper().Equals("X64"))
            {
                OSArchitecture = "64-bit";
            }
            else
            {
                OSArchitecture = "32 bits";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                title = $"Windows {OSArchitecture}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                title = $"OSX {OSArchitecture}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                title = $"Linux { OSArchitecture }";
            }

            GetLocationInfo().Wait();
            model.IPAddressString = PublicIP;

            GetWeatherInfo().Wait();
            model.CurrentIcon = CurrentWeatherIcon;

            model.WeatherBy = WeatherAttribution;
            model.CurrentTemperature = CurrentTemp;
            model.DailySummary = DayWeatherSummary;
            model.CurrentCity = City;
            model.UnitOfMeasure = TempUnitOfMeasure;

            model.InfoTitle = title;

            return View(model);

        }

        private DarkSkyService.OptionalParameters GetunitOfMeasure()
        {
            bool blnMetric = RegionInfo.CurrentRegion.IsMetric;
            DarkSkyService.OptionalParameters optParms = new DarkSkyService.OptionalParameters();

            if (blnMetric)
            {
                optParms.MeasurementUnits = "si";
                TempUnitOfMeasure = "C";
            }
            else
            {
                optParms.MeasurementUnits = "us";
                TempUnitOfMeasure = "F";
            }

            return optParms;
        }


        private string GetCurrentWeatherIcon(Icon ic)
        {
            string iconFilename = string.Empty;

            switch (ic)
            {
                case Icon.ClearDay:
                    iconFilename = "Sun.svg";
                    break;
                case Icon.ClearNight:
                    iconFilename = "Moon.svg";
                    break;

                case Icon.Cloudy:
                    iconFilename = "Cloud.svg";
                    break;

                case Icon.Fog:
                    iconFilename = "Cloud-Fog.svg";
                    break;

                case Icon.PartlyCloudyDay:
                    iconFilename = "Cloud-Sun.svg";
                    break;
                case Icon.PartlyCloudyNight:
                    iconFilename = "Cloud-Moon.svg";
                    break;

                case Icon.Rain:
                    iconFilename = "Cloud-Rain.svg";
                    break;

                case Icon.Snow:
                    iconFilename = "Snowflake.svg";
                    break;

                case Icon.Wind:
                    iconFilename = "Wind.svg";
                    break;
                default:
                    iconFilename = "Thermometer.svg";
                    break;
            }

            return iconFilename;

        }

        private async Task GetWeatherInfo()
        {
            string apiKey = "https://api.darksky.net/forecast/444d2c868cd209e1e72ba926eb8e0744/37.8267,-122.4233";
            DarkSkyService weather = new DarkSkyService(apiKey);
            DarkSkyService.OptionalParameters optParams = GetunitOfMeasure();
            var foreCast = await weather.GetForecast(Latt, Long, optParams);
            string iconFileName =
                GetCurrentWeatherIcon(foreCast.Response.Currently.Icon);
            string svgFile = Path.Combine(_hostEnv.ContentRootPath, "climacons", iconFileName);
            CurrentWeatherIcon = System.IO.File.ReadAllText($"{svgFile}");
            WeatherAttribution = foreCast.AttributionLine;
            DayWeatherSummary = foreCast.Response.Daily.Summary;
            if (foreCast.Response.Currently.Temperature.HasValue)
                CurrentTemp = Round(foreCast.Response.Currently.Temperature.Value, 0).ToString();
        }

        private async Task GetLocationInfo()
        {
            string API = "https://ipapi.co/8.8.8.8/json/";
            var httpClient = new HttpClient();
            string json = await httpClient.GetStringAsync(API);
            LocationInfo info = JsonConvert.DeserializeObject<LocationInfo>(json);

            PublicIP = "8.8.8.8";
            Long = -122.0838;
            Latt = 37.386;
            City = "Mountain View";
            //PublicIP = info.ip;
            //Long = info.longitude;
            //Latt = info.latitude;
            //City = info.city;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}