using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TTPService.Authorization;
using TTPService.Configuration;

namespace TTPService
{
    public static class SwaggerExtensions
    {
        private const string ApiName = "Request";

        public static void AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                // resolve the IApiVersionDescriptionProvider service
                // note: that we have to build a temporary service provider here because one has not been created yet
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                var authorizationOptions = configuration.GetSectionValue<AuthorizationOptions>();

                // add a swagger document for each discovered API version
                // note: you might choose to skip or document deprecated API versions differently
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                }

                options.UseAllOfToExtendReferenceSchemas();
                options.UseAllOfForInheritance();
                options.UseOneOfForPolymorphism();

                // add a custom operation filter which sets default values
                // options.OperationFilter<SwaggerDefaultValues>();
                options.AddSecurity(authorizationOptions.AuthenticationTokenEndPoint);

                // integrate xml comments
                // options.IncludeXmlComments(XmlCommentsFilePath);
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
        }

        public static void UseSwagger(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    options.RoutePrefix = string.Empty;

                    // build a swagger endpoint for each discovered API version
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo()
            {
                Title = $"{ApiName} API {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = $"{ApiName} API",
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private static void AddSecurity(this SwaggerGenOptions options, string authenticationTokenEndPoint)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\". \n\n" +
                              $"Copy token from: <a href=\"{authenticationTokenEndPoint}\" target=\"_blank\">Get Bearer Token</a>. No need to prefix token w/ **_Bearer_** inside input box.",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    new string[] { }
                },
            });
        }
    }
}
