using Built.Grpcc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.GrpcHttpGateway;
using Ocelot.Middleware;

namespace Examples.OcelotGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOcelot(Configuration).AddGrpcHttpGateway(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //, ILoggerFactory loggerFactory
            //loggerFactory.AddConsole();
            app.Run(async (context) =>
            {
                //CodeBuild.Build("fasdf", "");

                var sd = ServiceLocator.GetService<Built.Grpcc.ServiceDescriptor>();
                foreach (var srv in sd.Descriptor)
                {
                    await context.Response.WriteAsync(srv.Key + "\r\n");
                    foreach (var fuc in srv.Value)
                    {
                        await context.Response.WriteAsync("    " + fuc.Key + "\r\n");
                    }
                }
            });
            //ServiceLocator.Instance = app.ApplicationServices;

            app.UseOcelot(config =>
            {
                config.AddGrpcHttpGateway();
            }).Wait();
        }
    }
}