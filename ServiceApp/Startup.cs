using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;

namespace ServiceApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddHostedService<TimedHostedService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes => {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Add any Autofac modules or registrations.
            // This is called AFTER ConfigureServices so things you
            // register here OVERRIDE things registered in ConfigureServices.
            //
            // You must have the call to AddAutofac in the Program.Main
            // method or this won't be called.
            builder.RegisterInstance(Log.Logger).As<ILogger>();

            builder.RegisterType<TimedHostedService>().AsImplementedInterfaces().SingleInstance();

            var instance = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                EndpointResolverFactory = (Func<IEnumerable<AmqpTcpEndpoint>, IEndpointResolver>) (endpoints => (IEndpointResolver) new DefaultEndpointResolver(new [] {new AmqpTcpEndpoint("localhost", -1)})),
                VirtualHost = "/",
                UserName = "guest",
                Password = "guest",
                RequestedHeartbeat = 60
            };
            builder.RegisterInstance<ConnectionFactory>(instance).As<IConnectionFactory>().SingleInstance();
            builder.Register<IConnection>((Func<IComponentContext, IConnection>) (cc => cc.Resolve<IConnectionFactory>().CreateConnection())).As<IConnection>().SingleInstance();
            builder.Register<IModel>((Func<IComponentContext, IModel>) (cc => cc.Resolve<IConnection>().CreateModel())).As<IModel>().InstancePerLifetimeScope();
        }
    }
}
