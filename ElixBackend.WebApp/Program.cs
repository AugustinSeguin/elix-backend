using Microsoft.EntityFrameworkCore;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.IRepository;
using ElixBackend.Infrastructure.Repository;
using ElixBackend.Business.IService;
using ElixBackend.Business.Service;
using ElixBackend.WebApp.Services;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.FileProviders;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var currentDirectory = Directory.GetCurrentDirectory();

var configuration = new ConfigurationBuilder()
    .SetBasePath(currentDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", environment)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting WebApp host ({Environment})...", environment);
    Log.Information("Current Working Directory: {Directory}", currentDirectory);

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        EnvironmentName = environment,
        ContentRootPath = currentDirectory // Aligné avec l'API
    });

    builder.Configuration.Sources.Clear();
    builder.Configuration.AddConfiguration(configuration);

    builder.Host.UseSerilog();

    builder.Services.AddDbContext<ElixDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddControllersWithViews()
        .AddRazorRuntimeCompilation();
    builder.Services.AddRazorPages();

    // Injection de dépendances
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ITokenRepository, TokenRepository>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
    builder.Services.AddScoped<IQuestionService, QuestionService>();
    builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
    builder.Services.AddScoped<IAnswerService, AnswerService>();
    builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
    builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
    builder.Services.AddScoped<IResourceService, ResourceService>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<TokenPropagationHandler>();
    builder.Services.AddHttpClient("ApiClient")
        .AddHttpMessageHandler<TokenPropagationHandler>();

    builder.Services.AddAuthentication("Cookies")
        .AddCookie("Cookies", options =>
        {
            options.LoginPath = "/User/Login";
            options.AccessDeniedPath = "/User/Login";
            options.Cookie.Name = "ElixAuthCookie";
        });

    var app = builder.Build();

    if (!app.Environment.IsProduction())
    {
        app.UseDeveloperExceptionPage();
        app.UseHttpsRedirection();
    }
    else 
    {
        app.UseExceptionHandler("/Home/Error");
    }

    // Serve les fichiers statiques de base (CSS, JS) de wwwroot
    app.UseStaticFiles();

    // --- CONFIGURATION DES UPLOADS (SYNCHRO AVEC API) ---
    var uploadsPath = app.Configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
    
    // Chemin absolu vers le dossier partagé
    var physicalPath = Path.IsPathRooted(uploadsPath)
        ? uploadsPath
        : Path.Combine(app.Environment.ContentRootPath, uploadsPath);

    Log.Information("WebApp attempting to access uploads at: {Path}", physicalPath);

    try 
    {
        if (!Directory.Exists(physicalPath))
        {
            Log.Information("Creating directory: {Path}", physicalPath);
            Directory.CreateDirectory(physicalPath);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(physicalPath),
            RequestPath = "/uploads"
        });
        
        Log.Information("WebApp static files for /uploads configured.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to mount /uploads in WebApp");
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "WebApp host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}