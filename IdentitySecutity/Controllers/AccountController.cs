using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Mvc;
using IdentitySecutity.Models;
using System.Threading.Tasks;

namespace IdentitySecutity.Controllers
{
    public class AccountController : Controller
    {
        //Get instance of user manager for this request
        public UserManager<IdentityUser> UserManager => HttpContext.GetOwinContext().Get<UserManager<IdentityUser>>();



        public ActionResult Register() {
            return View();
        }
        [HttpPost]
        public  async Task<ActionResult> Register(RegisterModel model) {
            var identityUser = await UserManager.FindByNameAsync(model.UserName);

            if (identityUser != null) {
                return RedirectToAction("Index", "Home");
            }

            var user = new IdentityUser
            {
                UserName = model.UserName
            };
            var identityResult = await UserManager.CreateAsync(user,model.Password );

            if (identityResult.Succeeded) {
                return RedirectToAction("Index","Home");
                    }

            ModelState.AddModelError("",identityResult.Errors.FirstOrDefault());
            return View(model);
        }
    }
}