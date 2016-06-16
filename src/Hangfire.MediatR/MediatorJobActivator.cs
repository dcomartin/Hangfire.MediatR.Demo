using System;
using System.Data.SqlClient;
using MediatR;

namespace Hangfire.MediatR
{
    public class MediatRJobActivator : JobActivator
    {
        private readonly IMediator _mediator;
        private readonly HangfireExtensions.DatabaseConnectionFactory _dbFactory;

        public MediatRJobActivator(IMediator mediator, HangfireExtensions.DatabaseConnectionFactory dbFactory)
        {
            _mediator = mediator;
            _dbFactory = dbFactory;
        }

        public override object ActivateJob(Type type)
        {
            return new HangfireMediator(_mediator, _dbFactory);
        }
    }
}
