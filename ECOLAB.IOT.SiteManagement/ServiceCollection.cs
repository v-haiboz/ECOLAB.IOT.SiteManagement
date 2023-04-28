using ECOLAB.IOT.SiteManagement.Provider;
using ECOLAB.IOT.SiteManagement.Quartz;
using ECOLAB.IOT.SiteManagement.Repository;
using ECOLAB.IOT.SiteManagement.Service;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using static ECOLAB.IOT.SiteManagement.Provider.IDistributeJobProvider;

namespace ECOLAB.IOT.SiteManagement
{

    public static class ServiceCollection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            if (services == null)
                return null;
            //services.AddQuartzUI();
            // var optionsBuilder = new DbContextOptionsBuilder<QuarzEFContext>();
            //optionsBuilder.UseDm("Server=tcp:cn-ins-elinkcicd-sqlserver-001-d.database.chinacloudapi.cn,1433;Initial Catalog=CN-INS-ELINKCICD-SQLDB-001-D;Persist Security Info=False;User ID=ELINKCICDSQLAdmin;Password=XD#$s29os21P", b => b.MaxBatchSize(1));


            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IGetwayService, GetwayService>();
            services.AddScoped<IGatewayDeviceService, GatewayDeviceService>();
            services.AddScoped<ISiteDeviceHealthService, SiteDeviceHealthService>();
            services.AddScoped<IDistributeJobService, DistributeJobService>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            if (services == null)
                return null;

            services.AddScoped<ISiteRepository, SiteRepository>();
            services.AddScoped<IGetwayRepository, GetwayRepository>();
            services.AddScoped<IGatewayDeviceRepository, GatewayDeviceRepository>();
            services.AddScoped<ISiteDeviceHealthRepository, SiteDeviceHealthRepository>();
            services.AddScoped<IGatewayAllowListTaskRepository, GatewayAllowListTaskRepository>();
            return services;
        }

        public static IServiceCollection AddJobSchedule(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddSingleton<DistributeJob>();
            services.AddSingleton(
                 new JobSchedule(jobType: typeof(DistributeJob), cronExpression: "01 */2 * * * ?")
           );
            services.AddSingleton<QuartzHostedService>();
            return services;
        }

        public static IServiceCollection AddProviders(this IServiceCollection services)
        {
            if (services == null)
                return null;

            services.AddScoped<IStorageProvider, StorageProvider>();
            services.AddScoped<IDistributeJobProvider, DistributeJobProvider>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            return services;
        }
    }
}
