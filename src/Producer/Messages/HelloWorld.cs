using System;
using System.Threading;
using MediatR;

namespace Demo.Producer.Messages
{
    public class HelloWorld : IRequest
    {
        public string Input { get; set; }
    }

    public class HelloWorldHandler : IRequestHandler<HelloWorld, Unit>
    {
        public Unit Handle(HelloWorld message)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + message.Input);
            return Unit.Value;
        }
    }

    public static class HangfireHelpers
    {
        public static IMediator Mediator { private get; set; }

        public static void Send(IRequest command)
        {
            Mediator.Send(command);
        }
    }
}
