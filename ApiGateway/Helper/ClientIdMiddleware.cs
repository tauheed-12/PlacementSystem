namespace ApiGateway.Helper
{
    public class ClientIdMiddleware
    {
        private readonly RequestDelegate _requestDelegate;

        public ClientIdMiddleware(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            var userId = context.User?.FindFirst("sub")?.Value
                ?? context.User?.FindFirst("userId")?.Value
                ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                context.Request.Headers["X-Client-Id"] = userId;
            }

            await _requestDelegate(context);
        }
    }
}
