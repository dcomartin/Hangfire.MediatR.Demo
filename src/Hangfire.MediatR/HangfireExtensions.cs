using System.Data.SqlClient;
using Hangfire.Common;
using Hangfire.MediatR;
using MediatR;
using Newtonsoft.Json;

namespace Hangfire
{
    public static class HangfireExtensions
    {
        public delegate SqlConnection DatabaseConnectionFactory();

        public static IGlobalConfiguration UseMediatR(this IGlobalConfiguration config, IMediator mediator, DatabaseConnectionFactory getDbConnection)
        {
            config.UseActivator(new MediatRJobActivator(mediator, getDbConnection));
            
            JobHelper.SetSerializerSettings(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            });

            GetDatabaseConnection = getDbConnection;

            CreateMediatRTable();

            return config;
        }

        private static void CreateMediatRTable()
        {
            using (var connection = GetDatabaseConnection())
            {
                using (var sqlCmd = connection.CreateCommand())
                {
                    sqlCmd.CommandText =
                        "IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MediatR.Request]') AND type in (N'U')) BEGIN CREATE TABLE [dbo].[MediatR.Request](	[Id] [varbinary](50) NOT NULL, [Body] [text] NOT NULL, CONSTRAINT [PK_MediatR.Request] PRIMARY KEY CLUSTERED (	[Id] ASC ) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY] END";
                    sqlCmd.ExecuteNonQuery();
                }
            }
        }

        public static DatabaseConnectionFactory GetDatabaseConnection { get; set; }
    }
}
