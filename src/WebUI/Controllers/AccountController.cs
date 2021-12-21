﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OverTheBoard.Core.Security.Data;
using OverTheBoard.WebUI.Models;

namespace OverTheBoard.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<OverTheBoardUser> _signInManager;
        private readonly UserManager<OverTheBoardUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly SecurityDbContext _securityDbContext;

        public AccountController(
            SignInManager<OverTheBoardUser> signInManager,
            UserManager<OverTheBoardUser> userManager,
            ILogger<AccountController> logger,
            SecurityDbContext securityDbContext
            )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _securityDbContext = securityDbContext;
        }
        [HttpGet]
        public IActionResult Login()
        {
            // Checking if user is logged on and redirecting to Dashboard
            if (_signInManager.IsSignedIn(User))
            {
                return LocalRedirect("~/Dashboard");
            }
            return View();
        }
        
        
        

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            returnUrl ??= Url.Content("~/Dashboard/");
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.EmailAddress) ?? await _userManager.FindByNameAsync(model.EmailAddress);
                if (user != null)
                {
                    var checkResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
                    if (checkResult.Succeeded)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, lockoutOnFailure: true);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation(1, "User logged in.");
                            return LocalRedirect(returnUrl);
                        }
                    }
                }

                ModelState.AddModelError("Password", "Invalid login attempt.");
            }

            return View(model);
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            // Checking if user is logged on and redirecting to Dashboard
            if (_signInManager.IsSignedIn(User))
            {
                return LocalRedirect("~/Dashboard");
            }
            return View(new RegistrationViewModel());
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.EmailAddress) ?? await _userManager.FindByNameAsync(model.EmailAddress);

                if (user != null)
                {
                    ModelState.AddModelError(string.Empty, "This email account is already registered.");
                }
                else
                {
                    // Defines a new user
                    var userId = Guid.NewGuid().ToString();
                    var uniqueDisplayNameAndId = await GenerateUniqueDisplayName(model.DisplayName);
                    user = new OverTheBoardUser()
                    {
                        Id = userId,
                        DisplayName = uniqueDisplayNameAndId.Item1,
                        DisplayNameId = uniqueDisplayNameAndId.Item2,
                        UserName = model.EmailAddress,
                        Email = model.EmailAddress,
                        DisplayImagePath = "defaultDisplayPic.jpeg",
                        LockoutEnabled = true,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Success");
                    }
                    else
                    {
                        foreach (IdentityError error in result.Errors)
                        {
                            var safeErrorCode = error.Code ?? "";
                            ModelState.AddModelError(safeErrorCode, error.Description);
                        }
                    }
                }
            }
            return View("Register", model);
        }

        private async Task<Tuple<string, string>> GenerateUniqueDisplayName(string displayName, string number=null)
        {
            Random random = new Random();
            if (string.IsNullOrEmpty(number))
            {
                number = random.Next(0, 9999).ToString().PadLeft(4, '0');
            }
            var user = await _securityDbContext.Users.FirstOrDefaultAsync(e => e.DisplayName == displayName && e.DisplayNameId == number);

            while ( user != null)
            {
                number = random.Next(0, 9999).ToString().PadLeft(4, '0');
                user = await _securityDbContext.Users.FirstOrDefaultAsync(e => e.DisplayName == displayName && e.DisplayNameId == number);
            }
            return Tuple.Create(displayName, number);
        }

        [AllowAnonymous]
        public IActionResult Success()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}