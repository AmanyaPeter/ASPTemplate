using Template.Core.Services.AdAuthentication;
using Template.Data;
using Template.Data.Configurations;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SmartBreadcrumbs.Extensions;
using System.Reflection;
using System.Threading.RateLimiting;
using Template.Core.Configuration;
using Template.Web.Hubs;
using Template.Web.Services;
using Template.Web.Middleware;
var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.AddLogging();


builder.Configuration.AddJsonFile("appsettings.json");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCaching();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

//Configure Blazor
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => options.DetailedErrors = true);
builder.Services.AddBlazorBootstrap();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});

//Add configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
builder.Services.AddAuthorization();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddRoles<IdentityRole<Guid>>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 14;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.User.RequireUniqueEmail = true;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});


// Register the LDAP authentication service with the interface

builder.Services.AddSingleton<IAdAuthenticationService>(provider =>
    new AdAuthenticationService(
        builder.Configuration["ActiveDirectory:Domain"] ?? "localhost",
        builder.Configuration["ActiveDirectory:Container"] ?? string.Empty,
        provider.GetRequiredService<ILogger<AdAuthenticationService>>()
    ));

//Template.Data Service settings
DataServicesRegistration.AddDataServices(builder.Services, builder.Configuration);

//Template.Core Service settings
CoreServicesRegistration.AddCoreServices(builder.Services);
builder.Services.AddOptions<AttachmentOptions>().BindConfiguration(AttachmentOptions.SectionName)
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<ReminderOptions>().BindConfiguration(ReminderOptions.SectionName)
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<BusinessOptions>().BindConfiguration(BusinessOptions.SectionName)
    .ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddOptions<SmtpOptions>().BindConfiguration(SmtpOptions.SectionName);
builder.Services.AddHostedService<EmailOutboxWorker>();
builder.Services.AddHostedService<DeadlineReminderWorker>();

builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
	options.TagName = "nav";
	options.TagClasses = "";
	options.OlClasses = "breadcrumb";
	options.LiClasses = "breadcrumb-item";
	options.ActiveLiClasses = "breadcrumb-item active";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add response caching middleware
app.UseResponseCaching();

// Configure static files with caching

//app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static resources for 7 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});

// Apply no-cache only to dynamic content
//app.Use(async (context, next) =>
//{
//    // Don't modify caching for static files
//    if (!context.Request.Path.StartsWithSegments("/css") &&
//        !context.Request.Path.StartsWithSegments("/js") &&
//        !context.Request.Path.StartsWithSegments("/lib") &&
//        !context.Request.Path.StartsWithSegments("/images") &&
//        !context.Request.Path.StartsWithSegments("/fonts"))
//    {
//        context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
//        context.Response.Headers["Pragma"] = "no-cache";
//        context.Response.Headers["Expires"] = "0";
//    }
//    await next();
//});

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.TraceIdentifier = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self' wss:";
    await next();
});

app.UseRouting();
app.UseRateLimiter();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditTrailMiddleware>();
//app.UseMiddleware<LastActivityMiddleware>();
app.MapIdentityApi<ApplicationUser>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}/{param1?}");

app.MapBlazorHub();
app.MapHub<ImtsHub>("/hubs/imts");
app.MapHealthChecks("/health/live");

// Seed roles and default users on startup
try
{
    await DbInitializer.SeedAsync(app.Services);
    if (app.Environment.IsDevelopment())
    {
        await DashboardSeedData.SeedAsync(app.Services);
    }
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Database migration or seeding failed.");
    throw;
}

app.Run();
