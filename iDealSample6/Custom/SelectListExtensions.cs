using System.Collections.Generic;
using System.Linq;
using iDealSampleCore.Models;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace iDealSampleCore.Custom
{
    internal static class SelectListExtensions
    {
        public static Issuers GetIssuers(this IssuerModel pageIssuerListModel)
        {
            var connector = Connector.CreateConnector(merchantId: pageIssuerListModel.MerchantId, merchantSubId: pageIssuerListModel.SubId, acquirerUrl: pageIssuerListModel.AcquirerUrl);

            var issuers = connector.GetIssuerList();

            return issuers;
        }

        public static List<SelectListItem> GetIssuerSelectList(this Issuers issuers)
        {
            if (!issuers.Countries.Any())
            {
                return new List<SelectListItem>();
            }

            var issuerSelectItemList = new List<SelectListItem> {
                new("Kies uw bank...", "-1", true),
                new("--Overige banken---", "-2")
            };

            foreach (var country in issuers.Countries)
            {
                var listGroup = new SelectListGroup { Name = country.CountryNames };

                issuerSelectItemList.AddRange(country.Issuers.Select(issuer => new SelectListItem(issuer.Name, issuer.Id) { Group = listGroup }));
            }

            return issuerSelectItemList;
        }
    }
}


