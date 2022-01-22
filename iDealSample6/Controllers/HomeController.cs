using System.Diagnostics;
using System.Web;
using iDealSampleCore.Custom;
using iDealSampleCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace iDealSampleCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private static List<SelectListItem>? _issuerDropDownList;
        private static readonly object _issuerLockObject = new();

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Issuer()
        {
            var issuerModel = new IssuerModel
            {
                AcquirerUrl = HttpUtility.HtmlEncode(_configuration["AcquirerUrl"]),
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"]),
                DateTime = null,
                DropDownListIssuers = new List<SelectListItem>()
            };

            return View("Issuer", issuerModel);
        }

        [HttpGet]
        public IActionResult GetIssuer()
        {
            var issuerListModel = new IssuerModel
            {
                AcquirerUrl = HttpUtility.HtmlEncode(_configuration["AcquirerUrl"]),
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"]),
                DateTime = DateTime.Now
            };

            issuerListModel.DropDownListIssuers = GetIssuerDropDownList(issuerListModel);

            return View("Issuer", issuerListModel);
        }

        private static List<SelectListItem> GetIssuerDropDownList(IssuerModel issuerListModel)
        {
            if (_issuerDropDownList != null)
            {
                return _issuerDropDownList;
            }

            lock (_issuerLockObject)
            {
                return _issuerDropDownList ??= issuerListModel
                    .GetIssuers()
                    .GetIssuerSelectList();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}