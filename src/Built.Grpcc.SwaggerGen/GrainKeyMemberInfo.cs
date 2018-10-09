using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace Built.Grpcc.SwaggerGen
{
    public class GrainKeyMemberInfo : MemberInfo
    {
        public GrainKeyMemberInfo(Type declaringType, string name)
        {
            this.DeclaringType = declaringType;
            this.Name = name;
        }

        public override Type DeclaringType { get; }

        public override MemberTypes MemberType
        {
            get
            {
                return MemberTypes.Method;
            }
        }

        public override string Name { get; }

        public override Type ReflectedType => this.DeclaringType;

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[1]{
                new  RequiredAttribute()
            };
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(RequiredAttribute))
                return GetCustomAttributes(inherit);
            else
                return new object[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }
    }
}