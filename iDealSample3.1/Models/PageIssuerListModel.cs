using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace iDealSampleCore.Models
{
    public class PageIssuerListModel
    {
        [Display(Name = "Merchant Id:")]
        public string MerchantId { get; set; }

        [Display(Name = "Sub Id:")]
        public string SubId { get; set; }
        
        [Display(Name = "Acquirer Url:")]
        public string AcquirerUrl { get; set; }

        [Display(Name = "DateTime:")]
        public DateTime? DateTime { get; set; }

        [Display(Name = "Issuer List:")]
        public IEnumerable<SelectListItem> DropDownListIssuers { get; set; }
    }
}
