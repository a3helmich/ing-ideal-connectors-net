using System.ComponentModel.DataAnnotations;

namespace iDealSampleCore.Models
{
    public class RequestTransactionModel
    {
        [RegularExpression("[A-Z]{6,6}[A-Z2-9][A-NP-Z0-9]([A-Z0-9]{3,3}){0,1}", ErrorMessage = "Format for issuer id not correct.")]
        [Required(ErrorMessage = "IssuerId is mandatory")]
        [Display(Name = "Issuer Id:")]
        public string IssuerId { get; set; } = string.Empty;

        [RegularExpression("[a-zA-Z0-9]+", ErrorMessage = "PuchaseId contains illegal characters")]
        [Required(ErrorMessage = "PuchaseId is mandatory")]
        [Display(Name = "Purchase Id:")]
        public string PurchaseId { get; set; } = string.Empty;

        [MaxLength(12)]
        [RegularExpression(@"\d+(.\d{1,2})?", ErrorMessage = "Amount is not a number")]
        [Required(ErrorMessage = "Amount is mandatory")]
        [Display(Name = "Amount:")]
        public string Amount { get; set; } = string.Empty;

        [MaxLength(32)]
        [RegularExpression("^[-A-Za-z0-9= %*+,.@\"':;?()$]*$", ErrorMessage = "Description contains illegal characters")]
        [Required(ErrorMessage = "Description is mandatory")]
        [Display(Name = "Description:")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(40)]
        [RegularExpression("^[-A-Za-z0-9= %*+,.@\";':;?()$]*$", ErrorMessage = "Entrance Code contains illegal characters")]
        [Required(ErrorMessage = "Entrance Code is mandatory")]
        [Display(Name = "Entrance Code:")]
        public string EntranceCode { get; set; } = string.Empty;

        [MaxLength(20)]
        [Display(Name = "Expiration Period:")]
        public string ExpirationPeriod { get; set; } = string.Empty;

        [MaxLength(512)]
        [Display(Name = "Merchant Return Url:")]
        public string MerchantUrl { get; set; } = string.Empty;

        [Display(Name = "Merchant Id:")]
        public string MerchantId { get; set; } = string.Empty;

        [Display(Name = "Sub Id:")]
        public string SubId { get; set; } = string.Empty;

        [Display(Name = "Acquirer Id:")]
        public string AcquirerId { get; set; } = string.Empty;

        [Display(Name = "Transaction Id:")]
        public string TransactionId { get; set; } = string.Empty;

        [MaxLength(512)]
        [Display(Name = "Issuer Authentication Url:")]
        public string IssuerAuthenticationUrl { get; set; } = string.Empty;

        public bool IssuerAuthenticationValid { get; set; }
    }
}
