﻿using Microsoft.AspNetCore.Mvc;
using Halcyon.Web.HAL.Json;
using Threax.AspNetCore.Halcyon.Ext;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Halcyon.HAL;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Mvc.Formatters;
using NJsonSchema.Generation;
using NJsonSchema;
using Threax.AspNetCore.Halcyon.Ext.ValueProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions class for IServiceCollection for the Halcyon convention.
    /// </summary>
    public static class HalcyonConvention
    {
        /// <summary>
        /// Add the conventional Halcyon services. This will set everything up for attributes to work correctly.
        /// You must use this function when calling UseConventionalHalcyon on MvcOptions or the services will
        /// not be setup correctly.
        /// </summary>
        /// <param name="services">The service collection to modify.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static IServiceCollection AddConventionalHalcyon(this IServiceCollection services, HalcyonConventionOptions options)
        {
            services.TryAddSingleton<ILabelCacheFactory, LabelCacheFactory>();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.TryAddSingleton<HalcyonConventionOptions>(options);
            services.TryAddScoped<IHALConverter, CustomHalAttributeConverter>();
            services.TryAddScoped<HalModelResultFilterAttribute, HalModelResultFilterAttribute>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddScoped<IEndpointDocBuilder, EndpointDocBuilder>();
            services.TryAddScoped<IValidSchemaTypeManager, ValidSchemaTypeManager>();
            services.TryAddScoped<IValueProviderResolver>(s =>
            {
                return new ValueProviderResolver(s);
            });
            services.TryAddScoped<ISchemaCustomizerResolver>(s =>
            {
                return new SchemaCustomizerResolver(s);
            });
            services.TryAddScoped<ISchemaBuilder>(s =>
            {
                return new SchemaBuilder(new EndpointDocJsonSchemaGenerator(options.JsonSchemaGeneratorSettings, s.GetRequiredService<IValueProviderResolver>(), s.GetRequiredService<ISchemaCustomizerResolver>(), s.GetRequiredService<IAutoTitleGenerator>()), s.GetRequiredService<IValidSchemaTypeManager>());
            });
            services.TryAddSingleton<IAutoTitleGenerator, AutoTitleGenerator>();

            return services;
        }

        /// <summary>
        /// A default setup for the json serializer settings, reccomended to use unless you want to customize.
        /// By default it will use the StringEnumConverter and the CamelCasePropertyNamesContractResolver.
        /// </summary>
        public static JsonSerializerSettings DefaultJsonSerializerSettings
        {
            get
            {
                var serializerSettings = JsonSerializerSettingsProvider.CreateSerializerSettings();
                return serializerSettings.SetToHalcyonDefault();
            }
        }

        /// <summary>
        /// A default setup for the json serializer settings, reccomended to use unless you want to customize.
        /// By default it will use the StringEnumConverter and the CamelCasePropertyNamesContractResolver.
        /// </summary>
        public static JsonSchemaGeneratorSettings DefaultJsonSchemaGeneratorSettings
        {
            get
            {
                return new JsonSchemaGeneratorSettings()
                {
                    DefaultEnumHandling = EnumHandling.String,
                    DefaultPropertyNameHandling = PropertyNameHandling.CamelCase,
                    FlattenInheritanceHierarchy = true,
                };
            }
        }

        /// <summary>
        /// Add Halcyon with some custom formatters that handle marked up models better.
        /// </summary>
        /// <param name="mvcOptions">The MVC options to extend.</param>
        /// <param name="options">The hal options, send the same value you sent to AddConventionalHalcyon.</param>
        /// <returns></returns>
        public static MvcOptions UseConventionalHalcyon(this MvcOptions mvcOptions, HalcyonConventionOptions options)
        {
            mvcOptions.RespectBrowserAcceptHeader = true;
            var mediaTypes = new string[] { ProducesHalAttribute.MediaType };
            var outputFormatter = new JsonHalOutputFormatter(options.JsonSerializerSettings, mediaTypes);
            mvcOptions.OutputFormatters.Add(outputFormatter);
            if (options.MakeAllControllersHalcyon)
            {
                mvcOptions.Filters.Add(new ProducesHalAttribute());
                mvcOptions.Filters.AddService(typeof(HalModelResultFilterAttribute));
            }
            mvcOptions.Conventions.Add(new ApiExplorerVisibilityEnabledConvention());

            return mvcOptions;
        }

        /// <summary>
        /// If using the default serializer settings you can easily apply them to mvc by calling this function
        /// from inside AddJsonOptions when adding mvc. Otherwise you must sync your serializer settings manually.
        /// </summary>
        /// <param name="settings">The settings to set to default.</param>
        /// <returns>The settings.</returns>
        public static JsonSerializerSettings SetToHalcyonDefault(this JsonSerializerSettings settings)
        {
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new StringEnumConverter());
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.Include; //Reccomended default to allow use of DefaultValue attribute, want to serialize the value no matter what, and not set the value when deserializing.
            return settings;
        }
    }
}
