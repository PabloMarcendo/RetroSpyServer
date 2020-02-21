using System.ServiceModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SoapCore;
using Interfaces;
using Models;
using Services;

namespace WebServices
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddSoapCore();
            services.TryAddSingleton<IAuthService, AuthService>();
            services.TryAddSingleton<ICompService, CompService>();
            services.TryAddSingleton<ID2GService, D2GService>();
            services.TryAddSingleton<IMotdService, MotdService>();
            services.TryAddSingleton<ISakeService, SakeService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.UseSoapEndpoint<IAuthService>("/AuthService/AuthService.asmx", new BasicHttpsBinding(), SoapSerializer.XmlSerializer);
                endpoints.UseSoapEndpoint<ICompService>("/", new BasicHttpBinding(), SoapSerializer.XmlSerializer);
                endpoints.UseSoapEndpoint<ID2GService>("/", new BasicHttpBinding(), SoapSerializer.XmlSerializer);
                endpoints.UseSoapEndpoint<IMotdService>("/motd/motd.asp", new BasicHttpBinding(), SoapSerializer.XmlSerializer);
                endpoints.UseSoapEndpoint<ISakeService>("/SakeStorageServer/StorageServer.asmx", new BasicHttpBinding(), SoapSerializer.XmlSerializer);
            });
        }
    }
}
