using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Swashbuckle.Orleans.SwaggerGen
{
    public class GrainKeyParamterInfo : ParameterInfo
    {
        public GrainKeyParamterInfo(string name, Type type, MethodInfo method)
        {
            this.NameImpl = name;
            this.ClassImpl = type;
            this.MemberImpl = new GrainKeyMemberInfo(method.DeclaringType, method.Name);
        }
    }
}
