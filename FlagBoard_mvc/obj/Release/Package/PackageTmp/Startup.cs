using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FlagBoard_mvc.Startup))]
namespace FlagBoard_mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
