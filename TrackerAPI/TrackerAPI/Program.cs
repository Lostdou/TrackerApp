using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                           ?? builder.Configuration.GetConnectionString("CadenaSQL");

    return new NpgsqlConnection(connectionString);
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();