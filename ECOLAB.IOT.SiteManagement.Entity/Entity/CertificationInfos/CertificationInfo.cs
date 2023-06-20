namespace ECOLAB.IOT.SiteManagement.Data.Entity.CertificationInfos
{
    using ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CertificationInfo
    {
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

        public string CertificationToken { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// FileCertifications
        /// </summary>
        public List<PermissionDto> Permissions { get; set; }
    }
}
