using System.ComponentModel.DataAnnotations;

namespace iDealSampleCore.Models
{
    public class RequestTransactionStatusModel
    {

        [Display(Name = "Transaction Id:")]
        public string TransactionId { get; set; } = string.Empty;

        [Display(Name = "Merchant Id:")]
        public string MerchantId { get; set; } = string.Empty;

        [Display(Name = "Sub Id:")]
        public string SubId { get; set; } = string.Empty;

        [Display(Name = "Acquirer Id:")]
        public string AcquirerId { get; set; } = string.Empty;

        [Display(Name = "Transaction Status:")]
        public string TransactionStatus { get; set; } = string.Empty;

        [Display(Name = "Consumer Name:")]
        public string ConsumerName { get; set; } = string.Empty;

        [Display(Name = "Consumer IBAN:")]
        public string ConsumerIban { get; set; } = string.Empty;

        [Display(Name = "Consumer BIC:")]
        public string ConsumerBic { get; set; } = string.Empty;

        [Display(Name = "Amount:")]
        public string Amount { get; set; } = string.Empty;

        [Display(Name = "Currency:")]
        public string Currency { get; set; } = string.Empty;

        [Display(Name = "Signature Value:")]
        public string SignatureValue { get; set; } = string.Empty;

        [Display(Name = "Fingerprint Value:")]
        public string Fingerprint { get; set; } = string.Empty;
    }
}
