using Serilog.Context;

namespace MiniService.Services;

/// <summary>
/// Gán/đọc Correlation-Id cho mỗi request và đưa vào LogContext của Serilog
/// → mọi dòng log của request (và các call tích hợp mang theo header này) truy được xuyên suốt.
/// </summary>
public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public const string Header = "X-Correlation-Id";

    public async Task Invoke(HttpContext ctx)
    {
        var cid = ctx.Request.Headers[Header].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(cid)) cid = Guid.NewGuid().ToString("N")[..16];
        ctx.Response.Headers[Header] = cid;
        ctx.Items[Header] = cid;
        using (LogContext.PushProperty("CorrelationId", cid))
            await next(ctx);
    }
}
