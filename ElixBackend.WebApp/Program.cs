using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ElixAPI.Middlewares;
using ElixBackend.Business.IService;
using ElixBackend.Business.Service;
using ElixBackend.Infrastructure;
using ElixBackend.Infrastructure.IRepository;
using ElixBackend.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// ───── 1. DATABASE ─────────────────────────────
builder.Services.AddDbContext<ElixDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ───── 2. RAZOR, MVC, VIEWS ─────────────────────
// Razor pages + views + validation/form support
builder.Services.AddControllersWithViews()
    .WithRazorPagesAtContentRoot();

builder.Services.AddRazorPages();
builder.Services.AddMvc(); // Nécessaire pour le binding de formulaire Razor

// ───── 3. AUTHENTICATION (JWT) ─────────────────
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
        options.RequireHttpsMetadata = false; // Active en prod
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                var path = context.HttpContext.Request.Path;
                if (!path.StartsWithSegments("/user/login") && !context.Response.HasStarted)
                {
                    context.HandleResponse();
                    context.Response.Redirect("/user/login");
                }
                return Task.CompletedTask;
            }
        };
    });
}

// ───── 4. AUTHORIZATION ────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));
});

// ───── 5. DEPENDENCY INJECTION ─────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ───── 6. BUILD APP ────────────────────────────
var app = builder.Build();

// ───── 7. MIDDLEWARE PIPELINE ──────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Middleware 
app.UseMiddleware<JwtJtiValidationMiddleware>();

// Routing MVC + Razor
app.MapControllers();
app.MapRazorPages();

await app.RunAsync();
