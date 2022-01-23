using System.Diagnostics;
using System.Globalization;
using System.Web;
using iDealSampleCore.Custom;
using iDealSampleCore.Models;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace iDealSampleCore.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(/*ILogger<HomeController> logger, */IConfiguration configuration)
        {
            //_logger = logger;
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
                IssuersDropDownList = new List<SelectListItem>()
            };

            return View("Issuer", issuerModel);
        }

        [HttpGet]
        public IActionResult GetIssuers()
        {
            var issuerModel = new IssuerModel
            {
                AcquirerUrl = HttpUtility.HtmlEncode(_configuration["AcquirerUrl"]),
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"]),
                DateTime = DateTime.Now
            };

            issuerModel.SetIssuersDropDownList();

            return View("Issuer", issuerModel);
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
        public IActionResult TransActionRequest(IssuerModel issuerModel)
        {
            if (TryValidateModel(issuerModel))
            {
                var transactionModel = new TransactionModel
                {
                    IssuerId = issuerModel.SelectedIssuerId,
                    ExpirationPeriod = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["ExpirationPeriod"]) ?? string.Empty,
                    MerchantUrl = HttpUtility.HtmlEncode(_configuration["MerchantReturnUrl"]),
                    MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                    SubId = HttpUtility.HtmlEncode(_configuration["SubId"])
                };

                return View("Transaction", transactionModel);
            }

            issuerModel.AcquirerUrl = HttpUtility.HtmlEncode(_configuration["MerchantReturnUrl"]);
            issuerModel.MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]);
            issuerModel.SubId = HttpUtility.HtmlEncode(_configuration["SubId"]);
            issuerModel.DateTime = DateTime.Now;

            issuerModel.SetIssuersDropDownList();

            return View("Issuer", issuerModel);
        }

        [HttpPost]
        public IActionResult RequestTransaction(TransactionModel transactionModel)
        {
            transactionModel.IssuerAuthenticationValid =
                TryValidateModel(transactionModel) && transactionModel.SetTransactionModel();

            transactionModel.MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]);
            transactionModel.SubId = HttpUtility.HtmlEncode(_configuration["SubId"]);


            return View("Transaction", transactionModel);
        }


        [HttpGet]
        public IActionResult TransactionStatus(string trxid, string ec)
        {
            var transactionStatusModel = new TransactionStatusModel
            {
                TransactionId = trxid,
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"])
            };

            return View(transactionStatusModel);
        }

        [HttpPost]
        public IActionResult TransactionStatus(TransactionStatusModel transactionStatusModel)
        {
            transactionStatusModel.MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]);
            transactionStatusModel.SubId = HttpUtility.HtmlEncode(_configuration["SubId"]);

            RequestTransactionStatus(transactionStatusModel);

            return View("TransactionStatus", transactionStatusModel);
        }

        private static void RequestTransactionStatus(TransactionStatusModel transactionStatusModel)
        {
            try
            {
                var connector = Connector.CreateConnector(merchantId: transactionStatusModel.MerchantId, merchantSubId: transactionStatusModel.SubId);
                var transaction = connector.RequestTransactionStatus(transactionStatusModel.TransactionId);

                transactionStatusModel.AcquirerId = transaction.AcquirerId;
                transactionStatusModel.TransactionStatus = transaction.Status.ToString();
                transactionStatusModel.ConsumerName = transaction.ConsumerName;
                transactionStatusModel.Fingerprint = transaction.Fingerprint;
                transactionStatusModel.ConsumerIban = transaction.ConsumerIBAN;
                transactionStatusModel.ConsumerBic = transaction.ConsumerBIC;
                transactionStatusModel.Amount = transaction.Amount.ToString(CultureInfo.InvariantCulture);
                transactionStatusModel.Currency = transaction.Currency;

                var signatureString = ByteArrayToHexString(transaction.SignatureValue);

                // Place newlines in Hex String
                for (var i = 512; i > 0; i -= 32)
                {
                    signatureString = signatureString.Substring(0, i) + " " + signatureString.Substring(i);
                }

                transactionStatusModel.SignatureValue = signatureString;
            }
            catch (IDealException ex)
            {
                //LabelErrorValue.Text = ex.ErrorRes.Error.consumerMessage;
            }
        }

        private static string ByteArrayToHexString(byte[] bytes)
        {
            return string.Join(string.Empty, bytes.Select(b => b.ToString("X2")));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}