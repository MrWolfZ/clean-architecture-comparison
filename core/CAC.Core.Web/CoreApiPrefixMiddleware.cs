using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CAC.Core.Web
{
    public static class CoreApiPrefixMiddleware
    {
        public static void UseApiPrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Insert(0, new ApiPrefixRouteConvention(routeAttribute));
        }

        public static void UseApiPrefix(this MvcOptions opts, string prefixRouteTemplate = "api")
        {
            opts.UseApiPrefix(new RouteAttribute(prefixRouteTemplate));
        }
    }
}
