using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Built.Grpcc.SwaggerGen
{
    public class GrainKeyParmeterFilter : IParameterFilter
    {
        private readonly GrpcSwaggerGenOptions options;

        public GrainKeyParmeterFilter(GrpcSwaggerGenOptions options)
        {
            this.options = options;
        }

        public void Apply(IParameter parameter, ParameterFilterContext context)
        {
            if (this.options.GrainInterfaceGrainKeyAsName.TryGetValue(context.ParameterInfo.Member.DeclaringType, out GrainKeyDescription keyDescription))
            {
                if (context.ParameterInfo.Name == keyDescription.Name)
                    parameter.Description = keyDescription.Description;
            }
        }
    }
}