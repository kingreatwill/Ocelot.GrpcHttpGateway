using System;
using System.Collections.Generic;
using System.Text;

namespace Built.Grpcc.SwaggerGen
{
    public class WebApiRoute
    {
        public WebApiRoute(string controllerName, string routeTemplate)
        {
            this.ControllerName = controllerName;
            this.RouteTemplate = routeTemplate;
        }

        public string ControllerName { get; }

        public string RouteTemplate { get; }
    }
}