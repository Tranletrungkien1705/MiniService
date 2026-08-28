using System.Text;
using System.Text.Json;
using MiniService.Models;

namespace MiniService.Services;

public record EInvoiceResult(bool ok, string status, string? tctCode, string? error);

public interface IIntegrationService
{
    // Đẩy hóa đơn điện tử sang MiniTVAN (phát hành + truyền cơ quan thuế).
    Task<EInvoiceResult> PushEInvoiceAsync(RepairOrder ro);
    // Gửi thông báo cho khách qua MiniNotify.
    Task NotifyCustomerAsync(RepairOrder ro);
}

public class IntegrationService(IHttpClientFactory http, IConfiguration cfg) : IIntegrationService
{
    private static string Env(IConfiguration c, string key, string def) =>
        Environment.GetEnvironmentVariable(key) ?? c[key] ?? def;

    public async Task<EInvoiceResult> PushEInvoiceAsync(RepairOrder ro)
    {
        var url = Env(cfg, "TVAN_URL", "https://minitvan.onrender.com").TrimEnd('/');
        var apiKey = Env(cfg, "TVAN_APIKEY", "demo-tvan");
        var sellerMst = Env(cfg, "SELLER_MST", "0101243150");
        var sellerName = Env(cfg, "SELLER_NAME", "Trung tâm dịch vụ ô tô (Demo)");

        var payload = new
        {
            sellerMst,
            sellerName,
            buyerName = ro.Customer?.Name ?? "Khách lẻ",
            buyerMst = (string?)null,
            buyerAddress = ro.Customer?.Phone,
            amount = ro.Total,
            vatRate = 10m,
            docRef = ro.Code
        };
        try
        {
            var c = http.CreateClient();
            c.Timeout = TimeSpan.FromSeconds(30);
            var req = new HttpRequestMessage(HttpMethod.Post, $"{url}/api/invoices")
            { Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json") };
            req.Headers.Add("X-Api-Key", apiKey);
            var res = await c.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var tct = root.TryGetProperty("tctCode", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : null;
            var status = root.TryGetProperty("status", out var s) ? s.GetString() : null;
            if (res.IsSuccessStatusCode && !string.IsNullOrEmpty(tct))
                return new(true, status ?? "Accepted", tct, null);
            var err = root.TryGetProperty("error", out var e) ? e.GetString() : $"HTTP {(int)res.StatusCode}";
            return new(false, status ?? "Error", tct, err);
        }
        catch (Exception ex)
        {
            return new(false, "Error", null, "Không kết nối được MiniTVAN: " + ex.Message);
        }
    }

    public async Task NotifyCustomerAsync(RepairOrder ro)
    {
        var url = Env(cfg, "NOTIFY_URL", "https://mininotify.onrender.com").TrimEnd('/');
        var apiKey = Env(cfg, "NOTIFY_APIKEY", "demo-notify");
        var to = ro.Customer?.Email;
        if (string.IsNullOrWhiteSpace(to)) return;
        var payload = new
        {
            channel = "Email",
            to,
            subject = $"Hóa đơn dịch vụ {ro.Code}",
            body = $"Kính gửi {ro.Customer?.Name},\nXe {ro.Car?.Plate} đã hoàn tất dịch vụ. Tổng thanh toán {ro.Total:N0}đ."
                 + (ro.EInvoiceCode != null ? $"\nMã tra cứu HĐĐT: {ro.EInvoiceCode}" : "")
        };
        try
        {
            var c = http.CreateClient();
            c.Timeout = TimeSpan.FromSeconds(20);
            var req = new HttpRequestMessage(HttpMethod.Post, $"{url}/api/send")
            { Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json") };
            req.Headers.Add("X-Api-Key", apiKey);
            await c.SendAsync(req);
        }
        catch { /* thông báo là best-effort, không chặn nghiệp vụ */ }
    }
}
