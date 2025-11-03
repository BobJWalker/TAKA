using Autofac;
using Autofac.Extensions.DependencyInjection;
using NLog.Config;
using NLog.Web;
using Octopus.OpenFeature.Provider;
using OpenFeature;
using OpenFeature.Model;
using OpenFeature.Contrib.Providers.EnvVar;
using SumoLogic.Logging.NLog;
using System.Reflection;

namespace TAKA.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureOpenFeature().GetAwaiter().GetResult();            
            ConfigureDependencyInjection(builder);
            ConfigureLogging(builder);
            ConfigureServices(builder);

            builder.Host.UseNLog();

            var app = builder.Build();
            app.UseDeveloperExceptionPage();            
            app.UseStaticFiles();
            app.UseRouting();
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews();

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddWebOptimizer(minifyJavaScript: false, minifyCss: false);
            }
            else
            {
                builder.Services.AddWebOptimizer();
            }            
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            var sumoUrl = Environment.GetEnvironmentVariable("TAKA_SUMOLOGIC_URL");

            if (string.IsNullOrWhiteSpace(sumoUrl))
            {
                return;
            }

            if (sumoUrl == "blah")
            {
                return;
            }

            var logConfig = new LoggingConfiguration();
            var sumoTarget = new SumoLogicTarget();

            sumoTarget.Name = "SumoLogic";
            sumoTarget.Url = Environment.GetEnvironmentVariable("TAKA_SUMOLOGIC_URL");
            sumoTarget.SourceName = $"TAKA.{Environment.GetEnvironmentVariable("TAKA_ENVIRONMENT")}";
            sumoTarget.SourceCategory = "TAKA";
            sumoTarget.ConnectionTimeout = 30000;
            sumoTarget.UseConsoleLog = true;
            sumoTarget.Layout = "${LEVEL}, ${message}${exception:format=tostring}${newline}";

            logConfig.AddTarget(sumoTarget);
            logConfig.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, sumoTarget));

            builder.Logging.AddNLogWeb(logConfig);
        }

        private static void ConfigureDependencyInjection(WebApplicationBuilder builder)
        {
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(autofacBuilder =>
                {
                    autofacBuilder.RegisterInstance(GetFeatureClient())
                        .As<IFeatureClient>()
                        .SingleInstance();

                    var assembly = Assembly.GetExecutingAssembly();                    

                    autofacBuilder.RegisterType<LoggerFactory>()
                        .As<ILoggerFactory>()
                        .InstancePerLifetimeScope();

                    autofacBuilder.RegisterGeneric(typeof(Logger<>))
                        .As(typeof(ILogger<>))
                        .InstancePerLifetimeScope();
                });
        }

        private static async Task ConfigureOpenFeature()
        {
            var clientIdentifier = Environment.GetEnvironmentVariable("TAKA_OPEN_FEATURE_CLIENT_ID");

            if (string.IsNullOrWhiteSpace(clientIdentifier) == true)
            {
                await OpenFeature.Api.Instance.SetProviderAsync(new EnvVarProvider("FeatureToggle_"));
            }
            else
            {
                await OpenFeature.Api.Instance.SetProviderAsync(new OctopusFeatureProvider(new OctopusFeatureConfiguration(clientIdentifier)));
            }
        }

        private static IFeatureClient GetFeatureClient()
        {
            var client = OpenFeature.Api.Instance.GetClient();

            client.SetContext(EvaluationContext.Builder().Build());

            return client;
        }
    }
}