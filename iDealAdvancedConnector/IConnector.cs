using System;
using System.Security.Cryptography.X509Certificates;
using ING.iDealAdvanced.Data;

namespace ING.iDealAdvanced
{
    public interface IConnector
    {
        string ExpirationPeriod { get; set; }

        Uri MerchantReturnUrl { get; set; }

        X509Certificate2 ClientCertificate { get; set; }

        Transaction RequestTransaction(Transaction transaction);

        Issuers GetIssuerList();

        Transaction RequestTransactionStatus(string transactionId);
    }
}
