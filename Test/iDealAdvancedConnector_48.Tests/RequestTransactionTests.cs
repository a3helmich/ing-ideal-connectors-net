using FluentAssertions;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Prepare.Utils;

namespace iDealAdvancedConnector_48.Tests
{
    [TestClass]
    public class RequestTransactionTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            TestPrepare.CopyApplicationSettingsToActiveConfiguration();
        }

        [TestMethod]
        [Ignore("Requires a specific certificate (thumbprint B012A9AB7A7C1865EE2CA8E1CD18C5138B827CDB) installed in the build agent's certificate store, not present on KONFIDENCE8.")]
        public void When_connector_RequestTransaction_called_should_return_Transaction()
        {
            // arrange
            const string description = @"description";

            Transaction transaction = new Transaction
            {
                Amount = new decimal(10.10),
                Description = description,
                PurchaseId = @"purchaseid",
                IssuerId = @"INGBNL2A",
                EntranceCode = @"entranceCode"
            };

            IConnector connector = Connector.CreateConnector();
            //connector.ExpirationPeriod = HttpUtility.HtmlEncode(TextBoxExpirationPeriodValue.Text);
            //connector.MerchantReturnUrl = new Uri(TextBoxMerchantReturnUrlValue.Text);

            // act
            transaction = connector.RequestTransaction(transaction);

            // assert
            transaction.Description.Should().Be(description);
        }
    }
}
