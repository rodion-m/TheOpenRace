using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OpenRace;

public class IgnoreApiRouteConstraint : IRouteConstraint
{
    public bool Match(
        HttpContext? httpContext, 
        IRouter? route, 
        string routeKey,
        RouteValueDictionary values, 
        RouteDirection routeDirection)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(route);
        if (values.TryGetValue(routeKey, out var pathValue) && pathValue is string path)
        {
            return !path.StartsWith("api", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}