using FluentAssertions;
using ING.iDealAdvanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace iDealAdvancedConnector.Tests
{
    [TestClass]
    public class CertificateTests
    {
        [TestMethod]
        public void When_getCertificate_from_storage_by_thumbPrint_should_return_correct_certificate()
        {
            //Arrange
            var thumbPrint = "B012A9AB7A7C1865EE2CA8E1CD18C5138B827CDB";

            //Act
            var cert = Connector.GetCertificate(thumbPrint);

            //Assert
            cert.Should().NotBeNull();
            cert.Thumbprint.Should().Be(thumbPrint);
        }
    }
}
