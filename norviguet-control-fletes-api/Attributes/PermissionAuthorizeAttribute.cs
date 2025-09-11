using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using norviguet_control_fletes_api.Data;

public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Verifica si el usuario es admin por claim (ahora compara "Admin" correctamente)
        var roleClaim = context.HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == "role" || c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase));
        if (roleClaim != null && string.Equals(roleClaim.Value, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            // Permite acceso sin más validaciones
            return;
        }

        var route = context.HttpContext.Request.Path.Value ?? string.Empty;
        var method = context.HttpContext.Request.Method;

        var db = context.HttpContext.RequestServices.GetRequiredService<NorviguetDbContext>();
        var hasPermission = await db.Permissions
            .AnyAsync(p => p.UserId == userId &&
                           p.Route == route &&
                           p.Method == method);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}