namespace ECOLAB.IOT.SiteManagement.Data.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GatewayRequestDto
    {
        public string SN { set; get; }

        public bool Validate()
        {
            if (string.IsNullOrEmpty(SN))
            {
                throw new Exception("SN doesn't Empty");
            }

            return true;
        }
    }
}
