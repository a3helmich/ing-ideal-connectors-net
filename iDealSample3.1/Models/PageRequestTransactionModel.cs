using System.ComponentModel.DataAnnotations;

namespace iDealSampleCore.Models
{
    public class PageRequestTransactionModel
    {
        [Display(Name = "Issuer Id:")]
        public string IssuerId { get; set; }

        [Display(Name = "Purchase Id:")]
        public string PurchaseId { get; set; }

        [Display(Name = "Amount:")]
        public double Amount { get; set; }

        [Display(Name = "Description:")]
        public string Description { get; set; }

        [Display(Name = "Entrance Code:")]
        public string EntranceCode { get; set; }

        [Display(Name = "Expiration Period:")]
        public string ExpirationPeriod { get; set; }

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

        [Display(Name = "Issuer Authentication:")]
        public string IssuerAuthentication { get; set; }

        [Display(Name = "Url:")]
        public string Url { get; set; }
    }
}
