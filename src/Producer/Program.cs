using System;
using System.Configuration;
using System.Data.SqlClient;
using Demo.Producer.Messages;
using Hangfire;
using Hangfire.MediatR;
using MediatR;
using Microsoft.Practices.Unity;

namespace Demo.Producer
{
    class Program
    {
        static void Main()
        {
            var container = new UnityContainer();
            container.RegisterType<IMediator>(new InjectionFactory(x => new Mediator(type => x.Resolve(type), type => x.ResolveAll(type))));
            container.RegisterType<IRequestHandler<HelloWorld, Unit>>(new InjectionFactory(x => new HelloWorldHandler()));
            container.RegisterType<SqlConnection>(new InjectionFactory(x =>
            {
                var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Hangfire"].ConnectionString);
                connection.Open();
                return connection;
            }));

            var mediator = container.Resolve<IMediator>();

            GlobalConfiguration.Configuration
                .UseColouredConsoleLogProvider()
                .UseMediatR(mediator, () => container.Resolve<SqlConnection>())
                .UseSqlServerStorage("Hangfire");

            while (true)
            {
                Console.WriteLine("Input: ");
                var input = Console.ReadLine();

                var commandNoId = new HelloWorld {Input = $"Enqueing with no Id: {input}"};
                mediator.Enqueue(commandNoId);

                var id = Guid.NewGuid();
                var commandWithId = new HelloWorld { Input = $"Enqueing with Id({id}): {input}"};
                mediator.Enqueue(id, commandWithId);
            }
        }
    }
}
