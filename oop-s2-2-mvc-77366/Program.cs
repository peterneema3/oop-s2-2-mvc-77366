using FoodInspectionService.Data;
using FoodInspectionService.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Temporary provider for UserName enricher
var serviceProvider = builder.Services.BuildServiceProvider();
var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodInspectionService")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.With(new UserNameEnricher(httpContextAccessor))
    .WriteTo.Console()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Seed database + roles + default users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    context.Database.Migrate();

    // Seed fake data
    DbInitializer.Seed(context);

    // Create roles
    string[] roles = { "Admin", "Inspector", "Viewer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create admin user
    string adminEmail = "neemapr3@gmail.com";
    string adminPassword = "Fawe20202021!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (!result.Succeeded)
        {
            throw new Exception("Admin user could not be created.");
        }
    }

    
    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Create inspector user
    string inspectorEmail = "neemapeter537@gmail.com";
    string inspectorPassword = "Fawe20212022!";

    var inspectorUser = await userManager.FindByEmailAsync(inspectorEmail);

    if (inspectorUser == null)
    {
        inspectorUser = new IdentityUser
        {
            UserName = inspectorEmail,
            Email = inspectorEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(inspectorUser, inspectorPassword);

        if (!result.Succeeded)
        {
            throw new Exception("Inspector user could not be created.");
        }
    }

    // Ensure inspector is always in Inspector role
    if (!await userManager.IsInRoleAsync(inspectorUser, "Inspector"))
    {
        await userManager.AddToRoleAsync(inspectorUser, "Inspector");
    }

    // Create viewer user
    string viewerEmail = "neemabusiness3@gmail.com";
    string viewerPassword = "Fawe20222023!";

    var viewerUser = await userManager.FindByEmailAsync(viewerEmail);

    if (viewerUser == null)
    {
        viewerUser = new IdentityUser
        {
            UserName = viewerEmail,
            Email = viewerEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(viewerUser, viewerPassword);

        if (!result.Succeeded)
        {
            throw new Exception("Viewer user could not be created.");
        }
    }

    // Ensure viewer is always in Viewer role
    if (!await userManager.IsInRoleAsync(viewerUser, "Viewer"))
    {
        await userManager.AddToRoleAsync(viewerUser, "Viewer");
    }
}

// Global exception logging
app.UseSerilogRequestLogging();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();