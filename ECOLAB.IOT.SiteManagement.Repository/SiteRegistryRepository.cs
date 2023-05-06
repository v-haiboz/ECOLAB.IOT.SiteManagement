namespace ECOLAB.IOT.SiteManagement.Repository
{
    using Dapper;
    using ECOLAB.IOT.SiteManagement.Data.Entity;
    using Microsoft.Extensions.Configuration;
    using System.Linq;

    public interface ISiteRegistryRepository
    {

        public List<SiteRegistry>? GetSiteRegistryListRegistiesBySiteNo(string siteNo);

    }

    public class SiteRegistryRepository : Repository, ISiteRegistryRepository
    {

        public SiteRegistryRepository(IConfiguration config) : base(config)
        {
        }
        public List<SiteRegistry>? GetSiteRegistryListRegistiesBySiteNo(string siteNo)
        {

            return Execute((conn) =>
            {
                string query = $@"select b.* from 
                        [dbo].[Site] as a
                        inner join 
                        [dbo].[SiteRegistry] as b
                        on a.Id=b.SiteId where  a.SiteNo='{siteNo}'";
                var rows = conn.Query<SiteRegistry>(query);

                return rows?.Distinct()?.ToList();
            });
        }
    }
}
