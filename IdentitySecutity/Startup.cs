using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(IdentitySecutity.Startup))]

namespace IdentitySecutity
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Here you we register our dependencies

            const string connectionstring = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=IdentitySecurity;Integrated Security=SSPI;";
            app.CreatePerOwinContext(() => new IdentityDbContext(connectionstring));

            //Implement  user store
            app.CreatePerOwinContext<UserStore<IdentityUser>>((opt, cont) => new UserStore<IdentityUser>(cont.Get<IdentityDbContext>()));
            //Use UserStore to configure a UserManager
            app.CreatePerOwinContext<UserManager<IdentityUser>>
                ((opt,cont) => new UserManager<IdentityUser>(cont.Get<UserStore<IdentityUser>>()));


            //take credentials verify them
            app.CreatePerOwinContext<SignInManager<IdentityUser,string>>(
                (opt,cont) => 
                new SignInManager<IdentityUser, string>(cont.Get<UserManager<IdentityUser>>(),cont.Authentication));

            //Issue a cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });
        }
    }
}
