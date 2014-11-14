using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DatabaseConn.Startup))]
namespace DatabaseConn
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
