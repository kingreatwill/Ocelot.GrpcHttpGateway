using Built.Grpcc;
using Built.Grpcc.SwaggerGen;
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
            services.AddMvc();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "My API", Version = "v1" });
            });
            services.AddOcelot(Configuration).AddGrpcHttpGateway(Configuration);
        }

        private static void HandleHome(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
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
        }

        private static void HandleSwagger(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var handlers = ServiceLocator.GetService<Built.Grpcc.ServiceDescriptor>();
                var builder = new SwaggerDefinitionBuilder(new SwaggerOptions("t", "d", "")
                {
                }, context, handlers);
                var bytes = builder.BuildSwaggerJson();
                context.Response.Headers["Content-Type"] = new[] { "application/json" };
                context.Response.StatusCode = 200;
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.Map("/srv", HandleHome);
            app.Map("/swagger/v1/swagger.json", HandleSwagger);
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
            //ServiceLocator.Instance = app.ApplicationServices;
            app.UseOcelot(config =>
            {
                config.AddGrpcHttpGateway();
            }).Wait();
        }
    }
}