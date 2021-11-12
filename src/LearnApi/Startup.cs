using Autofac;
using Learning.Api.Swagger;
using Learning.Common.Settings;
using Learning.Repositories;
using Learning.Services;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LearnApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;

            GlobalContext.Properties["service"] = "learningapi";
            GlobalContext.Properties["environment"] = environment.EnvironmentName;
            _log = LogManager.GetLogger(typeof(Startup));
        }

        private readonly ILog _log;

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;//可选配置，接口http响应头输出api版本信息
                options.AssumeDefaultVersionWhenUnspecified = true;//是否提供api版本支持
                options.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
            _ = services.AddControllers();

            _ = services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            if (Environment.IsDevelopment())
            {
                _ = services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
                _ = services.AddSwaggerGen(options =>
                {
                    foreach (var file in System.IO.Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                    {
                        options.IncludeXmlComments(file);
                    }
                });
            }
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            _ = builder.RegisterModule<ServicesModule>();
            _ = builder.RegisterModule<RepositoriesModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                _ = app.UseDeveloperExceptionPage()
                   .UseSwagger()
                   .UseSwaggerUI(options =>
                   {
                       foreach (var description in provider.ApiVersionDescriptions)
                       {
                           options.SwaggerEndpoint(
                               $"{description.GroupName}/swagger.json",
                               description.GroupName.ToUpperInvariant());
                       }
                   });
            }

            _ = app.UseHttpsRedirection()
                   .UseExceptionHandler(error => error.Run(async context =>
                   {
                       var error = context.Features.Get<IExceptionHandlerFeature>();
                       if (error != null)
                       {
                           var ex = error.Error;
                           _log.Error($" path {context.Request.Path} Internal Server Error", ex);
                       }
                       context.Response.StatusCode = 500;
                       context.Response.ContentType = "application/json";
                       await context.Response.WriteAsync("{\"error\":\"Internal Server Error\"}").ConfigureAwait(false);
                   }))
                   .UseRouting()
                   .UseCors()
                   .UseAuthorization()
                   .UseEndpoints(endpoints => endpoints.MapControllers());
                   
        }
    }
}
