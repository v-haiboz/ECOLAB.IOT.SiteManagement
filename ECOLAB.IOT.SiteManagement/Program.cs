using ECOLAB.IOT.SiteManagement.Quartz;

namespace ECOLAB.IOT.SiteManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                //c.OperationFilter<SwaggerParametersAttributeHandler>();
            });
          
           
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

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}