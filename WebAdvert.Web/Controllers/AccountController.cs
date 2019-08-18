using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models;

namespace WebAdvert.Web.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<CognitoUser> signInManager;
		private readonly UserManager<CognitoUser> userManager;
		private readonly CognitoUserPool pool;
		
		public AccountController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.pool = pool;
		}

		public async Task<IActionResult> Signup()
		{
			var model = new SignupModel();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Signup(SignupModel model)
		{
			if (ModelState.IsValid)
			{
				var user = this.pool.GetUser(model.Email);
				if (user.Status != null)
				{
					ModelState.AddModelError("UserExists", "User with this email address already exists");
					return View(model);
				}

				user.Attributes.Add(CognitoAttributesConstants.Name, model.Email);
				var createdUser = await this.userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
				if (createdUser.Succeeded)
				{
					RedirectToAction("Confirm");
				}
			}


			return View();
		}

		public async Task<IActionResult> Confirm()
		{
			var model = new ConfirmModel();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Confirm(ConfirmModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await this.userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);				
				if (user == null)
				{
					ModelState.AddModelError("NotFound", "A user with the given email address was not found");
					return View(model);
				}

				var result = await (userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, model.Code, true).ConfigureAwait(false);

				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					foreach (var item in result.Errors)
					{
						ModelState.AddModelError(item.Code, item.Description);
					}
					return View(model);
				}
			}

			return View(model);
		}

		public async Task<IActionResult> Login(LoginModel model)
		{
			return View(model);
		}

		[HttpPost]
		[ActionName("Login")]
		public async Task<IActionResult> LoginPost(LoginModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await this.signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false).ConfigureAwait(false);
				if (result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError("LoginError", "Email and Password do not match");
				}
			}

			return View("Login", model);
		}
	}
}