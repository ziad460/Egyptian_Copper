using CopperFactory.Interfaces;
using CopperFactory.Models;
using CopperFactory.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web.Helpers;

namespace CopperFactory.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IUnityOfWork unityOfWork;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(IConfiguration configuration, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager,
       SignInManager<ApplicationUser> signInManager , IUnityOfWork unityOfWork)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.unityOfWork = unityOfWork;
            this.roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        //[Authorize(Roles ="Admin,PowerUser")]
        public IActionResult Register(string ReturnUrl = null)
        {
            ViewBag.URL = ReturnUrl;
            int usersCount = userManager.Users.Count();
            if (usersCount < 300)
            {
                ViewData["roles"] = new SelectList(roleManager.Roles, "Name", "Name");
                ViewData["factories"] = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "English_Name");
                //   var roles = roleManager.Roles;
                return View();
            }
            else
            {
                return View("MaxUsers");
            }
        }

        //[Authorize(Roles ="Admin,PowerUser")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to IdentityUser
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    SeconedName = model.SeconedName,
                    FactoryID = model.FactoryID,
                };

                // Store user data in AspNetUsers database table
                var result = await userManager.CreateAsync(user, model.Password);
                IdentityResult result2 = null;
                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    result2 = await userManager.AddToRoleAsync(user, model.RoleName);
                    //   await signInManager.SignInAsync(user, isPersistent: false);
                    // return RedirectToAction("index", "home");

                    if (result2.Succeeded)
                    {                      
                        return RedirectToAction("ListUsers", "Administration");
                    }


                    ////  await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index" , "Home");
                }

                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewData["factories"] = new SelectList(unityOfWork.Factory.FindAll(x => x.IsDeleted != true), "ID", "English_Name");
            ViewData["roles"] = new SelectList(roleManager.Roles, "Name", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string ReturnUrl)
        {
            ViewBag.URL = ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model , string ReturnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.URL = ReturnUrl;
                return View(model);
            }
            var result = await signInManager.PasswordSignInAsync(
                    model.UserName, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return LocalRedirect(ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            ViewBag.URL = ReturnUrl;
            return View(model);
        }
        [AllowAnonymous]
        public IActionResult ForgetPassword(string returnUrl)
        {
            ViewBag.URL = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword(string Email , string returnUrl)
        {
            var user = await userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Email = {Email} cannot be found";
                return View("NotFound");
            }
            
            return RedirectToAction("ResetPassword" , new { Id = user.Id , returnUrl = returnUrl});
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string Id ,string returnUrl)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with ID = {Id} cannot be found";
                return View("NotFound");
            }
            ViewBag.Id = user.Id;
            ViewBag.URL = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(PasswordViewModel model, string returnUrl)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with ID = {model.Id} cannot be found";
                return View("NotFound");
            }
            //Set new Password
            if (!String.IsNullOrEmpty(model.NewPassword))
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (result.Succeeded)
                {
                    //       await signInManager.RefreshSignInAsync(user);
                    //   await userManager.UpdateSecurityStampAsync(user);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    ViewBag.Id = user.Id;
                    ViewBag.URL = returnUrl;
                    return View(model);
                }
            }
            //   await userManager.UpdateSecurityStampAsync(user);
            //    await signInManager.RefreshSignInAsync(user);
            ViewBag.Id = user.Id;
            ViewBag.URL = returnUrl;
            return View();
        }
        public async Task<IActionResult> LogOut(string ReturnUrl)
        {
            await signInManager.SignOutAsync();
            return LocalRedirect(ReturnUrl);
        }

        public async Task<IActionResult> accessDenied(string ReturnUrl)
        {
            ModelState.AddModelError("", "Access denied to this route pleas login again");
            ViewBag.URL = ReturnUrl;
            return View("Login");
        }
    }
}