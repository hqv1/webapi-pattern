using AspNetCoreRateLimit;
using FluentValidation.AspNetCore;
using Hqv.CSharp.Common;
using Hqv.CSharp.Common.Extensions;
using Hqv.CSharp.Common.Interfaces;
using Hqv.CSharp.Common.Web.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;
using NLog.Web;
using Swashbuckle.AspNetCore.Swagger;
using WebApiPattern.Asp.CoreCore.Helpers;
using WebApiPattern.Asp.Shared;
using WebApiPattern.Asp.Shared.Ordering;
using WebApiPattern.Asp.Shared.Validators;
using WebApiPattern.Data.Sqlite;
using WebApiPattern.Logic;

namespace WebApiPattern.Asp.CoreCore
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        /// <summary>
        /// Use this method to set up your IOC container
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                // Add MVC
                .AddMvc(action =>
                {
                    // Return 406 if we cannot generate the response in the requested format 
                    // Can output in XML
                    // Can accept XML 
                    action.ReturnHttpNotAcceptable = true;
                    action.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                    action.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
                })
                // Shaping the return result will make the properties uppercase. This JSON option
                //will make it lower case. Reason is the returned object is an ExpandoObject which uses
                //a dictionary in the background. The default ContractResolver does not correctly work with
                //a dictionary.
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                })
                // Use Fluent Validation for validation instead of the built-in validation. I like to 
                //see validation in one place, rather than in attributes and in code. 
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<ProductForCreationModelValidator>())
                ;

            // Create a IUrlHelper. Helps create URLs for the resources we'll return to the client
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });


            services.AddScoped<IPagedListHelper, PagedListHelper>(); // Helps create a paged list

            services.AddSingleton<Hqv.CSharp.Common.Interfaces.ILogger, Logger>(); // Use Shared Logging
            services.AddSingleton<IMapper, Mapper>(); // Use Mapper
         
            services.AddScoped(provider => new DbRepository.Setting(Configuration["db-connection-string"]));
            services.AddScoped<IDbRepository, DbRepository>();
            services.AddScoped(provider => new ProductRepository.Setting(Configuration["db-connection-string"]));
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped(provider => new SellerRepository.Setting(Configuration["db-connection-string"]));
            services.AddScoped<ISellerRepository, SellerRepository>();           

            services.AddTransient<IPropertyMappingService, PropertyMappingService>(); // Ordering
            services.AddTransient<ITypeHelper, TypeHelper>(); // Shaping

            services.AddResponseCaching(); // Add response caching. Tells clients whether and how to cache results.

            services.AddMemoryCache(); // Memory cache used to store rate limiting information

            // Rate limiting
            services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = new System.Collections.Generic.List<RateLimitRule>()
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Limit = 1000,
                        Period = "5m"
                    }
                };
            });
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Web Api Pattern Core", Version = "v1" });
            });
        }

        /// <summary>
        /// Use this method to configure the HTTP request pipeline.
        /// The ordering of the middleware is important.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog(); // Add NLog to the list of loggers
            app.AddNLogWeb(); // Allows Nlog to get ASP.NET related information (like query string, etc)

            app.UseResponseCaching(); // Use response caching.

            // In development environment ...           
            if (env.IsDevelopment())
            {
                loggerFactory.AddConsole(); // Log to console
                app.UseDeveloperExceptionPage(); // Use developer's exception page.
            }
            else
            {               
                // Exception handling pipeline
                // Unable to get the body at this point as ASP.NET have already parse the body (it can only be read once)
                //unless you set EnableRewind(). But you can still only read the body before the controller reads it.
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {                      
                        // On an unhandled exception, send a 500 response with a generic message 
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later");
                    });
                });
            }

            app.UseIpRateLimiting(); // Use rate limiting by IP address. There are other options aside IP.
            app.UseMvc(); // Use MVC

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

    }
}
