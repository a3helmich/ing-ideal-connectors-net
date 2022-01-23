namespace iDealSampleCore.Dto
{
    public class CountryDto
    {
        public string CountryNames { get; set; } = string.Empty;

        // ReSharper disable once CollectionNeverUpdated.Global
        public List<IssuerDto> Issuers { get; set; } = new();
    }
}
