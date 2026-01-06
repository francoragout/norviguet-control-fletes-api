using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using norviguet_control_fletes_api.Data;
using norviguet_control_fletes_api.Repositories;
using norviguet_control_fletes_api.Services;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Use the same connection string key as in appsettings.json ("norviguetDB")
builder.Services.AddDbContext<NorviguetDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("norviguetDB")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICarrierService, CarrierService>();

// Registro de BlobServiceClient
builder.Services.AddSingleton(x =>
    new BlobServiceClient(builder.Configuration["AzureStorage:ConnectionString"]));
// Si necesitas el contenedor directamente:
builder.Services.AddSingleton(x =>
    new BlobContainerClient(
        builder.Configuration["AzureStorage:ConnectionString"],
        builder.Configuration["AzureStorage:ContainerName"]));
// Registro de tu servicio de blobs
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<ICarrierRepository, CarrierRepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:7117",
                "https://thankful-ocean-0a8a9e40f.1.azurestaticapps.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Permite el envío de cookies
});

// Prueba de conexión a la base de datos
try
{
    using (var connection = new SqlConnection(builder.Configuration.GetConnectionString("norviguetDB")))
    {
        connection.Open();
        Console.WriteLine("Conexión exitosa a la base de datos.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error al conectar a la base de datos: {ex.Message}");
}

// Configure the HTTP request pipeline.
var app = builder.Build();

// Apply any pending migrations at startup to ensure tables exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<NorviguetDbContext>();
        db.Database.Migrate();
        Console.WriteLine("Database migrated/applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error applying migrations: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
