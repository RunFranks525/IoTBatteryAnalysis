using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BatteryWebApp.Startup))]
namespace BatteryWebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
