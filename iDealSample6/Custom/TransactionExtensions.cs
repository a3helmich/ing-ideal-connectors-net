using System.Globalization;
using System.Web;
using iDealSampleCore.Models;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;

namespace iDealSampleCore.Custom
{
    internal static class TransactionExtensions
    {
        public static bool SetTransactionModel(this TransactionModel transactionModel)
        {
            try
            {
                Transaction transaction = new Transaction();

                if (!decimal.TryParse(transactionModel.Amount, NumberStyles.Currency, new CultureInfo("en-US"), out var amount))
                {
                    //LabelErrorValue.Text = String.Format(Properties.Resources.IllegalNumber, "Amount");

                    return false;
                }

                transaction.Amount = amount;
                transaction.Description = transactionModel.Description;
                transaction.PurchaseId = transactionModel.PurchaseId;
                transaction.IssuerId = transactionModel.IssuerId;
                transaction.EntranceCode = transactionModel.EntranceCode;

                var connector = Connector.CreateConnector();

                connector.ExpirationPeriod = HttpUtility.HtmlEncode(transactionModel.ExpirationPeriod);
                connector.MerchantReturnUrl = new Uri(transactionModel.MerchantUrl);

                transaction = connector.RequestTransaction(transaction);
                transactionModel.TransactionId = HttpUtility.HtmlEncode(transaction.Id);
                transactionModel.IssuerAuthenticationUrl = HttpUtility.HtmlDecode(transaction.IssuerAuthenticationUrl.ToString());
                transactionModel.AcquirerId = HttpUtility.HtmlEncode(transaction.AcquirerId);

                return true;
            }
            catch (IDealException)
            {
                //LabelErrorValue.Text = ex.ErrorRes.Error.consumerMessage;
                return false;
            }
        }
    }
}
