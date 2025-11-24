using CubArt.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using CubArt.Application.Common.Services;

namespace CubArt.Api.Attributes
{
    public class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly PermissionEnum _permission;

        public RequirePermissionAttribute(PermissionEnum permission)
        {
            _permission = permission;
            Policy = "PermissionPolicy";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
            if (authService == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var hasPermission = authService.HasPermissionAsync(_permission).GetAwaiter().GetResult();
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class RequireAnyPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly PermissionEnum[] _permissions;

        public RequireAnyPermissionAttribute(params PermissionEnum[] permissions)
        {
            _permissions = permissions;
            Policy = "PermissionPolicy";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();
            if (authService == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var hasAnyPermission = authService.HasAnyPermissionAsync(_permissions).GetAwaiter().GetResult();
            if (!hasAnyPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }

}
