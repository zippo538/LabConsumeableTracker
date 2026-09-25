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
using LabConsumableExpiryTracker.Application.Common;
using LabConsumableExpiryTracker.Application.Configurations;
using LabConsumableExpiryTracker.Application.Interfaces;
using LabConsumableExpiryTracker.Application.Mappings;
using LabConsumableExpiryTracker.Application.Services;
using LabConsumableExpiryTracker.Application.Services.Interfaces;
using LabConsumableExpiryTracker.Application.Validators.AuditValidator;
using LabConsumableExpiryTracker.Application.Validators.DashboardValidator;
using LabConsumableExpiryTracker.Application.Validators.DisposalValidator;
using LabConsumableExpiryTracker.Application.Validators.ItemValidator;
using LabConsumableExpiryTracker.Application.Validators.JobValidator;
using LabConsumableExpiryTracker.Application.Validators.Login;
using LabConsumableExpiryTracker.Application.Validators.LotValidator;
using LabConsumableExpiryTracker.Application.Validators.User;
using LabConsumableExpiryTracker.Application.DTOs.DashboardDTOs;
using LabConsumableExpiryTracker.Application.DTOs.DisposalDTOs;
using LabConsumableExpiryTracker.Application.DTOs.ItemDTOs;
using LabConsumableExpiryTracker.Application.DTOs.JobDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LoginDTOs;
using LabConsumableExpiryTracker.Application.DTOs.LotDTOs;
using LabConsumableExpiryTracker.Application.DTOs.PagedDTOs;
using LabConsumableExpiryTracker.Application.DTOs.UserDTOs;
using LabConsumableExpiryTracker.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Serilog;
using LabConsumableExpiryTracker.Application.Common.ErrorHandling;
using LabConsumableExpiryTracker.Application.DTOs.AuditDTOs;
using LabConsumableExpiryTracker.Infrastructure.Persistence.Seeders;
using HttpContextCurrentUserContext = LabConsumableExpiryTracker.Application.Services.HttpContextCurrentUserContext;
using ICurrentUserContext = LabConsumableExpiryTracker.Application.Services.Interfaces.ICurrentUserContext;
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
builder.Services.AddScoped<IDashboardSummaryService, DashboardSummaryService>();
builder.Services.AddScoped<IConsumptionHistoryService, ConsumptionHistoryService>();
builder.Services.AddScoped<IConsumptionHistoryRepository, ConsumptionHistoryRepository>();
builder.Services.AddScoped<IImageStorageRepository, ImageStorageRepository>();
builder.Services.AddScoped<IImageService, ImageService>();
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
builder.Services.AddTransient<IValidator<LoginRequestDto>, LoginRequestValidator>();
builder.Services.AddTransient<IValidator<RefreshTokenRequestDto>, RefreshTokenRequestValidator>();
builder.Services.AddTransient<IValidator<CreateItemDto>, CreateItemValidator>();
builder.Services.AddTransient<IValidator<UpdateItemDto>, UpdateItemValidator>();
builder.Services.AddTransient<IValidator<CreateLotDto>, CreateLotValidator>();
builder.Services.AddTransient<IValidator<UpdateLotDto>, UpdateLotValidator>();
builder.Services.AddTransient<IValidator<CreateJobDto>, CreateJobValidator>();
builder.Services.AddTransient<IValidator<UpdateJobDto>, UpdateJobValidator>();
builder.Services.AddTransient<IValidator<CreateDisposalDto>, CreateDisposalValidator>();
builder.Services.AddTransient<IValidator<CreateUserRequestDto>, CreateUserRequestValidator>();
builder.Services.AddTransient<IValidator<UpdateUserRequestDto>, UpdateUserRequestValidator>();
builder.Services.AddTransient<IValidator<UpdateFullNameRequestDto>, UpdateFullNameRequestValidator>();
builder.Services.AddTransient<IValidator<AuditQuery>, AuditQueryValidator>();
builder.Services.AddTransient<IValidator<DashboardAlertQuery>, DashboardAlertQueryValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserMappingProfile>();
    cfg.AddProfile<ItemMappingProfile>();
    cfg.AddProfile<LotMappingProfile>();
    cfg.AddProfile<JobMappingProfile>();
    cfg.AddProfile<DisposalMappingProfile>();
    cfg.AddProfile<LoginMappingProfile>();
});
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
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
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