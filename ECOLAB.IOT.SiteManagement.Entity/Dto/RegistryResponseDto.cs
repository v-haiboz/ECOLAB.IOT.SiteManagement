using System.Text.Json.Serialization;

namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    public class RegistryResponseDto
    {
        [JsonPropertyName("_version")]
        public string? Version { get; set; }
        public string? Model { get; set; }

        public string? Url { get; set; }

        public string? Checksum { get; set; }
    }
}
