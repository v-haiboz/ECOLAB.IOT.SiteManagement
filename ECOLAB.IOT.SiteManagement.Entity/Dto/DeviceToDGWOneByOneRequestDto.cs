namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceToDGWOneByOneRequestDto
    {
        [Required]
        public string DeviceId { get; set; }

    }
}
