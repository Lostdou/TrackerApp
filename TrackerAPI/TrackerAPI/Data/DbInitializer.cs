using Dapper;
using System.Data;

namespace TrackerAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IDbConnection connection)
        {
            // SINTAXIS POSTGRESQL
            var sqlCreate = @"
                CREATE TABLE IF NOT EXISTS UserLocations (
                    Id SERIAL PRIMARY KEY,            
                    DeviceId TEXT NOT NULL UNIQUE,     
                    PairingCode TEXT,
                    Name TEXT,
                    Latitude DOUBLE PRECISION,         
                    Longitude DOUBLE PRECISION,
                    LastUpdate TIMESTAMP                
                );";

            connection.Execute(sqlCreate);
        }
    }
}