using System.Diagnostics;
using System.Web;
using iDealSampleCore.Custom;
using iDealSampleCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ConfigurationManager = System.Configuration.ConfigurationManager;

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
        public IActionResult GetIssuers()
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

        [HttpGet]
        public IActionResult Transaction()
        {
            var transactionModel = new TransactionModel
            {
                ExpirationPeriod = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["ExpirationPeriod"]) ?? string.Empty,
                MerchantUrl = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["MerchantReturnUrl"]) ?? string.Empty,
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"])
            };

            return View(transactionModel);
        }

        [HttpPost]
        public IActionResult TransActionRequest(IssuerModel pageIssuerListModel)
        {
            if (TryValidateModel(pageIssuerListModel))
            {
                var pageRequestTransactionModel = new TransactionModel
                {
                    IssuerId = pageIssuerListModel.SelectedIssuerId,
                    ExpirationPeriod = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["ExpirationPeriod"]) ?? string.Empty,
                    MerchantUrl = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["MerchantReturnUrl"]) ?? string.Empty,
                    MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                    SubId = HttpUtility.HtmlEncode(_configuration["SubId"])
                };

                return View("Transaction", pageRequestTransactionModel);
            }

            pageIssuerListModel.AcquirerUrl = HttpUtility.HtmlEncode(_configuration["MerchantReturnUrl"]);
            pageIssuerListModel.MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]);
            pageIssuerListModel.SubId = HttpUtility.HtmlEncode(_configuration["SubId"]);
            pageIssuerListModel.DateTime = DateTime.Now;
            pageIssuerListModel.DropDownListIssuers = _issuerDropDownList!;

            return View("Issuer", pageIssuerListModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}