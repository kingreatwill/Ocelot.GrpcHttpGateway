using System;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.Orleans.SwaggerGen
{
    public class WebApiRoute
    {
       public WebApiRoute (string controllerName,string routeTemplate)
        {
            this.ControllerName = controllerName;
            this.RouteTemplate = routeTemplate;
        }
        public string ControllerName { get;  }

        public string RouteTemplate { get; }
    }
}
