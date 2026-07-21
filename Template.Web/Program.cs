using Template.Core.Services.AdAuthentication;
using Template.Core.Repository.Auditable;
using Template.Data;
using Template.Data.Configurations;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SmartBreadcrumbs.Extensions;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.AddLogging();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddResponseCaching();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configure Blazor
builder.Services.AddServerSideBlazor()
    // Detailed circuit errors may contain sensitive data, so expose them only locally.
    .AddCircuitOptions(options => options.DetailedErrors = builder.Environment.IsDevelopment());
builder.Services.AddBlazorBootstrap();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Keep the server session aligned with the authentication cookie lifetime.
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddApiEndpoints();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/LogoutAsync";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();

// Register the LDAP authentication service with the interface
builder.Services.AddSingleton<IAdAuthenticationService>(provider =>
    new AdAuthenticationService(
        builder.Configuration["Authentication:Ldap:Server"]
            ?? throw new InvalidOperationException("Authentication:Ldap:Server is required."),
        builder.Configuration["Authentication:Ldap:Container"]
            ?? throw new InvalidOperationException("Authentication:Ldap:Container is required."),
        provider.GetRequiredService<ILogger<AdAuthenticationService>>()
    ));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

// Register one DbContext and attach the auditing interceptor to that exact instance.
// The previous three registrations could silently override one another.
builder.Services.AddScoped<AuditSaveChangesInterceptor>();
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    options.UseSqlServer(connectionString)
        .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

//Template.Core Service settings
CoreServicesRegistration.AddCoreServices(builder.Services);

builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
{
    options.TagName = "nav";
    options.TagClasses = "";
    options.OlClasses = "breadcrumb";
    options.LiClasses = "breadcrumb-item";
    options.ActiveLiClasses = "breadcrumb-item active";
});

var app = builder.Build();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

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

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
//app.UseMiddleware<LastActivityMiddleware>();
app.MapIdentityApi<ApplicationUser>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}/{param1?}");

app.MapBlazorHub();

app.Run();
