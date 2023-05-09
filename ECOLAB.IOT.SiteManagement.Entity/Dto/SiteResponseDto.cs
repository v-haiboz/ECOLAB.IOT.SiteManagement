
using System.Text.Json.Serialization;

namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    public class SiteResponseDto
    {
        [JsonPropertyName("site_id")]
        public string Id { get; set; }
        public List<RegistryResponseDto>? Registry { get; set; } = new List<RegistryResponseDto>();
    }
}
