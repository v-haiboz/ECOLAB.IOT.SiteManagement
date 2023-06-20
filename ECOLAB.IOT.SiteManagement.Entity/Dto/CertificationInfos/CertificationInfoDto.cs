using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECOLAB.IOT.SiteManagement.Data.Dto.CertificationInfos
{
    public class CertificationInfoDto
    {
        /// <summary>
        /// 凭证名称
        /// </summary>
        public string CertificationName { get; set; }
        /// <summary>
        /// 自定义凭证描述
        /// </summary>
        public string CertificationDesc { get; set; }
        /// <summary>
        /// 凭证Token
        /// </summary>
        public string CertificationToken { get; set; }
        /// <summary>
        /// 凭证过期时间，如果为0则永不过期
        /// </summary>
        public long CertificationTokenExpirationUtcTime { get; set; }
        /// <summary>
        /// FileCertifications
        /// </summary>
        public List<PermissionDto>  Permissions { get; set; }
    }
}
