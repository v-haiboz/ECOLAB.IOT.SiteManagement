namespace ECOLAB.IOT.SiteManagement.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface IDistributeJobProvider
    {
        public bool Travel();
    }

    public class DistributeJobProvider : IDistributeJobProvider
    {
        public bool Travel()
        {
            return true;
        }
    }
}
