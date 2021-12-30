using System.ComponentModel.DataAnnotations;

namespace iDealSampleCore.Models
{
    public class PageRequestTransactionModel
    {
        [RegularExpression("[A-Z]{6,6}[A-Z2-9][A-NP-Z0-9]([A-Z0-9]{3,3}){0,1}", ErrorMessage = "Format for issuer id not correct.")]
        [Required(ErrorMessage = "IssuerId is mandatory")]
        [Display(Name = "Issuer Id:")]
        public string IssuerId { get; set; }

        [RegularExpression("[a-zA-Z0-9]+", ErrorMessage = "PuchaseId contains illegal characters")]
        [Required(ErrorMessage = "PuchaseId is mandatory")]
        [Display(Name = "Purchase Id:")]
        public string PurchaseId { get; set; }

        [MaxLength(12)]
        [RegularExpression(@"\d+(.\d{1,2})?", ErrorMessage = "Amount is not a number")]
        [Required(ErrorMessage = "Amount is mandatory")]
        [Display(Name = "Amount:")]
        public string Amount { get; set; }

        [MaxLength(32)]
        [RegularExpression("^[-A-Za-z0-9= %*+,.@\"':;?()$]*$", ErrorMessage = "Description contains illegal characters")]
        [Required(ErrorMessage = "Description is mandatory")]
        [Display(Name = "Description:")]
        public string Description { get; set; }

        [MaxLength(40)]
        [RegularExpression("^[-A-Za-z0-9= %*+,.@\";':;?()$]*$", ErrorMessage = "Entrance Code contains illegal characters")]
        [Required(ErrorMessage = "Entrance Code is mandatory")]
        [Display(Name = "Entrance Code:")]
        public string EntranceCode { get; set; }

        [MaxLength(20)]
        [Display(Name = "Expiration Period:")]
        public string ExpirationPeriod { get; set; }

        [MaxLength(512)]
        [Display(Name = "Merchant Return Url:")]
        public string AcquirerUrl { get; set; }

        [Display(Name = "Merchant Id:")]
        public string MerchantId { get; set; }

        [Display(Name = "Sub Id:")]
        public string SubId { get; set; }

        [Display(Name = "Acquirer Id:")]
        public string AcquirerId { get; set; }

        [Display(Name = "Transaction Id:")]
        public string TransactionId { get; set; }

        [MaxLength(512)]
        [Display(Name = "Issuer Authentication Url:")]
        public string IssuerAuthenticationUrl { get; set; }

        public bool IssuerAuthenticationDisabled { get; set; } = true;
    }
}
