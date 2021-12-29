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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

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

        public IActionResult GetIssuerList()
        {
            var pageIssuerListModel = new PageIssuerListModel
            {
                AcquirerUrl = _configuration["AcquirerUrl"],
                MerchantId = _configuration["MerchantId"],
                SubId = _configuration["SubId"],
                DateTime = DateTime.Now
            };

            pageIssuerListModel.DropDownListIssuers =
                pageIssuerListModel
                    .GetIssuers()
                    .GetIssuerSelectList();

            return View("PageIssuerList", pageIssuerListModel);
        }

        //[HttpPost]
        public IActionResult TransActionRequest(PageIssuerListModel pageIssuerListModel)
        {
            //var pageIssuerListModel = new PageIssuerListModel
            //{
            //    AcquirerUrl = _configuration["AcquirerUrl"],
            //    MerchantId = _configuration["MerchantId"],
            //    SubId = _configuration["SubId"],
            //    DateTime = null
            //};

            return View("PageIssuerList", pageIssuerListModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError($"Error, RequestId/TraceIdentifier = {Activity.Current?.Id ?? HttpContext.TraceIdentifier} ");

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
