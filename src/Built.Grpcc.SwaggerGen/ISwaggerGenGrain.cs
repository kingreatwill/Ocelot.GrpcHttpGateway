using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Built.Grpcc.SwaggerGen
{
    public interface ISwaggerGenGrain
    {
        Task<string> Generator();
    }
}