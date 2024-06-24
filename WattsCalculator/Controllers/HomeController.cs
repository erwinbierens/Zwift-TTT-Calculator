using Microsoft.AspNetCore.Mvc;
using WattsCalculator.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Html;
using System.Text;
using System.Linq;

namespace WattsCalculator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Initialize a list of 6 riders
            var riders = new List<Rider>();
            for (int i = 0; i < 6; i++)
            {
                riders.Add(new Rider());
            }

            ViewBag.ScalingFactorOptions = GenerateScalingFactorOptions(1.3);
            ViewBag.ResultsAvailable = false;
            return View(riders);
        }

        [HttpPost]
        public IActionResult Index(List<Rider> riders, double scalingFactor, double velocity)
        {
            foreach (var rider in riders)
            {
                // Calculate Power Output in Watts per Kilogram
                if (rider.Weight > 0)
                {
                    rider.PowerOutputWattsPerKg = rider.FTP / rider.Weight;
                }
                else
                {
                    rider.PowerOutputWattsPerKg = 0; // or handle this case as per your application logic
                }
            }

            // Calculate average power output of the team in Watts and W/kg
            var validRiders = riders.Where(r => r.PowerOutputWattsPerKg > 0).ToList();
            double averagePowerOutputWatts = 0;
            double averagePowerOutputWattsPerKg = 0;
            if (validRiders.Count > 0)
            {
                double totalPowerOutputWatts = validRiders.Sum(r => r.FTP);
                double totalPowerOutputWattsPerKg = validRiders.Sum(r => r.FTP / r.Weight);
                averagePowerOutputWatts = totalPowerOutputWatts / validRiders.Count;
                averagePowerOutputWattsPerKg = totalPowerOutputWattsPerKg / validRiders.Count;
            }

            // Calculate pull wattage and watt/kg for each rider and each position using the given formula
            double selectedScalingFactor = scalingFactor;
            foreach (var rider in riders)
            {
                rider.PullWattages = new double[6];
                rider.PullWattagesPerKg = new double[6];
                rider.PowerZones = new string[6];

                for (int i = 0; i < 6; i++)
                {
                    double powerOutput = CalculatePowerOutput(rider.Weight, rider.Height, velocity) * selectedScalingFactor;
                    rider.PullWattages[i] = powerOutput;
                    rider.PullWattagesPerKg[i] = rider.Weight > 0 ? powerOutput / rider.Weight : 0;
                    rider.PowerZones[i] = GetPowerZone(powerOutput, rider.FTP);
                }
            }

            ViewBag.AveragePowerOutputWatts = Math.Floor(averagePowerOutputWatts);
            ViewBag.AveragePowerOutputWattsPerKg = averagePowerOutputWattsPerKg;
            ViewBag.ResultsAvailable = true;
            ViewBag.ScalingFactorOptions = GenerateScalingFactorOptions(scalingFactor);
            ViewBag.Velocity = velocity;
            return View(riders);
        }

        private static double CalculatePowerOutput(double weight, double height, double velocity)
        {
            // P = 1.86e-02 * w * v - 5.37e-04 * v^3 + 2.23e-05 * w * v^3 + 1.33e-05 * h * v^3
            double vCubed = Math.Pow(velocity, 3);
            return 1.86e-02 * weight * velocity - 5.37e-04 * vCubed + 2.23e-05 * weight * vCubed + 1.33e-05 * height * vCubed;
        }

        private static string GetPowerZone(double power, double ftp)
        {
            double percentage = (power / ftp) * 100;
            if (percentage <= 70) return "green";
            if (percentage <= 99) return "orange";
            return "red";
        }

        private static IHtmlContent GenerateScalingFactorOptions(double selectedValue)
        {
            var factors = new[] { 1.1, 1.2, 1.3, 1.4 };
            var sb = new StringBuilder();

            foreach (var factor in factors)
            {
                sb.AppendFormat("<option value=\"{0}\"{1}>{0}</option>", factor, factor == selectedValue ? " selected" : string.Empty);
            }

            return new HtmlString(sb.ToString());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
