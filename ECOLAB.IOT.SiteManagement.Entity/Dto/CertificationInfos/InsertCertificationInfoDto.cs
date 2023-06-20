namespace ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class InsertCertificationInfoDto
    {
        /// <summary>
        /// CertificationName
        /// </summary>
        [StringLength(128)]
        [Required]
        public string CertificationName { get; set; }
        /// <summary>
        /// CertificationDesc
        /// </summary>
        public string CertificationDesc { get; set; }

        /// <summary>
        /// CertificationTokenExpirationUtcTime，if 0, it will never expire.
        /// </summary>
        public long CertificationTokenExpirationUtcTime { get; set; }
        /// <summary>
        /// FileCertifications
        /// </summary>
        public List<PermissionDto> Permissions { get; set; }
    }
}
