namespace iDealSampleCore.Dto
{
    public class IssuersDto
    {
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<CountryDto> Countries { get; set; } = new();

        public DateTime DateTimestamp { get; set; }
    }
}
