﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using iDealSampleCore.Custom;
using iDealSampleCore.Models;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace iDealSampleCore.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration _configuration;

        private static List<SelectListItem> _issuerListModel;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PageIssuerList()
        {
            var pageIssuerListModel = new PageIssuerModel
            {
                AcquirerUrl = HttpUtility.HtmlEncode(_configuration["AcquirerUrl"]),
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"]),
                DateTime = null,
                IssuersDropDownList = new List<SelectListItem>()
            };

            return View(pageIssuerListModel);
        }

        [HttpGet]
        public IActionResult GetIssuerList()
        {
            var pageIssuerListModel = new PageIssuerModel
            {
                AcquirerUrl = HttpUtility.HtmlEncode(_configuration["AcquirerUrl"]),
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"]),
                DateTime = DateTime.Now
            };

            _issuerListModel ??= pageIssuerListModel
                .GetIssuers()
                .GetIssuerSelectList();

            pageIssuerListModel.IssuersDropDownList = _issuerListModel;

            return View("PageIssuerList", pageIssuerListModel);
        }

        [HttpPost]
        public IActionResult TransActionRequest(PageIssuerModel pageIssuerListModel)
        {
            if (TryValidateModel(pageIssuerListModel))
            {
                var pageRequestTransactionModel = new PageRequestTransactionModel
                {
                    IssuerId = pageIssuerListModel.SelectedIssuerId,
                    ExpirationPeriod = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["ExpirationPeriod"]),
                    MerchantUrl = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["MerchantReturnUrl"]),
                    MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                    SubId = HttpUtility.HtmlEncode(_configuration["SubId"])
                };

                return View("PageRequestTransaction", pageRequestTransactionModel);
            }

            pageIssuerListModel.AcquirerUrl = HttpUtility.HtmlEncode(_configuration["MerchantReturnUrl"]);
            pageIssuerListModel.MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]);
            pageIssuerListModel.SubId = HttpUtility.HtmlEncode(_configuration["SubId"]);
            pageIssuerListModel.DateTime = DateTime.Now;
            pageIssuerListModel.IssuersDropDownList = _issuerListModel;

            return View("PageIssuerList", pageIssuerListModel);
        }

        [HttpGet]
        public IActionResult PageRequestTransaction()
        {
            var pageRequestTransactionModel = new PageRequestTransactionModel
            {
                ExpirationPeriod = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["ExpirationPeriod"]),
                MerchantUrl = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["MerchantReturnUrl"]),
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"])
            };

            return View(pageRequestTransactionModel);
        }

        [HttpPost]
        public IActionResult PageRequestTransaction(PageRequestTransactionModel pageRequestTransactionModel)
        {
            if (TryValidateModel(pageRequestTransactionModel) && RequestTransaction(pageRequestTransactionModel))
            {
                pageRequestTransactionModel.IssuerAuthenticationValid = true;
            }

            return View("PageRequestTransaction", pageRequestTransactionModel);
        }

        [HttpGet]
        public IActionResult PageRequestTransactionStatus(string trxid, string ec)
        {
            var pageRequestTransactionModelStatus = new PageRequestTransactionStatusModel
            {
                TransactionId = trxid,
                MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]),
                SubId = HttpUtility.HtmlEncode(_configuration["SubId"])
            };

            return View(pageRequestTransactionModelStatus);
        }

        [HttpPost]
        public IActionResult GetRequestTransactionStatus(PageRequestTransactionStatusModel pageRequestTransactionModelStatus)
        {
            pageRequestTransactionModelStatus.MerchantId = HttpUtility.HtmlEncode(_configuration["MerchantId"]);
            pageRequestTransactionModelStatus.SubId = HttpUtility.HtmlEncode(_configuration["SubId"]);

            RequestTransactionStatus(pageRequestTransactionModelStatus);

            return View("PageRequestTransactionStatus", pageRequestTransactionModelStatus);
        }

        private static bool RequestTransaction(PageRequestTransactionModel pageRequestTransactionModel)
        {
            try
            {
                Transaction transaction = new Transaction();

                if (!decimal.TryParse(pageRequestTransactionModel.Amount, NumberStyles.Currency, new CultureInfo("en-US"), out var amount))
                {
                    //LabelErrorValue.Text = String.Format(Properties.Resources.IllegalNumber, "Amount");

                    return false;
                }

                transaction.Amount = amount;
                transaction.Description = pageRequestTransactionModel.Description;
                transaction.PurchaseId = pageRequestTransactionModel.PurchaseId;
                transaction.IssuerId = pageRequestTransactionModel.IssuerId;
                transaction.EntranceCode = pageRequestTransactionModel.EntranceCode;

                var connector = Connector.CreateConnector();
                connector.ExpirationPeriod = HttpUtility.HtmlEncode(pageRequestTransactionModel.ExpirationPeriod);
                connector.MerchantReturnUrl = new Uri(pageRequestTransactionModel.MerchantUrl);

                transaction = connector.RequestTransaction(transaction);
                pageRequestTransactionModel.TransactionId = HttpUtility.HtmlEncode(transaction.Id);
                pageRequestTransactionModel.IssuerAuthenticationUrl = HttpUtility.HtmlDecode(transaction.IssuerAuthenticationUrl.ToString());
                pageRequestTransactionModel.AcquirerId = HttpUtility.HtmlEncode(transaction.AcquirerId);

                return true;
            }
            catch (IDealException ex)
            {
                //LabelErrorValue.Text = ex.ErrorRes.Error.consumerMessage;
                return false;
            }
        }

        private static void RequestTransactionStatus(PageRequestTransactionStatusModel pageRequestTransactionModelStatus)
        {
            try
            {
                var connector = Connector.CreateConnector(merchantId: pageRequestTransactionModelStatus.MerchantId, merchantSubId: pageRequestTransactionModelStatus.SubId);
                var transaction = connector.RequestTransactionStatus(pageRequestTransactionModelStatus.TransactionId);

                pageRequestTransactionModelStatus.AcquirerId = transaction.AcquirerId;
                pageRequestTransactionModelStatus.TransactionStatus = transaction.Status.ToString();
                pageRequestTransactionModelStatus.ConsumerName = transaction.ConsumerName;
                pageRequestTransactionModelStatus.Fingerprint = transaction.Fingerprint;
                pageRequestTransactionModelStatus.ConsumerIban = transaction.ConsumerIBAN;
                pageRequestTransactionModelStatus.ConsumerBic = transaction.ConsumerBIC;
                pageRequestTransactionModelStatus.Amount = transaction.Amount.ToString(CultureInfo.InvariantCulture);
                pageRequestTransactionModelStatus.Currency = transaction.Currency;

                var signatureString = ByteArrayToHexString(transaction.SignatureValue);

                // Place newlines in Hex String
                for (var i = 512; i > 0; i -= 32)
                {
                    signatureString = signatureString.Substring(0, i) + " " + signatureString.Substring(i);
                }

                pageRequestTransactionModelStatus.SignatureValue = signatureString;
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
            _logger.LogError($"Error, RequestId/TraceIdentifier = {Activity.Current?.Id ?? HttpContext.TraceIdentifier} ");

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
