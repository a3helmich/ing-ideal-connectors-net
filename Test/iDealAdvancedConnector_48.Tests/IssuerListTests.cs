using System.Linq;
using FluentAssertions;
using ING.iDealAdvanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Prepare.Utils;

namespace iDealAdvancedConnector_48.Tests
{

    [TestClass]
    public class IssuerListTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            TestPrepare.CopyApplicationSettingsToActiveConfiguration();
        }

        [TestMethod]
        [Ignore("Requires a specific certificate (thumbprint B012A9AB7A7C1865EE2CA8E1CD18C5138B827CDB) installed in the build agent's certificate store, not present on KONFIDENCE8.")]
        public void When_connector_getIssuerList_called_should_return_list_of_issuers()
        {
            //Arrange
            var connector = Connector.CreateConnector();

            //Act
            var issuers = connector.GetIssuerList();

            //Assert
            issuers.Should().NotBeNull();
            issuers.Countries.Should().HaveCount(1);
            issuers.Countries.First().CountryNames.Should().BeEquivalentTo("nederland");
            issuers.Countries.First().Issuers.Should().HaveCount(2);
        }
    }
}
