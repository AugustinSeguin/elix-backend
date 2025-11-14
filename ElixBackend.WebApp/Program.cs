using Microsoft.EntityFrameworkCore;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.IRepository;
using ElixBackend.Infrastructure.Repository;
using ElixBackend.Business.IService;
using ElixBackend.Business.Service;
using ElixBackend.WebApp.Services; 

var builder = WebApplication.CreateBuilder(args);

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

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(physicalPath),
        RequestPath = uploadsPath
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