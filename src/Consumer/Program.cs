using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using Demo.Producer.Messages;
using Hangfire;
using Hangfire.MediatR;
using MediatR;
using Microsoft.Owin.Hosting;
using Microsoft.Practices.Unity;

namespace Demo.Consumer
{
    class Program
    {
        static void Main()
        {
            var container = new UnityContainer();
            container.RegisterType<IMediator>(
                new InjectionFactory(x => new Mediator(type => x.Resolve(type), type => x.ResolveAll(type))));
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

            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                var hangfire = new BackgroundJobServer();
                Console.WriteLine($"Hangfire Server started on Thread {Thread.CurrentThread.ManagedThreadId}.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
