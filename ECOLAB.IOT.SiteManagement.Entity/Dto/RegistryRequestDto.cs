namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    public class RegistryRequestDto
    { 
        public string? Model { get; set; }

        public string? Url { get; set; }

        public string? Checksum { get; set; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(Model) || string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(Checksum))
            {
                throw new Exception("Model Or Url Or Checksum doesn't Empty");
            }

            return true;
        }
    }
}
