﻿using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using VTP2015.Config;
using VTP2015.Identity;
using VTP2015.Modules.Authentication.ViewModels;
using VTP2015.ServiceLayer.Authentication;

namespace VTP2015.Modules.Authentication
{
    [Authorize]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationFacade _authenticationFacade;
        private UserManager<ApplicationUser> _userManager;

        public AuthenticationController(IAuthenticationFacade authenticationFacade)
        {
            _authenticationFacade = authenticationFacade;
            _userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        }

        //
        // GET: /
        // GET: /Login
        [HttpGet]
        [AllowAnonymous]
        [Route("")]
        [Route("Login")]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Route("")]
        [Route("Login")]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);        
            var user = await _userManager.FindAsync(model.Email, model.Password);
            if (user == null)
            {
                if (!_authenticationFacade.AuthenticateUserByEmail(model.Email, model.Password))
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
                user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded){
                    AddErrors(result);
                    return View(model);
                }
                if (GetRole(model.Email) == "Student")
                {
                    _authenticationFacade.SyncStudentByUser(model.Email, new ConfigFile().AcademieJaar());
                    var name = model.Email.Split('@')[0];
                    var path = Server.MapPath("/bewijzen/" + name);
                    Directory.CreateDirectory(path);
                }

                _userManager.AddToRole(user.Id, GetRole(model.Email));
                if (GetRole(model.Email).Equals("Counselor"))
                {
                    _userManager.AddToRole(user.Id, "Lecturer");
                }
            }
            await SignInAsync(user, model.RememberMe);
            return RedirectToRoute(new { controller = GetRole(model.Email), action = "Index" });
        }

        //
        // POST: /LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LogOff")]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        //
        // Post: /Home
        // Get: /Home
        [Route("Home")]
        public ActionResult Home()
        {
            return RedirectToRoute(new { controller = GetRole(User.Identity.Name), action = "Index"});
        }
       
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers

        //TODO: Uitleg
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(_userManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private string GetRole(string email){
            if (email.Contains("@howest.be"))
            {
                return _authenticationFacade.IsCounselor(email) ? "Counselor" : "Lecturer";
            }
            return email.Contains("@student.howest.be") ? "Student" : "Authentication";
        }
       
        #endregion
    }
}