using AspNetCoreRateLimit;
using ECOLAB.IOT.SiteManagement.Data.Entity.CertificationInfos;
using ECOLAB.IOT.SiteManagement.Filters;
using ECOLAB.IOT.SiteManagement.Middlewares;
using ECOLAB.IOT.SiteManagement.Quartz;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ECOLAB.IOT.SiteManagement
{
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

            #region 读取Configuration
            var azureAdClientId = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AAD_ClientId")) ?
                builder.Configuration["AAD:ClientId"] : Environment.GetEnvironmentVariable("AAD_ClientId");
            var swaggerClientId = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Swagger_ClientId")) ?
                builder.Configuration["Swagger:ClientId"] : Environment.GetEnvironmentVariable("Swagger_ClientId");
            var instance = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AAD_Instance")) ?
                builder.Configuration["AAD:Instance"] : Environment.GetEnvironmentVariable("AAD_Instance");
            var tenantId = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AAD_TenantId")) ?
                builder.Configuration["AAD:TenantId"] : Environment.GetEnvironmentVariable("AAD_TenantId");
            var domain = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AAD_Domain")) ?
                builder.Configuration["AAD:Domain"] : Environment.GetEnvironmentVariable("AAD_Domain");
            #endregion

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
            .AddAutoMapper(typeof(CertificationInfoProfile).Assembly, typeof(CertificationDtoProfile).Assembly)
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
                c.AddSecurityDefinition("AAD", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    In = ParameterLocation.Header,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(instance + "/" + tenantId + "/oauth2/authorize")
                        }
                    }
                });
                //c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "AAD"
                            },
                            Scheme = "AAD",
                            Name = "AAD",
                            In = ParameterLocation.Header
                        },
                        new List < string > ()
                    }
                });
            });

            #region 跨域设置
            var devCorsPolicy = "devCorsPolicy";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(devCorsPolicy, builder => {
                    //builder.WithOrigins("http://localhost:800").AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    //builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
                    //builder.SetIsOriginAllowed(origin => true);
                });
            });
            #endregion

            #region AAD认证设置
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                options.Instance = instance;
                options.Domain = domain;
                options.TenantId = tenantId;
                options.ClientId = azureAdClientId;
            });

            builder.Services.AddAuthentication(AzureADDefaults.JwtBearerAuthenticationScheme)
                .AddAzureADBearer(options =>
                {
                    options.Instance = instance;
                    options.TenantId = tenantId;
                    options.ClientId = azureAdClientId;
                    options.Domain = domain;
                });
            #endregion
            #region  AspNetCoreRateLimit设置

            builder.Services.AddOptions();

            builder.Services.AddMemoryCache();

            builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("AspNetCoreRateLimit"));

            builder.Services.AddInMemoryRateLimiting();

            builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            #endregion
            builder.Services.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
            var app = builder.Build();

           

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI(swaggerUI =>
                {
                    swaggerUI.OAuthClientId(swaggerClientId);
                    swaggerUI.OAuthRealm(azureAdClientId);
                    swaggerUI.OAuthScopeSeparator(" ");
                    swaggerUI.OAuthAdditionalQueryStringParams(new Dictionary<string, string>() { { "resource", azureAdClientId } });

                    //swaggerUI.OAuthClientId(swaggerClientId);
                    //swaggerUI.OAuthRealm(azureAdClientId);
                    //swaggerUI.OAuthScopeSeparator(" ");
                    //swaggerUI.OAuthAdditionalQueryStringParams(new Dictionary<string, string>() { { "resource", azureAdClientId } });
                });
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
            app.UseCors(devCorsPolicy);
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}