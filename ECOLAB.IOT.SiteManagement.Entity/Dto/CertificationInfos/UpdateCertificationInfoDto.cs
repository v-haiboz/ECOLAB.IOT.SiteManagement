namespace ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos
{
    using System.Collections.Generic;

    public class UpdateCertificationInfoDto
    {
        /// <summary>
        /// 自定义凭证描述
        /// </summary>
        public string CertificationDesc { get; set; }

        /// <summary>
        /// 凭证过期时间，如果为0则永不过期
        /// </summary>
        public long CertificationTokenExpirationUtcTime { get; set; }
        /// <summary>
        /// FileCertifications
        /// </summary>
        public List<PermissionDto> Permission { get; set; }
    }
}
