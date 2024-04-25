using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace CmApi.Classes;

public class OAuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        filterContext.HttpContext.Request.Headers.TryGetValue("X-Forwarded-Preferred-Username", out var user);
        if (user.Count == 0)
        {
            filterContext.Result = new UnauthorizedObjectResult("User is unauthorized.");
        }
    }

}