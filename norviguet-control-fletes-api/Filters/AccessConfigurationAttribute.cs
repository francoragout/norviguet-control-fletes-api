using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using norviguet_control_fletes_api.Entities;
using norviguet_control_fletes_api.Services;

public class AccessConfigurationAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _action;
    public AccessConfigurationAttribute(string action)
    {
        _action = action;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var service = context.HttpContext.RequestServices.GetService<IAccessConfigurationService>();
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var roleClaim = user.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        if (!Enum.TryParse<UserRole>(roleClaim, out var role))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Permitir acceso total al rol Admin
        if (role == UserRole.Admin)
        {
            return;
        }

        var route = context.HttpContext.Request.Path.Value ?? "";
        var httpMethod = context.HttpContext.Request.Method;

        var hasAccess = await service!.HasAccessAsync(route, httpMethod, role, _action);
        if (!hasAccess)
        {
            context.Result = new ForbidResult();
        }
    }
}