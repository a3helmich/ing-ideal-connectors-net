using iDealSampleCore.Models;
using ING.iDealAdvanced;
using ING.iDealAdvanced.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using iDealSampleCore.Dto;
using AutoMapper;

namespace iDealSampleCore.Custom
{
    internal static class IssuerExtensions
    {
        private static readonly Mapper _mapper;

        private static readonly TimeSpan _oneDay = new(1, 0, 0, 0);

        private static List<SelectListItem>? _issuerDropDownList;
        private static readonly object _issuerLockObject = new();

        static IssuerExtensions()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Issuers, IssuersDto>();
                cfg.CreateMap<Issuer, IssuerDto>();
                cfg.CreateMap<Country, CountryDto>();
            });

            _mapper = new Mapper(config);
        }

        public static void SetIssuersDropDownList(this IssuerModel issuerModel)
        {
            issuerModel.IssuersDropDownList = issuerModel.GetIssuersDropDownList();
        }

        private static List<SelectListItem> GetIssuersDropDownList(this IssuerModel issuerModel)
        {
            if (_issuerDropDownList != null)
            {
                return _issuerDropDownList;
            }

            lock (_issuerLockObject)
            {
                return _issuerDropDownList ??= issuerModel
                    .GetIssuers()
                    .GetIssuersSelectList();
            }
        }

        private static IssuersDto GetIssuers(this IssuerModel issuerModel)
        {
            IssuersDto issuers;

            if (File.Exists("issuers.json"))
            {
                var issuersText = File.ReadAllText("issuers.json");

                var existingIssuers = JsonSerializer.Deserialize<IssuersDto>(issuersText, new JsonSerializerOptions(JsonSerializerDefaults.Web));

                if (existingIssuers != null  && DateTime.Now - existingIssuers.DateTimestamp < _oneDay)
                {
                    return existingIssuers;
                }
            }

            var connector = Connector.CreateConnector(merchantId: issuerModel.MerchantId, merchantSubId: issuerModel.SubId, acquirerUrl: issuerModel.AcquirerUrl);

            issuers = connector.GetIssuerList().MapToDto();

            string newIssuersText = JsonSerializer.Serialize(issuers, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            File.WriteAllText("issuers.json", newIssuersText);

            return issuers;
        }

        private static List<SelectListItem> GetIssuersSelectList(this IssuersDto issuers)
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

        private static IssuersDto MapToDto(this Issuers issuers)
        {
            var issuersDto = _mapper.Map<IssuersDto>(issuers);

            issuersDto.DateTimestamp = DateTime.Now;

            return issuersDto;
        }
    }
}


