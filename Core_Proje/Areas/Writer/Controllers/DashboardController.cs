using DataAccessLayer.Concrete;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace Core_Proje.Areas.Writer.Controllers
{
    [Area("Writer")]
    public class DashboardController : Controller
    {
        private readonly UserManager<WriterUser> _userManager;

        public DashboardController(UserManager<WriterUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var values = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.v = values.Name + " " + values.Surname;

            // Hava durumu alınamazsa dashboard diğer bilgileri göstermeye devam eder.
            ViewBag.v5 = "—";
            string? api = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY");

            if (!string.IsNullOrWhiteSpace(api))
            {
                try
                {
                    using var client = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(5)
                    };
                    string connection =
                        "https://api.openweathermap.org/data/2.5/weather" +
                        "?q=istanbul&mode=xml&lang=tr&units=metric&appid=" +
                        Uri.EscapeDataString(api.Trim());
                    string response = await client.GetStringAsync(connection);
                    XDocument document = XDocument.Parse(response);
                    string? temperature = document.Descendants("temperature")
                        .FirstOrDefault()?.Attribute("value")?.Value;

                    if (!string.IsNullOrWhiteSpace(temperature))
                    {
                        ViewBag.v5 = temperature;
                    }
                }
                catch (HttpRequestException)
                {
                    // İstek ayrıntıları gizli anahtar içerebileceğinden loglanmaz.
                }
                catch (OperationCanceledException)
                {
                    // Zaman aşımında varsayılan gösterim korunur.
                }
                catch (System.Xml.XmlException)
                {
                    // Geçersiz XML yanıtında varsayılan gösterim korunur.
                }
            }

            //statictics
            Context c = new Context();
            ViewBag.v1 = c.WriterMessages.Where(x => x.Receiver == values.Email).Count();
            ViewBag.v2 = c.Announcements.Count();
            ViewBag.v3 = c.Users.Count();
            ViewBag.v4 = c.Skills.Count();

            return View();
        }
    }
}

