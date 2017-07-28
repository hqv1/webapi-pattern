using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using FluentValidation.WebApi;
using Hqv.CSharp.Common.Ordering;
using Hqv.CSharp.Common.Utilities;
using Microsoft.Owin.Logging;
using Newtonsoft.Json.Serialization;
using NLog.Owin.Logging;
using Owin;
using Swashbuckle.Application;
using WebApiPattern.Asp.Shared;
using WebApiPattern.Asp.Shared.Ordering;
using WebApiPattern.Data.Sqlite;
using WebApiPattern.Domain;
using WebApiThrottle;
using IMapper = Hqv.CSharp.Common.Map.IMapper;

namespace WebApiPattern.Asp.Framework
{
    /// <summary>
    /// 
    /// Autofac.WebApi2.Owin
    /// Install Microsoft.AspNet.WebApi.Core
    /// Install Microsoft.AspNet.WebApi.OwinSelfHost
    /// Install Microsoft.Owin.Host.SystemWeb
    /// Install Marvin.JsonPatch
    /// Install NLog.Web 
    /// Install WebApiThrottle
    /// </summary>
    public class Startup
    {
        public static void Configuration(IAppBuilder app)
        {
            var dbConnectionString = WebConfigurationManager.AppSettings["DbConnectionString"];

            app.UseNLog();
            var logger = app.GetLoggerFactory().Create("Startup");

            var config = new HttpConfiguration();

            // Use camel case JSON
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            // Return Not Acceptable for unknown content types 
            var defaultNegotiator = new DefaultContentNegotiator(excludeMatchOnTypeOnly: true);
            config.Services.Replace(typeof(IContentNegotiator), defaultNegotiator);

            // Fluent Validation 
            FluentValidationModelValidatorProvider.Configure(config);

            // IOC using Autofac
            var builder = new ContainerBuilder();

            builder.RegisterType<DbRepository>().As<IDbRepository>().InstancePerLifetimeScope();
            builder.RegisterInstance(new DbRepository.Setting(dbConnectionString)).SingleInstance();
            builder.RegisterType<ProductRepository>().As<IProductRepository>().InstancePerLifetimeScope();
            builder.RegisterInstance(new ProductRepository.Setting(dbConnectionString)).SingleInstance();
            builder.RegisterType<SellerRepository>().As<ISellerRepository>().InstancePerLifetimeScope();
            builder.RegisterInstance(new SellerRepository.Setting(dbConnectionString)).SingleInstance();

            builder.RegisterType<Mapper>().As<IMapper>().InstancePerLifetimeScope();
            builder.RegisterType<TypeHelper>().As<ITypeHelper>().InstancePerLifetimeScope();
            builder.RegisterType<PropertyMappingService>().As<IPropertyMappingService>().InstancePerLifetimeScope();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            // IOC using Autofac

            // Caching
            var objCacheCow = new CacheCow.Server.CachingHandler(config);
            config.MessageHandlers.Add(objCacheCow);

            // Rate Limiting. Does not return response headers like X-Rate-Limit.
            config.MessageHandlers.Add(new ThrottlingHandler
            {
                Policy = new ThrottlePolicy(perSecond: 50, perMinute: 100, perHour: 200, perDay: 1500, perWeek: 3000)
                {
                    IpThrottling = true
                },
                Repository = new CacheRepository()
            });

            config.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger(logger));
            config.MapHttpAttributeRoutes(); // Go through our controllers and get the routes
            config.EnableSwagger(c => c.SingleApiVersion("v1", "Web Api Pattern - Framework"))
                .EnableSwaggerUi();
            app.UseWebApi(config);
        }
    }

    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            
        }
    }
}