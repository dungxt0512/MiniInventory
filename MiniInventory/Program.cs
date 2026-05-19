using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MiniInventory.Data;
using MiniInventory.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("default")));

builder.Services.AddScoped<JwtHelpers, JwtHelpersImpl>();
builder.Services.AddScoped<PasswordHelpers, PasswordHelpersImpl>();
var app = builder.Build();
var jwtsettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtsettings["SecretKey"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtsettings["Issuer"],
            ValidAudience = jwtsettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
         policy.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure the HTTP request pipeline.
app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
