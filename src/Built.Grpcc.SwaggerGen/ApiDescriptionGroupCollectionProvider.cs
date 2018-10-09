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
    public class ApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly GrpcSwaggerGenOptions options;

        public ApiDescriptionGroupCollectionProvider(IOptions<GrpcSwaggerGenOptions> options)
        {
            this.options = options?.Value ?? new GrpcSwaggerGenOptions();
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                var apiDescriptions = GetApiDescriptions();
                var group = new ApiDescriptionGroup("default", apiDescriptions);//default 可以是srvName或者dllName
                return new ApiDescriptionGroupCollection(new[] { group }, 1);
            }
        }

        private List<ControllerActionDescriptor> CreateActionDescriptors()
        {
            return options.GrainAssembly.GetTypes()
                  .Where(type =>
                  // typeof(IGrain).IsAssignableFrom(type) &&
                  type.IsPublic
                  && type.IsInterface
                  && !type.IsGenericType
                  && !this.options.IgnoreGrainInterfaces.Invoke(type))
                  .SelectMany(interfaceType => interfaceType.GetMethods())
                  .Where(method => method.IsPublic && !this.options.IgnoreGrainMethods.Invoke(method))
                  .Select(method =>
                  {
                      string httpMethod = "POST";
                      var apiRoute = this.options.SetApiRouteTemplateFunc(method);
                      return CreateActionDescriptor(httpMethod, apiRoute.RouteTemplate, method, apiRoute.ControllerName);
                  })
                  .ToList();
        }

        private ControllerActionDescriptor CreateActionDescriptor(string httpMethod, string routeTemplate, MethodInfo methodInfo,
            string controllerName)
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

                //if (parameterInfo is GrainKeyParamterInfo)
                //{
                //    routeTemplate += "/{" + grainKey.Name + "}";
                //    parameterDescriptor.BindingInfo.BindingSource = BindingSource.Path;
                //}
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
            options.InputFormatters.Add(
                new JsonInputFormatter(Mock.Of<ILogger>(), new JsonSerializerSettings(), ArrayPool<char>.Shared, new DefaultObjectPoolProvider(), false)
                );
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
    }
}