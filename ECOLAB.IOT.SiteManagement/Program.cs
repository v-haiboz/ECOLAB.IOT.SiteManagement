namespace ECOLAB.IOT.SiteManagement
{
    using ECOLAB.IOT.SiteManagement.Filters;
    using ECOLAB.IOT.SiteManagement.Middlewares;
    using ECOLAB.IOT.SiteManagement.Quartz;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(2);
                serverOptions.AllowSynchronousIO = true;
            });
            
            


            // Azure log
            builder.Host.ConfigureLogging(logging => logging.AddAzureWebAppDiagnostics());

            // Add services to the container.
            builder.Services.AddServices()
                .AddSwaggerGen(c => {
                    var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                    var xmlPath = Path.Combine(basePath, @"APIAssembly.xml");
                    c.IncludeXmlComments(xmlPath);
                })
            .AddRepositories()
            .AddProviders()
            .AddJobSchedule();
            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<UniformResponseFilter>();
                //options.Filters.Add(new ServiceFilterAttribute(typeof(GlobalExceptionFilterAttribute)));
            });
            builder.Services.AddScoped<GlobalExceptionFilterAttribute>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                //c.OperationFilter<SwaggerParametersAttributeHandler>();
            });

            //      builder.Services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
            //.AddAzureADBearer(options =>
            //{
            //    options.Instance = "";
            //    options.TenantId = "";
            //    options.ClientId = "";
            //    options.Domain = "";
            //});

            builder.Services.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI();
            }

            var quartz = app.Services.GetRequiredService<QuartzHostedService>();
            var cancellationToken = new CancellationToken();
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                quartz.StartAsync(cancellationToken).Wait(); //app启动完成执行
            });
            app.Lifetime.ApplicationStopped.Register(() =>
            {
                quartz.StopAsync(cancellationToken).Wait();  //app停止完成执行
            });

            app.UseExceptionMiddleware();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}