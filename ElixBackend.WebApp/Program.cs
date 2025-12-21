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
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
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

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        Args = args,
        EnvironmentName = environment,
        ContentRootPath = AppContext.BaseDirectory
    });

    // Remplace la configuration par celle chargée explicitement
    builder.Configuration.Sources.Clear();
    builder.Configuration.AddConfiguration(configuration);

    // Utilise Serilog comme provider de logs
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

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<TokenPropagationHandler>();
    builder.Services.AddHttpClient("ApiClient")
        .AddHttpMessageHandler<TokenPropagationHandler>();

    builder.Services.AddAuthentication("Cookies")
        .AddCookie("Cookies", options =>
        {
            options.LoginPath = "/User/Login";
            options.AccessDeniedPath = "/User/Login";
        });

    var app = builder.Build();

    if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local")
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    var uploadsPath = builder.Configuration["FileStorage:UploadsPath"] ?? string.Empty;

    if (!string.IsNullOrEmpty(uploadsPath))
    {
        var physicalPath = Path.IsPathRooted(uploadsPath)
            ? uploadsPath
            : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);

        if (!Directory.Exists(physicalPath))
        {
            Directory.CreateDirectory(physicalPath);
        }

        // Serve the same uploads folder under /uploads for the WebApp
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(physicalPath),
            RequestPath = "/uploads"
        });
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
catch (Exception ex)
{
    Log.Fatal(ex, "WebApp host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
