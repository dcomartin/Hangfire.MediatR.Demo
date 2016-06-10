using System;
using Demo.Producer.Messages;
using Hangfire;
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
            
            var mediator = container.Resolve<IMediator>();

            GlobalConfiguration.Configuration
                .UseColouredConsoleLogProvider()
                .UseMediatR(mediator)
                .UseSqlServerStorage("Hangfire");

            while (true)
            {
                Console.WriteLine("Input: ");
                var input = Console.ReadLine();

                var command = new HelloWorld {Input = input};
                mediator.Enqueue(command);
            }
        }
    }
}
