namespace AuctionManagementService.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;

    public class GlobalRoutePrefixConvention : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _centralPrefix;

        public GlobalRoutePrefixConvention(string routePrefix)
        {
            _centralPrefix = new AttributeRouteModel(new Microsoft.AspNetCore.Mvc.RouteAttribute(routePrefix));
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel != null)
                    {
                        selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_centralPrefix, selector.AttributeRouteModel);
                    }
                    else
                    {
                        selector.AttributeRouteModel = _centralPrefix;
                    }
                }
            }
        }
    }

}