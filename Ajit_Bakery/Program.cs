using Ajit_Bakery.Data;
using Ajit_Bakery.Services;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//ADD NOTIFY
builder.Services.AddNotyf(config => 
{ 
    config.DurationInSeconds = 5; 
    config.IsDismissable = true; 
    config.Position = NotyfPosition.TopRight; 
});
//ADD CONNECTION STRINGG
var connectionString = builder.Configuration.GetConnectionString("ContextDBConnection") ?? throw new InvalidOperationException("Connection string 'ContextDBConnection' not found.");
builder.Services.AddDbContext<DataDBContext>(options =>
options.UseNpgsql(connectionString));

//Register API Service 
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<WeighingScaleService>();

builder.Services.AddHttpClient();
//end

//Login page 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "AjiBakery";
        option.LoginPath = "/UserMasters/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        //option.ExpireTimeSpan = TimeSpan.FromSeconds(10);
        option.AccessDeniedPath = "/Access/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(name: "User", configurePolicy: policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "User");
    });
});
//end
builder.Services.AddSession();
// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the Background Service
builder.Services.AddHostedService<BoxMasterResetService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//USE SETTION
app.UseSession();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseNotyf();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UserMasters}/{action=Login}/{id?}");

app.Run();
