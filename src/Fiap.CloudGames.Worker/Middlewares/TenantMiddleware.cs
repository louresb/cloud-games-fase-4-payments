using Serilog.Context;

namespace Fiap.CloudGames.Worker.Middlewares;

public sealed class TenantMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Tenant-Id";
    public const string ContextKey = "TenantId";
    private const string LogPropertyName = "TenantId";
    private const string Default = "FIAP";
    private static readonly HashSet<string> Known = new(StringComparer.OrdinalIgnoreCase) { "FIAP", "Alura", "PM3" };

    public async Task InvokeAsync(HttpContext context)
    {
        var raw = context.Request.Headers.TryGetValue(HeaderName, out var v) ? v.ToString() : null;
        var tenant = !string.IsNullOrWhiteSpace(raw) && Known.Contains(raw) ? raw : Default;
        context.Items[ContextKey] = tenant;

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderName))
            {
                context.Response.Headers[HeaderName] = tenant;
            }
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(LogPropertyName, tenant))
        {
            await next(context);
        }
    }
}
