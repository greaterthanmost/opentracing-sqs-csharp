using System;
using EventBus.Sqs.Configuration;
using EventBus.Sqs.Examples.AspNetCore.Events;
using EventBus.Sqs.Tracing.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing.Util;

namespace EventBus.Sqs.Examples.AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            Environment.SetEnvironmentVariable("JAEGER_SERVICE_NAME", Configuration["JAEGER_SERVICE_NAME"]);
            Environment.SetEnvironmentVariable("JAEGER_AGENT_HOST", Configuration["JAEGER_AGENT_HOST"]);
            Environment.SetEnvironmentVariable("JAEGER_AGENT_PORT", Configuration["JAEGER_AGENT_PORT"]);
            Environment.SetEnvironmentVariable("JAEGER_SAMPLER_TYPE", Configuration["JAEGER_SAMPLER_TYPE"]);

            services.AddOpenTracing();

            services.AddSingleton(serviceProvider =>
            {
                var loggerFactory = new LoggerFactory();

                var config = Jaeger.Configuration.FromEnv(loggerFactory);
                var tracer = config.GetTracer();

                if (!GlobalTracer.IsRegistered())
                    GlobalTracer.Register(tracer);

                return tracer;
            });

            services.AddEventBusSQS(Configuration)
                    .AddOpenTracing();

            services.AddHealthChecks()
                    .AddSqsCheck<WeatherInEvent>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
