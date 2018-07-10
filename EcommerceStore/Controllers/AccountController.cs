using EcommerceStore.Models.Data;
using EcommerceStore.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EcommerceStore.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/login
        [HttpGet]
        public ActionResult Login()
        {
            // Confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            // return view
            return View();
        }

        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if the user is valid
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }
            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        // GET: /account/create-account
        [HttpGet]
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [HttpPost]
        // POST: /account/create-account
        [ActionName("create-account")]
        public ActionResult CreateAccount(UserVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }
            // Check if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Make sure username is unique
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Username " + model.UserName + " is taken.");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }
                // Create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password
                };
                // Add the DTO
                db.Users.Add(userDTO);
                // Save
                db.SaveChanges();
                // Add to userRolesDTO
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();
            }
            // Create a TempData message
            TempData["SM"] = "You are now registered and can login.";

            // Redirect
            return Redirect("~/account/login");
        }

        // GET: /account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        public ActionResult UserNavPartial()
        {
            // Get username
            string username = User.Identity.Name;
            // Declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                // Get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);
                // Build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            // Return partial view with model
            return PartialView(model);
        }

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            // Get user name
            string username = User.Identity.Name;
            // Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                if (!db.Users.Any(x => x.UserName == username))
                {
                    FormsAuthentication.SignOut();
                    // Set TempData message
                    TempData["SM"] = "You have edited your username please login again!";
                    return Redirect("~/account/login");
                }
                // Get user
                 UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);
                // Build model
                 model = new UserProfileVM(dto);
            }
            // Return view with model
            return View("UserProfile", model);
        }

        // POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }
            // Check if passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match!");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Get username
                string username = User.Identity.Name;
                // Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == username))
                {
                    ModelState.AddModelError("", "Username " + model.UserName + " already exists.");
                    model.UserName = "";
                    return View("UserProfile", model);
                }
                // Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                // Save
                db.SaveChanges();
            }
            // Set TempData message
            TempData["SM"] = "You have edited your profile!";
            
            // Redirect
            return Redirect("~/account/user-profile");
        }
    }

}