using Louvre.Helpers;
using Louvre.Shared.Repository;
using Louvre.Shared.Repository.General;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Progbiz.DapperEntity;
using System;
using System.Data;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;


// Data Protection
var keysDir = Path.Combine(environment.ContentRootPath, "wwwroot", "Keys");
Directory.CreateDirectory(keysDir);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDir))
    .SetApplicationName("CustomCookieAuthentication");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);   // Shorten lifetime to reasonable value
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);    // Short session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});


builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});


// HTTP Context Accessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Database connection factory
var connectionString = configuration["DBSettingKey"] switch
{
    "production" => "Data source=.;Initial Catalog=LouvreNewlat;user id=sa;password=P@ssw0rd123;integrated security=false;",
    "test" => "Data source=.;Initial Catalog=LouvreNewlat;user id=sa;password=P@ssw0rd123;integrated security=false;",
    "dev-test" => "Data Source=192.64.87.146;Initial Catalog=_Lvr;User ID=LVR;Password=Ashique@2025;Encrypt=True;TrustServerCertificate=True;",
    _ => "Server=ASHIQUE-PC\\SQLEXPRESS;Database=Louvre;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
};

builder.Services.AddScoped<IDbConnection>(_ => new Microsoft.Data.SqlClient.SqlConnection(connectionString));
builder.Services.AddScoped<IDbContext, DbContext>();
builder.Services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<ICommonRepository, CommonRepository>();
builder.Services.AddScoped<IReflexionRepository, ReflexionRepository>();
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();

builder.Services.AddHttpClient();

// Routing options
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Razor Pages setup
var mvcBuilder = builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = null)
    .AddMvcOptions(options =>
    {
        //options.Filters.Add<ValidateModelPageFilter>();
    });



builder.Services.AddControllers();

if (environment.IsDevelopment())
{
    //mvcBuilder.AddRazorRuntimeCompilation();
}

var app = builder.Build();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbInit = services.GetRequiredService<IDatabaseInitializer>();
        dbInit.InsertDefaultEntries().Wait();
    }
    catch
    {
        // Log if needed
    }
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();
app.UseStatusCodePagesWithRedirects("/errors/{0}");
app.MapRazorPages();
app.MapControllers();
app.UseMiddleware<Middleware>();
app.Run();
