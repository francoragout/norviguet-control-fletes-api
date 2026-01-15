using norviguet_control_fletes_api.Extensions;
using norviguet_control_fletes_api.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddJsonConfiguration();
builder.Services.AddOpenApi();
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddCorsConfiguration();

// Configure the HTTP request pipeline
var app = builder.Build();

app.ApplyMigrations();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwaggerConfiguration(app.Environment);
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
