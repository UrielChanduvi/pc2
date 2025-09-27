using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configuración de Redis y sesiones
builder.Services.AddStackExchangeRedisCache(options =>
{
    // Si existe la variable de entorno la usa, si no, usa localhost para dev
    options.Configuration = builder.Configuration["REDIS_CONNECTION_STRING"] ?? "localhost:6379";
    options.InstanceName = "PortalInmobiliario:"; 
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ✅ Identity con roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
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

app.UseAuthentication();
app.UseAuthorization();

app.UseSession(); // ✅ mover antes de MapRazorPages para que se aplique bien

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Inicializar datos de ejemplo (roles y usuario Broker demo)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    DbInitializer.Seed(context);

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (!roleManager.RoleExistsAsync("Broker").Result)
    {
        roleManager.CreateAsync(new IdentityRole("Broker")).Wait();
    }

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var brokerUser = userManager.FindByEmailAsync("broker@demo.com").Result;
    if (brokerUser != null && !userManager.IsInRoleAsync(brokerUser, "Broker").Result)
    {
        userManager.AddToRoleAsync(brokerUser, "Broker").Wait();
    }
}

app.Run();
