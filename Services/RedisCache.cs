using System.Text.Json;
using StackExchange.Redis;

namespace MiniService.Services;

/// <summary>
/// Cache Redis "mềm": nếu có REDIS_URL thì bật, không có thì fallback no-op (app vẫn chạy).
/// Dùng cho các truy vấn đọc-nhiều (dashboard, danh sách) để chịu tải tốt hơn.
/// </summary>
public interface ICache
{
    bool Enabled { get; }
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan ttl);
    Task RemoveByPrefixAsync(string prefix);
}

public sealed class RedisCache : ICache
{
    private readonly IConnectionMultiplexer? _mux;
    private readonly ILogger<RedisCache> _log;
    public bool Enabled => _mux is { IsConnected: true };

    public RedisCache(IConfiguration cfg, ILogger<RedisCache> log)
    {
        _log = log;
        var url = Environment.GetEnvironmentVariable("REDIS_URL") ?? cfg["REDIS_URL"];
        if (string.IsNullOrWhiteSpace(url)) { _log.LogInformation("Redis: chưa cấu hình REDIS_URL — chạy không cache."); return; }
        try
        {
            var opts = ConfigurationOptions.Parse(ToStackExchange(url));
            opts.AbortOnConnectFail = false; opts.ConnectTimeout = 5000;
            _mux = ConnectionMultiplexer.Connect(opts);
            _log.LogInformation("Redis: đã kết nối ({Endpoint}).", string.Join(",", opts.EndPoints));
        }
        catch (Exception ex) { _log.LogWarning("Redis: kết nối thất bại — chạy không cache. {Err}", ex.Message); }
    }

    // Chấp nhận redis://user:pass@host:port hoặc host:port,password=...
    private static string ToStackExchange(string url)
    {
        if (!url.Contains("://")) return url;
        var u = new Uri(url);
        var pass = u.UserInfo.Contains(':') ? u.UserInfo.Split(':', 2)[1] : u.UserInfo;
        var ssl = url.StartsWith("rediss://");
        return $"{u.Host}:{(u.Port > 0 ? u.Port : 6379)},password={Uri.UnescapeDataString(pass)},ssl={ssl.ToString().ToLower()},abortConnect=false";
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (!Enabled) return default;
        try { var v = await _mux!.GetDatabase().StringGetAsync(key); return v.HasValue ? JsonSerializer.Deserialize<T>(v!) : default; }
        catch { return default; }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        if (!Enabled) return;
        try { await _mux!.GetDatabase().StringSetAsync(key, JsonSerializer.Serialize(value), ttl); } catch { }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        if (!Enabled) return;
        try
        {
            foreach (var ep in _mux!.GetEndPoints())
                foreach (var k in _mux.GetServer(ep).Keys(pattern: prefix + "*"))
                    await _mux.GetDatabase().KeyDeleteAsync(k);
        }
        catch { }
    }
}
