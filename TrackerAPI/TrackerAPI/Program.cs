using Microsoft.Data.SqlClient;
using System.Data;
using TrackerAPI.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                           ?? builder.Configuration.GetConnectionString("CadenaSQL");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("No se encontró la cadena de conexión a la base de datos.");

    return new SqlConnection(connectionString);
});

builder.Services.AddHttpClient<ITmdbService, TmdbService>(client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();