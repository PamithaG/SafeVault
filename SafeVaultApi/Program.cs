using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SafeVaultApi.Repositores;
using SafeVaultApi.Services;
using SafeVaultApi.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Adds the Swagger generator

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();

Console.WriteLine("_--------------------- SafeVaultApi ---------------------_");
Console.WriteLine("!!! Starting SafeVaultApi");


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);

Console.WriteLine($"::::::::JWT Settings Key: {Encoding.UTF8.GetBytes(jwtSettings.Key)}");
Console.WriteLine("_--------------------- --------------------- ---------------------_");


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAnyOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

