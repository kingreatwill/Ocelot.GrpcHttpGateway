using System;
using Microsoft.Extensions.DependencyInjection;

namespace Built.Grpcc
{
    /*
     public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        //app.UseMvc()..
        //最末的时候赋值
        ServiceLocator.Instance = app.ApplicationServices;
    }
    */

    public static class ServiceLocator
    {
        public static IServiceProvider Instance { get; set; }

        public static T GetService<T>() where T : class
        {
            return Instance.GetService<T>();
        }
    }
}