using System;
using System.Diagnostics;
using System.Linq;
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
                DateTime = null
            };

            pageIssuerListModel.DropDownListIssuers = new SelectList(Enumerable.Empty<string>());

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

            pageIssuerListModel.DropDownListIssuers = new SelectList(Enumerable.Empty<string>());
            ViewData["Issuers"] = new SelectList(Enumerable.Empty<string>());

            return View("PageIssuerList", pageIssuerListModel);
        }

        public IActionResult TransActionRequest()
        {
            var pageIssuerListModel = new PageIssuerListModel
            {
                AcquirerUrl = _configuration["AcquirerUrl"],
                MerchantId = _configuration["MerchantId"],
                SubId = _configuration["SubId"],
                DateTime = null
            };

            return View("PageIssuerList", pageIssuerListModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
