using FluentValidation;
using LabConsumableExpiryTracker.Domain.Models;
using LabConsumableExpiryTracker.Infrastructure.Persistence;
using LabConsumableExpiryTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using LabConsumableExpiryTracker.Application.Configurations;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Mappings;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Application.Validators.Login;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Serilog;
using LabConsumableExpiryTracker.Mappings;
using LabConsumableExpiryTracker.Application.Common.ErrorHandling;
using LabConsumableExpiryTracker.Application.Validators.Helpers;
using LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders;
using LabConsumableExpiryTracker.Application.Common.Mappings;
using LabConsumableExpiryTracker.API.CurrentUser;
using LabConsumableExpiryTracker.Application.Common.CurrentUser;


var builder = WebApplication.CreateBuilder(args);

// --- GlobalExeptionHandler ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// --- DbContext ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Identity ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// --- JWT Settings ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

// --- Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
        )
    };
});

builder.Services.AddAuthorization();


// --- Application / Domain / Infrastructure Services ---
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ILotService, LotService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ILotSelectionService, LotSelectionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDisposalService, DisposalService>();
builder.Services.AddScoped<IDashboardAlertService, DashboardAlertService>();



builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ILotSelectionRepository, LotSelectionRepository>();
builder.Services.AddScoped<IDisposalRepository, DisposalRepository>();
builder.Services.AddScoped<IConsumptionRepository, ConsumptionRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IDashboardAlertRepository, DashboardAlertRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IDashboardSummaryService, DashboardSummaryService>();

builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IUserLookupService, UserLookupService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddScoped<UserSeeder>();
builder.Services.AddScoped<ICurrentUserContext, HttpContextCurrentUserContext>();


builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IdentitySeeder>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// --- HttpContext ---
builder.Services.AddHttpContextAccessor();
// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();


builder.Services.AddAutoMapper(
    _ => { },
    typeof(UserMappingProfile).Assembly,
    typeof(ItemMappingProfile).Assembly,
    typeof(LotMappingProfile).Assembly,
    typeof(JobMappingProfile).Assembly,
    typeof(DisposalMappingProfile).Assembly,
    typeof(UserMappingProfile).Assembly
    );

// --- Serilog ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


builder.Host.UseSerilog();

// --- policy CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // URL aplikasi React Anda
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Tambahkan ini jika nanti butuh cookies/session
    });
});


// --- JsonSerializer ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new SafeGuidConverter());
    });
// --- Controllers & Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Masukkan JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ==================================================
// Database Initialization / Seeder
// ==================================================

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.MapControllers();
app.UseExceptionHandler();
app.UseCors("ReactDevelopment");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider
        .GetRequiredService<IDbInitializer>();

    await dbInitializer.InitializeAsync();
}

app.Run();