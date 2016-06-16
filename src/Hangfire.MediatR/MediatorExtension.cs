using System;
using System.Data;
using System.Data.SqlClient;
using Hangfire;
using Hangfire.MediatR;
using Newtonsoft.Json;

namespace MediatR
{
    public static class MediatRExtension
    {
        public static void Enqueue(this IMediator mediator, IRequest request)
        {
            BackgroundJob.Enqueue<HangfireMediator>(m => m.SendCommand(request));
        }

        public static void Enqueue(this IMediator mediator, Guid id, IRequest request)
        {
            using (var db = HangfireExtensions.GetDatabaseConnection())
            {
                using (var sqlCmd = db.CreateCommand())
                {
                    sqlCmd.CommandText = "INSERT INTO [MediatR.Request] VALUES (@id, @body)";
                    sqlCmd.Parameters.Add(new SqlParameter("id", SqlDbType.UniqueIdentifier) { Value = id });
                    sqlCmd.Parameters.Add(new SqlParameter("body", SqlDbType.Text)
                    {
                        Value = JsonConvert.SerializeObject(request, new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        })
                    });
                    sqlCmd.ExecuteNonQuery();
                }
                
                BackgroundJob.Enqueue<HangfireMediator>(m => m.Process(id));
            }
        }
    }

    public class HangfireMediator
    {
        private readonly IMediator _mediator;
        private readonly HangfireExtensions.DatabaseConnectionFactory _dbFactory;

        public HangfireMediator(IMediator mediator, HangfireExtensions.DatabaseConnectionFactory dbFactory)
        {
            _mediator = mediator;
            _dbFactory = dbFactory;
        }

        public void SendCommand(IRequest request)
        {
            _mediator.Send(request);
        }

        public void Process(Guid id)
        {
            using (var db = _dbFactory())
            {
                using (var sqlCmd = db.CreateCommand())
                {
                    sqlCmd.CommandText = "SELECT body FROM [MediatR.Request] WHERE Id=@id";
                    sqlCmd.Parameters.Add(new SqlParameter("id", SqlDbType.UniqueIdentifier) { Value = id });

                    var data = sqlCmd.ExecuteScalar();
                    if (data == null) throw new InvalidOperationException($"Request (id) not found.");

                    var request = JsonConvert.DeserializeObject<IRequest>(data.ToString(), new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });

                    _mediator.Send(request);
                }
            }
        }
    }
}
