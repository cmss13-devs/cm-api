using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

        filterContext.HttpContext.User = new GenericPrincipal(new AuthenticatedUser(user.First() ?? string.Empty), []);
    }

}

public class AuthenticatedUser : IIdentity
{
    public string? AuthenticationType { get; }
    public bool IsAuthenticated { get; }
    public string? Name { get; }

    public AuthenticatedUser(string name)
    {
        IsAuthenticated = true;
        AuthenticationType = "Oauth";
        Name = name;
    }
}