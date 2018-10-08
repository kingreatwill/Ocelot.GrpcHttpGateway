using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Built.Grpcc.SwaggerGen
{
    public class OrleansApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly GrpcSwaggerGenOptions options;

        public OrleansApiDescriptionGroupCollectionProvider(IOptions<GrpcSwaggerGenOptions> options)
        {
            this.options = options?.Value ?? new GrpcSwaggerGenOptions();
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                var apiDescriptions = GetApiDescriptions();
                var group = new ApiDescriptionGroup("default", apiDescriptions);
                return new ApiDescriptionGroupCollection(new[] { group }, 1);
            }
        }

        private List<ControllerActionDescriptor> CreateActionDescriptors()
        {
            return options.GrainAssembly.GetTypes()
                  .Where(type => typeof(IGrain).IsAssignableFrom(type) && type.IsPublic && type.IsInterface && !type.IsGenericType && type.Module.Name != "Orleans.Core.Abstractions.dll" && !this.options.IgnoreGrainInterfaces.Invoke(type))
                  .SelectMany(interfaceType => interfaceType.GetMethods())
                  .Where(method => method.IsPublic && !this.options.IgnoreGrainMethods.Invoke(method))
                  .Select(method =>
                  {
                      string httpMethod = "POST";
                      var grainKey = this.ResolveGrainKey(method);
                      var apiRoute = this.options.SetApiRouteTemplateFunc(method);
                      return CreateActionDescriptor(httpMethod, apiRoute.RouteTemplate, method, apiRoute.ControllerName, grainKey);
                  })
                  .ToList();
        }

        private ControllerActionDescriptor CreateActionDescriptor(string httpMethod, string routeTemplate, MethodInfo methodInfo,
            string controllerName, GrainKeyParamterInfo grainKey)
        {
            var descriptor = new ControllerActionDescriptor();
            descriptor.SetProperty(new ApiDescriptionActionData());
            descriptor.ControllerTypeInfo = methodInfo.DeclaringType.GetTypeInfo();
            descriptor.MethodInfo = methodInfo;
            descriptor.FilterDescriptors = descriptor.MethodInfo.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Select((filter) => new FilterDescriptor(filter, FilterScope.Action))
                .ToList();
            descriptor.RouteValues = new Dictionary<string, string> {
                { "controller", controllerName}
            };
            descriptor.ActionConstraints = new List<IActionConstraintMetadata>();
            if (httpMethod != null)
                descriptor.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { httpMethod }));

            descriptor.Parameters = new List<ParameterDescriptor>();
            List<ParameterInfo> ParameterInfos = descriptor.MethodInfo.GetParameters().ToList();
            if (grainKey.ParameterType != typeof(Guid))
                ParameterInfos.Insert(0, grainKey);
            foreach (var parameterInfo in ParameterInfos)
            {
                var parameterDescriptor = new ControllerParameterDescriptor
                {
                    Name = parameterInfo.Name,
                    ParameterType = parameterInfo.ParameterType,
                    ParameterInfo = parameterInfo,
                    BindingInfo = new BindingInfo()
                    {
                        BinderModelName = parameterInfo.Name,
                        BindingSource = BindingSource.Query,
                        BinderType = parameterInfo.ParameterType
                    }
                };
                if (parameterInfo.ParameterType.CanHaveChildren())
                    parameterDescriptor.BindingInfo.BindingSource = BindingSource.Body;

                if (parameterInfo is GrainKeyParamterInfo)
                {
                    routeTemplate += "/{" + grainKey.Name + "}";
                    parameterDescriptor.BindingInfo.BindingSource = BindingSource.Path;
                }
                descriptor.Parameters.Add(parameterDescriptor);
            };
            descriptor.AttributeRouteInfo = new AttributeRouteInfo { Template = routeTemplate };

            return descriptor;
        }

        private IReadOnlyList<ApiDescription> GetApiDescriptions()
        {
            var actionDescriptors = this.CreateActionDescriptors();
            var context = new ApiDescriptionProviderContext(actionDescriptors);

            var options = new MvcOptions();
            options.InputFormatters.Add(new JsonInputFormatter(Mock.Of<ILogger>(), new JsonSerializerSettings(), ArrayPool<char>.Shared, new DefaultObjectPoolProvider(), false));
            options.OutputFormatters.Add(new JsonOutputFormatter(new JsonSerializerSettings(), ArrayPool<char>.Shared));

            var constraintResolver = new Mock<IInlineConstraintResolver>();
            constraintResolver.Setup(i => i.ResolveConstraint("int")).Returns(new IntRouteConstraint());

            var provider = new DefaultApiDescriptionProvider(
                Options.Create(options),
                constraintResolver.Object,
                CreateModelMetadataProvider()
                , new ActionResultTypeMapper());

            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);
            return new ReadOnlyCollection<ApiDescription>(context.Results);
        }

        public IModelMetadataProvider CreateModelMetadataProvider()
        {
            var detailsProviders = new IMetadataDetailsProvider[]
            {
                new DefaultBindingMetadataProvider(),
                new DefaultValidationMetadataProvider(),
                new DataAnnotationsMetadataProvider(
                    Options.Create(new MvcDataAnnotationsLocalizationOptions()),
                    null)
            };

            var compositeDetailsProvider = new DefaultCompositeMetadataDetailsProvider(detailsProviders);
            return new DefaultModelMetadataProvider(compositeDetailsProvider, Options.Create(new MvcOptions()));
        }

        private GrainKeyParamterInfo ResolveGrainKey(MethodInfo method)
        {
            Type type = method.DeclaringType;
            if (!this.options.GrainInterfaceGrainKeyAsName.TryGetValue(type, out GrainKeyDescription keyDescription))
                keyDescription = new GrainKeyDescription("grainKey", "");

            Type grainType;
            if (typeof(IGrainWithGuidCompoundKey).IsAssignableFrom(type) || typeof(IGrainWithGuidKey).IsAssignableFrom(type))
                grainType = typeof(Guid);
            else if (typeof(IGrainWithIntegerCompoundKey).IsAssignableFrom(type) || typeof(IGrainWithIntegerKey).IsAssignableFrom(type))
                grainType = typeof(long);
            else if (typeof(IGrainWithStringKey).IsAssignableFrom(type) || typeof(IGrainWithStringKey).IsAssignableFrom(type))
                grainType = typeof(string);
            else
                return null;

            //When setting the method does not require GrainKey, set to Guid
            if (keyDescription.IgnoreGrainKey)
                grainType = typeof(Guid);

            if (keyDescription.NoNeedKeyMethod.Exists(f => f.Equals(method.Name, StringComparison.OrdinalIgnoreCase)))
                grainType = typeof(Guid);

            return new GrainKeyParamterInfo(keyDescription.Name, grainType, method);
        }
    }
}