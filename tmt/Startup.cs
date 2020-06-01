using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(tmt.Startup))]
namespace tmt
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
