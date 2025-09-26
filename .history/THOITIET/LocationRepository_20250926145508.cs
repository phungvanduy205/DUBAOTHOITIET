using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace THOITIET
{
    public class LocationRepository
    {
        private readonly string _connectionString;

        public LocationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void EnsureCreated()
        {
            // Ensure database exists
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.InitialCatalog;
            var masterCs = new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "master" }.ConnectionString;

            using (var conn = new SqlConnection(masterCs))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"IF DB_ID(@db) IS NULL CREATE DATABASE [{databaseName}]";
                cmd.Parameters.AddWithValue("@db", databaseName);
                cmd.ExecuteNonQuery();
            }

            // Ensure table exists
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
IF OBJECT_ID('dbo.SavedLocations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SavedLocations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(256) NOT NULL,
        NormalizedName NVARCHAR(256) NOT NULL,
        Lat FLOAT NOT NULL,
        Lon FLOAT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_SavedLocations_NormalizedName ON dbo.SavedLocations(NormalizedName);
END";
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<SavedLocation> GetAll()
        {
            var result = new List<SavedLocation>();
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT Name, Lat, Lon FROM dbo.SavedLocations ORDER BY CreatedAt DESC";
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        var name = rd.GetString(0);
                        var lat = rd.GetDouble(1);
                        var lon = rd.GetDouble(2);
                        result.Add(new SavedLocation(name, lat, lon));
                    }
                }
            }
            return result;
        }

        public bool ExistsByNameOrNear(string normalizedName, double lat, double lon, double epsilon)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"
SELECT TOP 1 1
FROM dbo.SavedLocations
WHERE NormalizedName = @n
   OR (ABS(Lat - @lat) <= @eps AND ABS(Lon - @lon) <= @eps)";
                cmd.Parameters.AddWithValue("@n", normalizedName);
                cmd.Parameters.AddWithValue("@lat", lat);
                cmd.Parameters.AddWithValue("@lon", lon);
                cmd.Parameters.AddWithValue("@eps", epsilon);
                var val = cmd.ExecuteScalar();
                return val != null && val != DBNull.Value;
            }
        }

        public void Add(string name, string normalizedName, double lat, double lon)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "INSERT INTO dbo.SavedLocations(Name, NormalizedName, Lat, Lon) VALUES(@name, @n, @lat, @lon)";
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@n", normalizedName);
                cmd.Parameters.AddWithValue("@lat", lat);
                cmd.Parameters.AddWithValue("@lon", lon);
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteByName(string name)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "DELETE FROM dbo.SavedLocations WHERE Name = @name";
                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

