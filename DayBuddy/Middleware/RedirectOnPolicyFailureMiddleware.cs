using Microsoft.AspNetCore.Authorization;

namespace DayBuddy.Middleware
{
    public class RedirectOnPolicyFailureMiddleware : IMiddleware
    {
        private readonly IAuthorizationService _authorizationService;

        public RedirectOnPolicyFailureMiddleware(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var endpoint = context.GetEndpoint();

            Console.WriteLine("rawr");

            // Skip if no endpoint or no Authorize attribute is found
            if (endpoint == null)
            {
                await next(context);
                return;
            }

            // Check if the endpoint has the AuthorizeAttribute with a policy
            var authorizeAttributes = endpoint.Metadata.GetOrderedMetadata<AuthorizeAttribute>();
            foreach (var authorizeAttribute in authorizeAttributes)
            {
                var policyName = authorizeAttribute.Policy;
                if (string.IsNullOrEmpty(policyName))
                {
                    continue;
                }

                // Attempt to authorize the user based on the policy
                var result = await _authorizationService.AuthorizeAsync(context.User, null, policyName);

                if (!result.Succeeded)
                {
                    // Custom logic to determine where to redirect
                    var redirectTo = GetRedirectUrlForFailedPolicy(policyName);
                    context.Response.Redirect(redirectTo);
                    return;
                }
            }

            await next(context);
        }

        private string GetRedirectUrlForFailedPolicy(string policyName)
        {
            // Custom redirection logic based on the policy name
            if (policyName == "EmailVerified")
            {
                return "/Home/Index";
            }

            // You can extend this logic for other policies
            return "/Account/AccessDenied";
        }
    }
}
