using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Swashbuckle.Orleans.SwaggerGen
{
    public interface ISwaggerGenGrain:IGrainWithGuidKey
    {
        Task<string> Generator();
    }
}
