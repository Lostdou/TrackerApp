using Npgsql;
using System.Data;
using TrackerApi.Data;
using TrackerAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- CAMBIO: INYECCIÓN DE POSTGRESQL ---
builder.Services.AddScoped<IDbConnection>(sp =>
{
    // En local usará appsettings, en Render usará la Variable de Entorno
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                           ?? builder.Configuration.GetConnectionString("DefaultConnection");

    return new NpgsqlConnection(connectionString);
});

var app = builder.Build();

// Inicializar la DB al arrancar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();
    DbInitializer.Initialize(db);
}

app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();

app.MapControllers();

app.Run();