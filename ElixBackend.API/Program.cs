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
            Log.Information("Starting web host ({Environment})...", environment);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory,
                EnvironmentName = environment
            });

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddConfiguration(configuration);

            builder.Logging.ClearProviders();
            builder.Host.UseSerilog(Log.Logger);

            // --- CONFIGURATION CORS ---
            // On récupère l'URL du front depuis l'environnement (défini dans docker-compose)
            var frontEndUrl = builder.Configuration["FRONTEND_URL"] ?? "https://app.elix.cleanascode.fr";
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("WebAppPolicy", policy =>
                {
                    policy.WithOrigins(frontEndUrl)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();

            builder.Services.AddDbContext<ElixDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"];
            if (jwtSecretKey != null)
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

            // Repositories & Services
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

            // Automatic Migrations
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ElixDbContext>();
                    Log.Information("Checking for pending migrations...");
                    await context.Database.MigrateAsync();
                    Log.Information("Database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while migrating the database.");
                }
            }

            if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Local" || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Static Files
            try
            {
                var uploadsPath = builder.Configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
                var uploadsFolder = Path.IsPathRooted(uploadsPath) ? uploadsPath : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uploadsFolder),
                    RequestPath = "/uploads"
                });
            }
            catch (Exception ex) { Log.Warning(ex, "Could not configure static file serving"); }

            app.UseRouting();

            // --- APPLICATION DU CORS ---
            // Doit être placé APRES UseRouting et AVANT UseAuthentication/Authorization
            app.UseCors("WebAppPolicy");

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