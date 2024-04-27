using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CmApi.Classes;

public class OAuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {

        var config = filterContext.HttpContext.RequestServices.GetService<IConfiguration>();

        if (!string.IsNullOrEmpty(config?.GetSection("Debug")["SkipAuth"]))
        {
            filterContext.HttpContext.User = new GenericPrincipal(new AuthenticatedUser("adminbot"), []);
            return;
        };
        
        filterContext.HttpContext.Request.Headers.TryGetValue("X-Forwarded-Preferred-Username", out var user);
        if (user.Count == 0)
        {
            filterContext.Result = new UnauthorizedObjectResult("User is unauthorized.");
        }

        filterContext.HttpContext.User = new GenericPrincipal(new AuthenticatedUser(user.First() ?? string.Empty), []);
    }

}

public class AuthenticatedUser(string name) : IIdentity
{
    public string? AuthenticationType { get; } = "Oauth";
    public bool IsAuthenticated { get; } = true;
    public string? Name { get; } = name;
}