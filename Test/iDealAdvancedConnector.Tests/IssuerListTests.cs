using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using ING.iDealAdvanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace iDealAdvancedConnector.Tests
{
    [TestClass]
    public class IssuerListTests
    {
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
