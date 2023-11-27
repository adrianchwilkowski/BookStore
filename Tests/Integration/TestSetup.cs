﻿using Infrastructure.Migrations;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tests.Integration
{
    [SetUpFixture]
    public class TestSetup : IntegrationTestBase, IDisposable
    {
        public TestSetup()
        {
            DestroyDatabase();
            CreateDatabase();
        }

        public void Dispose()
        {
            DestroyDatabase();
        }

        private static void CreateDatabase()
        {
            ExecuteSqlCommand(Master, $@"
                CREATE DATABASE [test]
                ON (NAME = 'test',
                FILENAME = '{Filename}')");

            var context = TestDbContext();
            context.Database.Migrate();
        }

        private static void DestroyDatabase()
        {
            var fileNames = ExecuteSqlQuery(Master, @"
                SELECT [physical_name] FROM [sys].[master_files]
                WHERE [database_id] = DB_ID('test')",
                row => (string)row["physical_name"]);

            if (fileNames.Any())
            {
                ExecuteSqlCommand(Master, @"
                    ALTER DATABASE [test] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    EXEC sp_detach_db 'test'");

                fileNames.ForEach(File.Delete);
            }
        }

        private static void ExecuteSqlCommand(
            SqlConnectionStringBuilder connectionStringBuilder,
            string commandText)
        {
            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }
        }

        private static List<T> ExecuteSqlQuery<T>(
            SqlConnectionStringBuilder connectionStringBuilder,
            string queryText,
            Func<SqlDataReader, T> read)
        {
            var result = new List<T>();
            using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryText;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(read(reader));
                        }
                    }
                }
            }
            return result;
        }

        private static SqlConnectionStringBuilder Master =>
            new SqlConnectionStringBuilder
            {
                DataSource = @"(LocalDB)\MSSQLLocalDB",
                InitialCatalog = "master",
                IntegratedSecurity = true
            };

        private static string Filename => Path.Combine(
            Path.GetDirectoryName(
                typeof(TestSetup).GetTypeInfo().Assembly.Location),
            "test.mdf");
    }
}
