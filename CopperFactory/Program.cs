using CopperFactory;
using CopperFactory.Interfaces;
using CopperFactory.Models;
using CopperFactory.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using System.Web.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;

}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Account/login";
    options.LogoutPath = $"/Account/logout";
    options.AccessDeniedPath = $"/Account/accessDenied";
    //options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container.
builder.Services.AddLocalization(opt => { opt.ResourcesPath = "Recourses"; });

builder.Services.AddMvc().AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultres = new List<CultureInfo> {
        new CultureInfo("en-US"),
        new CultureInfo("ar-EG")
    };
    options.DefaultRequestCulture = new RequestCulture("ar-EG");
    options.SupportedCultures = supportedCultres;
    options.SupportedUICultures = supportedCultres;
});

builder.Services.AddControllersWithViews().AddViewLocalization().AddDataAnnotationsLocalization(options => {
    var type = typeof(SharedResource);
    var assemplyName = new AssemblyName(type.Assembly.FullName);
    var factory = builder.Services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
    var localizer = factory.Create("SharedResource", assemplyName.Name);
    options.DataAnnotationLocalizerProvider = (t, f) => localizer;
});

builder.Services.AddTransient<IUnityOfWork,UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();   
}
else
{
    app.UseStatusCodePagesWithRedirects("/Home/NotFound");
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting(); 

app.UseAuthorization();

var supportedCulture = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("ar-EG")
};
var options = ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>();

app.UseRequestLocalization(options.Value);

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "Admin",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
