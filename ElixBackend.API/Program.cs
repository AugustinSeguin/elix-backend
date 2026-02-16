using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.IRepository;
using ElixBackend.Infrastructure.Repository;
using System.Text;
using ElixAPI.Middlewares;
using ElixBackend.Business.IService;
using ElixBackend.Business.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.FileProviders;

namespace ElixBackend.API;

public static class Program
{
    private static async Task<int> Main(string[] args)
    {
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
            Log.Information("Starting web host ({Environment})...", environment);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = currentDirectory,
                EnvironmentName = environment
            });

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddConfiguration(configuration);
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog(Log.Logger);
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("WebAppPolicy", policy =>
                {
                    var allowedOrigins = new List<string> 
                    { 
                        "https://backoffice.elix.cleanascode.fr",
                        "https://app.elix.cleanascode.fr",
                        "http://localhost:5173",
                        "http://localhost:3000" 
                    };

                    var frontEndUrl = builder.Configuration["FRONTEND_URL"];
                    if (!string.IsNullOrEmpty(frontEndUrl))
                    {
                        allowedOrigins.Add(frontEndUrl.TrimEnd('/'));
                    }

                    policy.WithOrigins(allowedOrigins.Distinct().ToArray())
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition"); 
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            // ... (Le reste de vos injections de services : DbContext, Auth, Repositories reste inchangé)
            builder.Services.AddDbContext<ElixDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"];
            if (!string.IsNullOrEmpty(jwtSecretKey))
            {
                var key = Encoding.ASCII.GetBytes(jwtSecretKey);
                builder.Services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("JwtSettings:RequireHttpsMetadata");
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false
                        };
                    });
            }

            builder.Services.AddAuthorization();
            
            // Injections (UserRepository, UserService, etc.) - Gardez votre bloc ici
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
            builder.Services.AddScoped<IUserPointRepository, UserPointRepository>();
            builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IResourceService, ResourceService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IAnswerService, AnswerService>();
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<IUserAnswerService, UserAnswerService>();
            builder.Services.AddScoped<IUserPointService, UserPointService>();
            builder.Services.AddScoped<IQuizService, QuizService>();

            var app = builder.Build();

            // Migrations automatiques
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try {
                    var context = services.GetRequiredService<ElixDbContext>();
                    await context.Database.MigrateAsync();
                } catch (Exception ex) {
                    Log.Error(ex, "Migration error");
                }
            }

            if (!app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Serve les fichiers statiques de base (CSS, JS) de wwwroot
            app.UseStaticFiles();

            // Gestion des fichiers statiques
            var uploadsPath = app.Configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
            var uploadsFolder = Path.IsPathRooted(uploadsPath) ? uploadsPath : Path.Combine(app.Environment.ContentRootPath, uploadsPath);
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(uploadsFolder),
                RequestPath = "/uploads"
            });

            app.UseRouting();

            // --- PLACEMENT CRITIQUE ---
            app.UseCors("WebAppPolicy"); // Doit être ici

            app.UseAuthentication();
            app.UseAuthorization();

            if (!app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }
            
            app.UseMiddleware<JwtJtiValidationMiddleware>();
            app.MapControllers();

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}