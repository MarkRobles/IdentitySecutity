using System;
using System.Configuration;
using System.Threading.Tasks;
using IdentitySecutity.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
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
          
            app.CreatePerOwinContext<UserManager<IdentityUser>>
                ((opt,cont) =>
                {
                    //Use UserStore to configure a UserManager
                    var usermanager = new UserManager<IdentityUser>(cont.Get<UserStore<IdentityUser>>());
                    //Register new instance of PhoneNumberTokenProvider that get configured every time we request a new UserManager
                    usermanager.RegisterTwoFactorProvider("SMS", new PhoneNumberTokenProvider<IdentityUser>() { MessageFormat="Token:{0}"});        
                    usermanager.SmsService = new SmsService();
                    //Register user Provider
                    usermanager.UserTokenProvider = new DataProtectorTokenProvider<IdentityUser>(opt.DataProtectionProvider.Create());
                    usermanager.EmailService = new EmailService();
                //Configure user validator
                usermanager.UserValidator = new UserValidator<IdentityUser>(usermanager) { RequireUniqueEmail = true };
                    usermanager.PasswordValidator = new PasswordValidator
                    {
                        RequireDigit = true,
                        RequireLowercase = true,
                        RequireNonLetterOrDigit = true,
                        RequireUppercase = true,
                        RequiredLength = 8
                    };

                    usermanager.UserLockoutEnabledByDefault = true;
                    usermanager.MaxFailedAccessAttemptsBeforeLockout = 3;
                    usermanager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(3);
                        

            return usermanager;
                });


            //take credentials verify them
            app.CreatePerOwinContext<SignInManager<IdentityUser,string>>(
                (opt,cont) => 
                new SignInManager<IdentityUser, string>(cont.Get<UserManager<IdentityUser>>(),cont.Authentication));

            //Issue a cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Provider  =  new CookieAuthenticationProvider 
                { 
                OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<UserManager<IdentityUser>,IdentityUser>(
                    validateInterval:TimeSpan.FromSeconds(3),
                    regenerateIdentity:(manager,user) => manager.CreateIdentityAsync(user,DefaultAuthenticationTypes.ApplicationCookie))
                }
                
            });

            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            //if this external signin cookie is after the google authenticator middleware(below) you will get an error
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            //Configure google  authentication 
            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["google:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["google:ClientSecret"],
                Caption = "Google"
            }); ;


       

        }
    }
}
