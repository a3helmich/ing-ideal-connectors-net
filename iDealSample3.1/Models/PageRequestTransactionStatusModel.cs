using System.ComponentModel.DataAnnotations;

namespace iDealSampleCore.Models
{
    public class PageRequestTransactionStatusModel
    {

        [Display(Name = "Transaction Id:")]
        public string TransactionId { get; set; }

        [Display(Name = "Merchant Id:")]
        public string MerchantId { get; set; }

        [Display(Name = "Sub Id:")]
        public string SubId { get; set; }

        [Display(Name = "Acquirer Id:")]
        public string AcquirerId { get; set; }

        [Display(Name = "Transaction Status:")]
        public string TransactionStatus { get; set; }

        [Display(Name = "Consumer Name:")]
        public string ConsumerName { get; set; }

        [Display(Name = "Consumer IBAN:")]
        public string ConsumerIban { get; set; }

        [Display(Name = "Consumer BIC:")]
        public string ConsumerBic { get; set; }

        [Display(Name = "Amount:")]
        public string Amount { get; set; }

        [Display(Name = "Currency:")]
        public string Currency { get; set; }

        [Display(Name = "Signature Value:")]
        public string SignatureValue { get; set; }

        [Display(Name = "Fingerprint Value:")]
        public string Fingerprint { get; set; }
    }
}
