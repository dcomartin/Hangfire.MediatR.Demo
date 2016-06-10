using Hangfire;
using Owin;

namespace Demo.Consumer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard("/hangfire");
        }
    }
}
