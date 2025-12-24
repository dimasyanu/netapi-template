using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetApi.Abstractions;
using NetApi.Application.Auth;
using NetApi.Application.Common.Contracts;
using NetApi.Application.Roles;
using NetApi.Application.Users;
using NetApi.Domain.Repositories;
using NetApi.Infrastructure.Persistence.Models;
using NetApi.Infrastructure.Persistence.Repositories;
using NetApi.Infrastructure.Persistence.Services;
using NetApi.Middlewares;
using NetApi.Services;
using Quartz;
// using Quartz.Impl.AdoJobStore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<IHashingService, HashingService>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserSettingRepository, UserSettingRepository>();

builder.Services.AddScoped<IMailService, SmtpMailService>();
builder.Services.AddScoped<IEmailTemplateManager, EmailTemplateManager>();
builder.Services.AddScoped<IAuthService, JwtAuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJobService, QuartzJobService>();

const string schedulerName = "NetApiQuartzScheduler";
builder.Services.AddQuartz(opt => {
    opt.SchedulerId = schedulerName;
    opt.SchedulerName = schedulerName;
    opt.UseInMemoryStore();
    opt.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);
});
builder.Services.AddQuartzHostedService(opt => {
    opt.WaitForJobsToComplete = true;
});

var appSettings = builder.Configuration.Get<AppSettings>()
    ?? throw new InvalidConfigurationException("AppSettings section is missing or invalid");
builder.Services.AddTransient(_ => appSettings);

// builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("NetApiInMemoryDb"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ICommand>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidIssuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidConfigurationException("JWT Issuer is not defined"),
        ValidAudience = builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidConfigurationException("JWT Audience is not defined"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]
            ?? throw new InvalidConfigurationException("JWT Secret Key is not defined"))),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseErrorHandlerMiddleware();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
