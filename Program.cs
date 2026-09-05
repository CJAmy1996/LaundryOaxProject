using System.Security.Claims;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;
using LaundryOaxWebAPI.Data;
using LaundryOaxWebAPI.IoT;
using LaundryOaxWebAPI.Services.OrderServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var demo = builder.Configuration.GetValue<bool>("Demo:Enabled");
if (demo && !builder.Environment.IsDevelopment()) throw new InvalidOperationException("Demo mode requires Development.");
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 32 * 1024);
builder.Services.AddProblemDetails();
builder.Services.Configure<ForwardedHeadersOptions>(o => {
    o.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    o.ForwardLimit = 1;
    if (IPAddress.TryParse(builder.Configuration["Proxy:KnownProxy"], out var proxy)) o.KnownProxies.Add(proxy);
    if (IPAddress.TryParse(builder.Configuration["Proxy:KnownNetwork"], out var network) &&
        int.TryParse(builder.Configuration["Proxy:PrefixLength"], out var prefix))
        o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(network, prefix));
});
builder.Services.AddControllers(o => o.Filters.Add(new AuthorizeFilter("LegacyAdmin")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LaundryOaxDBContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("LaundryOaxConnectionString")));
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddDbContextFactory<FleetDb>(o => {
    if (builder.Configuration["Fleet:Provider"] == "SqlServer")
        o.UseSqlServer(builder.Configuration.GetConnectionString("Fleet"));
    else o.UseSqlite(builder.Configuration.GetConnectionString("Fleet") ?? "Data Source=fleet.db;Default Timeout=30");
});
builder.Services.AddSingleton<EventPipeline>();
builder.Services.AddHostedService<IngestionWorker>();
builder.Services.AddHostedService<SqsWorker>();
var auth = builder.Services.AddAuthentication(demo ? "Demo" : JwtBearerDefaults.AuthenticationScheme);
auth.AddScheme<AuthenticationSchemeOptions, DemoAuthentication>("Demo", _ => { });
auth.AddScheme<AuthenticationSchemeOptions, DeviceAuthentication>("DeviceKey", _ => { });
auth.AddJwtBearer(o => {
    o.Authority = builder.Configuration["Auth:Authority"];
    o.Audience = builder.Configuration["Auth:Audience"];
    o.RequireHttpsMetadata = true;
    o.MapInboundClaims = false;
    o.TokenValidationParameters.RoleClaimType = "roles";
    o.TokenValidationParameters.NameClaimType = "sub";
    o.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
});
if (!demo && (string.IsNullOrWhiteSpace(builder.Configuration["Auth:Authority"]) ||
              string.IsNullOrWhiteSpace(builder.Configuration["Auth:Audience"])))
    throw new InvalidOperationException("Configure Auth:Authority and Auth:Audience, or explicitly enable local Development demo mode.");
builder.Services.AddAuthorization(o => {
    o.AddPolicy("FleetRead", p => p.RequireAuthenticatedUser().RequireClaim("tenant_id").RequireRole("Viewer", "Operator", "Admin"));
    o.AddPolicy("Operator", p => p.RequireAuthenticatedUser().RequireClaim("tenant_id").RequireRole("Operator", "Admin"));
    o.AddPolicy("LegacyAdmin", p => p.RequireAuthenticatedUser().RequireRole("Admin"));
    o.AddPolicy("Device", p => p.AddAuthenticationSchemes("DeviceKey").RequireAuthenticatedUser().RequireClaim("device_id").RequireClaim("tenant_id"));
});
builder.Services.AddRateLimiter(o => {
    o.RejectionStatusCode = 429;
    o.AddPolicy("api", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.User.FindFirstValue("tenant_id") ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 600, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseForwardedHeaders();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async (ctx, next) => {
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["Referrer-Policy"] = "same-origin";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseDefaultFiles(); app.UseStaticFiles();
app.UseCors(); app.UseAuthentication(); app.UseAuthorization(); app.UseRateLimiter();
app.MapGet("/health/live", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/health/ready", async (IDbContextFactory<FleetDb> factory, CancellationToken ct) => {
    try { await using var db = await factory.CreateDbContextAsync(ct); await db.Machines.AnyAsync(ct); return Results.Ok(new { status = "ready" }); }
    catch { return Results.StatusCode(503); }
});
app.MapGet("/api/config", () => Results.Ok(new {
    demo, authority = builder.Configuration["Auth:Authority"], clientId = builder.Configuration["Auth:ClientId"],
    scope = builder.Configuration["Auth:Scope"] ?? "openid profile"
}));
app.MapFleet();
if (demo) DemoEndpoints.Map(app);
if (builder.Configuration.GetValue<bool>("Legacy:Enabled")) app.MapControllers();
app.MapFallbackToFile("index.html");
if (demo || builder.Configuration.GetValue<bool>("Fleet:Initialize")) {
    await using var scope = app.Services.CreateAsyncScope();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FleetDb>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.EnsureCreatedAsync();
    if (demo) await DemoEndpoints.Seed(db);
}
await app.RunAsync();
public partial class Program { }
