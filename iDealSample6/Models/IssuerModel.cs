using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace iDealSampleCore.Models
{
    public class IssuerModel
    {
        [Display(Name = "Merchant Id:")] 
        public string MerchantId { get; set; } = string.Empty;

        [Display(Name = "Sub Id:")]
        public string SubId { get; set; } = string.Empty;

        [Display(Name = "Acquirer Url:")]
        public string AcquirerUrl { get; set; } = string.Empty;

        [Display(Name = "DateTime:")]
        public DateTime? DateTime { get; set; }

        [Display(Name = "Issuer List:")]
        public List<SelectListItem> DropDownListIssuers { get; set; } = new();

        [RegularExpression("[A-Z]{6,6}[A-Z2-9][A-NP-Z0-9]([A-Z0-9]{3,3}){0,1}", ErrorMessage = "Format for issuer id not correct.")]
        public string SelectedIssuerId { get; set; } = string.Empty;
    }
}
