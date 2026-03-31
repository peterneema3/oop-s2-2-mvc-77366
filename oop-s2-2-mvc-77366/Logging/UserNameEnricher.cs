using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace FoodInspectionService.Logging
{
    public class UserNameEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserNameEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            string userName = "Anonymous";

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                userName =
                    httpContext.User.Identity?.Name ??
                    httpContext.User.FindFirst(ClaimTypes.Name)?.Value ??
                    httpContext.User.FindFirst(ClaimTypes.Email)?.Value ??
                    "AuthenticatedUser";
            }

            var property = propertyFactory.CreateProperty("UserName", userName);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}