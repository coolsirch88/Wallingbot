using IRCBot.Common;
using IRCBot.Lib;
using IRCBotWeb.Config;
using IRCBotWeb.Hubs;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace IRCBotWeb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.
            Configuration = new Configuration()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(Configuration);
            services.Configure<EnvConfiguration>(Configuration);
            services.AddSingleton<IIRCBot, IRC>();
            services.Configure<MvcOptions>(options =>
                                     options
                                     .OutputFormatters
                                     .RemoveAll(formatter => formatter.Instance is XmlDataContractSerializerOutputFormatter)
                                           );
            services.AddSignalR(options =>
                {
                    options.Hubs.EnableDetailedErrors = true;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseErrorPage(ErrorPageOptions.ShowAll);
            app.UseStaticFiles();
            app.UseSignalR();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
