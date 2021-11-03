using FluentAssertions;
using ING.iDealAdvanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Prepare.Utils;

namespace iDealAdvancedConnector_48.Tests
{
    [TestClass]
    public class CertificateTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            TestPrepare.CopyApplicationSettingsToActiveConfiguration();
        }

        [TestMethod]
        public void When_getCertificate_from_storage_by_thumbPrint_With_impersonation_Should_return_the_correct_certificate()
        {
            //Arrange
            const string thumbPrint = "B012A9AB7A7C1865EE2CA8E1CD18C5138B827CDB";

            //Act
            var cert = Connector.GetCertificate(thumbPrint);

            //Assert
            cert.Should().NotBeNull();
            cert.Thumbprint.Should().Be(thumbPrint);
        }
 
        [TestMethod]
        public void When_getCertificate_from_storage_by_thumbPrint_Without_impersonation_Should_still_return_the_correct_certificate()
        {
            //Arrange
            const string thumbPrint = "B012A9AB7A7C1865EE2CA8E1CD18C5138B827CDB";

            //Act
            var cert = Connector.TryGetCertificateFromStore(thumbPrint);

            //Assert
            cert.Should().NotBeNull();
            cert.Thumbprint.Should().Be(thumbPrint);
        }
    }
}
