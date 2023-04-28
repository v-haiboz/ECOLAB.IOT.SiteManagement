
namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    public class SiteResponseDto
    {
        public string Id { get; set; }
        public List<RegistryResponseDto>? Registry { get; set; } = new List<RegistryResponseDto>();
    }
}
