using System.Linq;
using FluentAssertions;
using ING.iDealAdvanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Prepare.Utils;

namespace iDealAdvancedConnector_50.Tests
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
        public void When_getIssuerList_called_should_return_list_of_issuers()
        {
            //Arrange
            var connector = new Connector();

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
