using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Built.Grpcc.SwaggerGen
{
    public class GrainKeyDescription
    {
        public GrainKeyDescription(string name, string des)
       : this(name, des, null)
        {
        }

        public GrainKeyDescription(string name, string des, params string[] noNeedKeyMethod)
        {
            this.IgnoreGrainKey = false;
            this.Name = name;
            this.Description = des;
            this.NoNeedKeyMethod = noNeedKeyMethod?.ToList() ?? new List<string>();
        }

        public GrainKeyDescription(bool allIgnoreGrainKey)
        {
            this.IgnoreGrainKey = true;
        }

        public bool IgnoreGrainKey { get; }
        public string Name { get; }
        public string Description { get; }
        public List<string> NoNeedKeyMethod { get; set; } = new List<string>();
    }
}