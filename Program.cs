using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PortalInmobiliario.Data;


var builder = WebApplication.CreateBuilder(args);
// Configuración de Redis y sesiones
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configurar acceso denegado para Broker
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 403)
    {
        context.HttpContext.Response.Redirect("/Broker/Shared/AccessDenied");
    }
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSession();

app.MapRazorPages();

// Inicializar datos de ejemplo
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<PortalInmobiliario.Data.ApplicationDbContext>();
    PortalInmobiliario.Data.DbInitializer.Seed(context);

    // Crear rol Broker si no existe
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!roleManager.RoleExistsAsync("Broker").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Broker")).Wait();
    }
    // Asignar rol Broker a un usuario demo (puedes cambiar el email)
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var brokerUser = userManager.FindByEmailAsync("broker@demo.com").Result;
    if (brokerUser != null && !userManager.IsInRoleAsync(brokerUser, "Broker").Result)
    {
        userManager.AddToRoleAsync(brokerUser, "Broker").Wait();
    }
}

app.Run();

// Inicializar datos de ejemplo
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<PortalInmobiliario.Data.ApplicationDbContext>();
    PortalInmobiliario.Data.DbInitializer.Seed(context);

    // Crear rol Broker si no existe
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!roleManager.RoleExistsAsync("Broker").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Broker")).Wait();
    }
    // Asignar rol Broker a un usuario demo (puedes cambiar el email)
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var brokerUser = userManager.FindByEmailAsync("broker@demo.com").Result;
    if (brokerUser != null && !userManager.IsInRoleAsync(brokerUser, "Broker").Result)
    {
        userManager.AddToRoleAsync(brokerUser, "Broker").Wait();
    }
}
