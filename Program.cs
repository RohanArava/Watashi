using Microsoft.EntityFrameworkCore;
using Watashi.Data;
using Watashi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<WatashiDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WatashiDbConnection"))
);

builder.Services.AddSingleton<RsaKeyService>();

builder
    .Services.AddAuthentication("WatashiCookie")
    .AddCookie(
        "WatashiCookie",
        options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.Cookie.Name = "Watashi.Auth";
        }
    );

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=User}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
