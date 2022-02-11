using System;
using System.IO;
using System.Reflection;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
using MarketData.Gateway.HealthChecks;
using MarketData.Gateway.Options;
using MarketData.Gateway.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTracing;
using Prometheus;

namespace MarketData.Gateway
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
            var authorizationOptions = new AuthorizationOptions();
            Configuration.Bind(authorizationOptions);

            if (authorizationOptions.EnableAzureAd)
            {
                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAdB2C"));
            }

            services.Configure<ValidationClientOptions>(Configuration);
            services.AddSingleton<IValidationClient, ValidationClient>();
            services.AddSingleton<IGatewayService, GatewayService>();

            // Register Jaeger
            services.AddSingleton<ITracer>(serviceProvider =>
            {
                var serviceName = serviceProvider.GetRequiredService<IWebHostEnvironment>().ApplicationName;
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                ISampler sampler = new ConstSampler(true);

                var host = Environment.GetEnvironmentVariable("JAEGER_AGENT_HOST") ?? "localhost";

                var remoteReporter = new RemoteReporter.Builder()
                    .WithLoggerFactory(loggerFactory)
                    .WithSender(new UdpSender(host, 6831, 0))
                    .Build();

                return new Tracer.Builder(serviceName)
                    .WithReporter(remoteReporter)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(sampler)
                    .Build();
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Market Data Gateway API",
                        Version = "v1",
                        Description = "Market Data Gateway API",
                        TermsOfService = new Uri("https://example.com/terms"),
                        Contact = new OpenApiContact
                        {
                            Name = "Engineering Team",
                            Email = "engineering@socgen.com",
                        },
                    }
                );
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services
                .AddHealthChecks()
                .AddCheck<ValidationClientHealthCheck>(
                    "Health check for access to validation service.",
                    HealthStatus.Degraded,
                    new[] { "health" })
                .ForwardToPrometheus();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market Data Gateway API v1"));

            // This is the responsibility of APIM and should not be done in the API itself
            // app.UseHttpsRedirection();

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                // Only map the "health" tag
                // TODO expand to live and ready when this is deployed to Kubernetes
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    },
                    Predicate = registration => registration.Tags.Contains("health"),
                });
                endpoints.MapMetrics();
            });
        }
    }
}