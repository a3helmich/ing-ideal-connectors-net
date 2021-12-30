using System;
using System.Collections.Generic;
using System.Diagnostics;
using iDealSampleCore.Custom;
using iDealSampleCore.Models;
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
            var pageIssuerListModel = new PageIssuerListModel
            {
                AcquirerUrl = _configuration["AcquirerUrl"],
                MerchantId = _configuration["MerchantId"],
                SubId = _configuration["SubId"],
                DateTime = null,
                DropDownListIssuers = new List<SelectListItem>()
            };

            return View(pageIssuerListModel);
        }

        [HttpGet]
        public IActionResult GetIssuerList()
        {
            var pageIssuerListModel = new PageIssuerListModel
            {
                AcquirerUrl = _configuration["AcquirerUrl"],
                MerchantId = _configuration["MerchantId"],
                SubId = _configuration["SubId"],
                DateTime = DateTime.Now
            };

            _issuerListModel ??= pageIssuerListModel
                .GetIssuers()
                .GetIssuerSelectList();

            pageIssuerListModel.DropDownListIssuers = _issuerListModel;

            return View("PageIssuerList", pageIssuerListModel);
        }

        [HttpPost]
        public IActionResult TransActionRequest(PageIssuerListModel pageIssuerListModel)
        {
            if (TryValidateModel(pageIssuerListModel))
            {
                var pageRequestTransactionModel = new PageRequestTransactionModel
                {
                    IssuerId = pageIssuerListModel.SelectedIssuerId
                };

                return View("PageRequestTransaction", pageRequestTransactionModel);
            }

            pageIssuerListModel.AcquirerUrl = _configuration["AcquirerUrl"];
            pageIssuerListModel.MerchantId = _configuration["MerchantId"];
            pageIssuerListModel.SubId = _configuration["SubId"];
            pageIssuerListModel.DateTime = DateTime.Now;
            pageIssuerListModel.DropDownListIssuers = _issuerListModel;

            return View("PageIssuerList", pageIssuerListModel);
        }

        [HttpGet]
        public IActionResult PageRequestTransaction()
        {
            var pageRequestTransactionModel = new PageRequestTransactionModel
            {
                IssuerId = "ING?"
            };

            return View(pageRequestTransactionModel);
        }

        [HttpPost]
        public IActionResult PageRequestTransaction(PageRequestTransactionModel pageRequestTransactionModel)
        {
            return View("PageRequestTransaction", pageRequestTransactionModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError($"Error, RequestId/TraceIdentifier = {Activity.Current?.Id ?? HttpContext.TraceIdentifier} ");

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
