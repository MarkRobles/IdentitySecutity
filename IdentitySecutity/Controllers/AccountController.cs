﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Mvc;
using IdentitySecutity.Models;
using System.Threading.Tasks;
using Microsoft.Owin.Security;

namespace IdentitySecutity.Controllers
{
    public class AccountController : Controller
    {
        //Get instance of user manager for this request
        public UserManager<IdentityUser> UserManager => HttpContext.GetOwinContext().Get<UserManager<IdentityUser>>();
        public SignInManager<IdentityUser, string> SignInManager 
            => HttpContext.GetOwinContext().Get<SignInManager<IdentityUser, string>>();

        public  ActionResult ExternalAuthentication(string provider)
        {
            SignInManager.AuthenticationManager.Challenge(
                new AuthenticationProperties {
                    RedirectUri = Url.Action("ExternalCallback", new { provider })
                },provider);
            return new HttpUnauthorizedResult();

        }

        public async Task<ActionResult> ExternalCallback(string provider) {
      var loginInfo =      await SignInManager.AuthenticationManager.GetExternalLoginInfoAsync();
    var signInStatus =   await     SignInManager.ExternalSignInAsync(loginInfo,true);

            switch (signInStatus)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index","Home");
            
                default:
                     var user =await UserManager.FindByEmailAsync(loginInfo.Email);
                    if (user != null) {
                    var result =    await UserManager.AddLoginAsync(user.Id,loginInfo.Login);
                        if (result.Succeeded) {
                            return await ExternalCallback(provider);
                        }
                    }
                    return View("Error");
                    
            }
        }

        public ActionResult Register() {
            return View();
        }
        [HttpPost]
        public  async Task<ActionResult> Register(RegisterModel model) {
            

            var identityUser = await UserManager.FindByNameAsync(model.UserName);

            if (identityUser != null) {
                return RedirectToAction("Index", "Home");
            }

            var result = await UserManager.PasswordValidator.ValidateAsync(model.Password);

            if (!result.Succeeded) {
                ModelState.AddModelError("",result.Errors.FirstOrDefault());
                return View(model);
            }

            var user = new IdentityUser(model.UserName)
            {
               Email = model.UserName
            };
            
            var identityResult = await UserManager.CreateAsync(user,model.Password );


            if (identityResult.Succeeded) {
              var token =  await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
              var confirmUrl =  Url.Action("ConfirmEmail","Account", new {userid = user.Id, token = token },Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id,"Email confirmation",$"Use link to confirm email:{ confirmUrl}");
                return RedirectToAction("Index","Home");
                    }

            ModelState.AddModelError("",identityResult.Errors.FirstOrDefault());
            return View(model);
        }


        public ActionResult Login() {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model) {
            //Is persisten parameter 
            //false = lifetime session duration
            ///true = SetLifetime
            ///ShouldLockOut parameter
            ///true = increment lockout count if credentials are wrong
           var signInStatus = await  SignInManager.PasswordSignInAsync(model.UserName,model.Password,true,true);

            switch (signInStatus)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index","Home");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("ChooseProvider");

                default:
                    ModelState.AddModelError("","Invalid Credentials");
                    return View(model);
            }
        }

        public async Task<ActionResult> ChooseProvider()
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            var providers = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            return View(new ChooseProviderModel { Providers = providers.ToList() });
        }

        [HttpPost]
        public async Task<ActionResult> ChooseProvider(ChooseProviderModel model)
        {
            await SignInManager.SendTwoFactorCodeAsync(model.ChosenProvider);
            return RedirectToAction("TwoFactor", new { provider = model.ChosenProvider });
        }

        public ActionResult TwoFactor(string provider)
        {
            return View(new TwoFactorModel { Provider = provider });
        }

        [HttpPost]
        public async Task<ActionResult> TwoFactor(TwoFactorModel model)
        {
            var signInStatus = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, true, model.RememberBrowser);
            switch (signInStatus)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Index", "Home");
                default:
                    ModelState.AddModelError("", "Invalid Credentials");
                    return View(model);
            }
        }


        public  async Task<ActionResult> ConfirmEmail(string userid, string token) {
           var identityResult =  await UserManager.ConfirmEmailAsync(userid,token);
            if(!identityResult.Succeeded)
            {
                return View("Error");
            }
            return RedirectToAction("Index","Home");
        }


        public ActionResult ForgotPassword() {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordModel model) {

            var user = await UserManager.FindByNameAsync(model.UserName);

            if (user != null) { 
                var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var resetUrl =    Url.Action("PasswordReset","Account", new { userid = user.Id, token = token }, Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id,"Password Reset",$"Use link to reset password {resetUrl}");
            }

            return RedirectToAction("Index","Home");
        }


        public ActionResult PasswordReset(string userid, string token) {
            return View(new PasswordResetModel { UserId = userid, Token =  token });
        }

        [HttpPost]
        public async Task<ActionResult> PasswordReset(PasswordResetModel model)
        {
            var identityResult = await UserManager.ResetPasswordAsync(model.UserId,model.Token,model.Password);
            if (!identityResult.Succeeded)
            {
                return View("Error");
            }
            return RedirectToAction("Index", "Home");

        }

    }
}