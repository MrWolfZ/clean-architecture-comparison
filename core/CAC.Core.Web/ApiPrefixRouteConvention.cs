using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CAC.Core.Web
{
    internal sealed class ApiPrefixRouteConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel centralPrefix;

        public ApiPrefixRouteConvention(IRouteTemplateProvider routeTemplateProvider)
        {
            centralPrefix = new AttributeRouteModel(routeTemplateProvider);
        }

        public void Apply(ApplicationModel application)
        {
            var apiControllers = application.Controllers.Where(c => c.Attributes.Any(a => a.GetType() == typeof(ApiControllerAttribute)));
            foreach (var selectorModel in apiControllers.SelectMany(c => c.Selectors))
            {
                selectorModel.AttributeRouteModel = selectorModel.AttributeRouteModel != null
                    ? AttributeRouteModel.CombineAttributeRouteModel(centralPrefix, selectorModel.AttributeRouteModel)
                    : centralPrefix;
            }
        }
    }
}
