using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Complaint_Hostel_system.Models;
using Complaint_Hostel_system.DAL;
using System.Data.SqlClient;
using Complaint_Hostel_system.Helper;


namespace Complaint_Hostel_system.Controllers
{
    public class AccountController : Controller
    {
        DbManager db = new DbManager(); 
        // GET: /Account/Register
        public ActionResult Register() 
        {
            ViewBag.Active = "Register";
            ViewBag.ShowHeader = false;
            ViewBag.HideSidebar = true;
            return View(); 
        }
        // POST: /Account/Register
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        { 
            if (ModelState.IsValid)
            { 
                if (db.IsEmailTaken(model.Email))
                { ViewBag.Message = "Email already registered!";
                    return View(model); 
                }

                db.RegisterUser(model.Name, model.Email, model.Password, model.RoomNo);
                ViewBag.Message = "Registration successful!"; 
                return RedirectToAction("Login");
            } 
            return View(model);
        }
        // GET: /Account/Login
        public ActionResult Login()
        {
            ViewBag.Active = "Login";
            ViewBag.ShowHeader = false;
            ViewBag.HideSidebar = true;
            return View(); 
        }
        // POST: /Account/Login
         [HttpPost]
        public ActionResult Login(LoginModel model)
        { 
            if (ModelState.IsValid) 
            {
                var user= db.ValidateLogin(model.Email, model.Password); 
                if (user != null)
                { 
                    // store user info in session Session
                    Session["UserId"] = user.UserId;
                    Session["UserName"] = user.Name;
                    Session["UserEmail"] = user.Email;
                    return RedirectToAction("MyComplaints", "Complaint");
                    // redirect to homepage
                    }
                else
                { 
                    ViewBag.Message = "Invalid email or password."; 
                }
            } 
            return View(model); 
        }

        // GET: Logout
        public ActionResult Logout() 
        {
            Session.Clear();
            Session.Abandon();
            return  RedirectToAction("Index" , "Home");

        }

        public ActionResult Profile()
        {
            // Get current user ID from session
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
                int userId = Convert.ToInt32(Session["UserId"]);

            DbManager db = new DbManager();
            User user = db.GetUserById(userId);  // method you already wrote

            if(user == null)
            {
                ViewBag.Error = "User not found";
                return View("Error");
            }
            ViewBag.ShowHeader = true;
            return View("Profile", user);
        }

        [HttpPost]
        public ActionResult Profile(User model)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                DbManager db = new DbManager();
                db.UpdateUserProfile(model);  // method you already wrote

                // reload updated data and show profile again
                User updatedUser = db.GetUserById(model.UserId);
                ViewBag.Message = "Profile updated successfully!";
                return View("Profile", updatedUser);
            }

            return View("Profile", model);
        }

        #region FORGOT PASSWORD(GET & POST)
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var db = new DbManager())
            {
                var user = db.GetUserByEmail(model.Email);
                if (user == null)
                {
                    ViewBag.Error = "No account found with that email id.";
                    return View(model);
                }

                var token = TokenHelper.GenerateToken(32); // small helper just for token
                var expiry = DateTime.UtcNow.AddHours(1);

                db.SaveResetToken(model.Email, token, expiry);

                // build the link and send email
                var resetUrl = Url.Action("ResetPassword", "Account",
                                          new { token = token, email = model.Email },
                                          Request.Url.Scheme);

                EmailService.SendResetLink(model.Email, resetUrl);

                ViewBag.Message = "A resent link has been sent to the provided email id";
                ModelState.Clear();
                return View();
            }
        }

        #endregion

        #region RESET PASSWORD( GET & POST):
        [HttpGet]
        public ActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return View("InvalidOrExpiredToken");
            }

            var db = new DbManager();
            DateTime? expiry = db.GetResetTokenExpiry(email, token);

            if (expiry == null || expiry < DateTime.UtcNow)
            {
                return View("InvalidOrExpiredToken");
            }

            // token is valid → show the form
            return View("ResetPasswordForm", new ResetPassword { Email = email, Token = token });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View("ResetPasswordForm", model);
            }

            var db = new DbManager();
           
            // check token expiry in DB
            DateTime? expiry = db.GetResetTokenExpiry(model.Email, model.Token);
            if (expiry == null || expiry < DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Invalid or expired token.");
                return View(model);
            }

            // update password and clear token
            db.UpdatePassword(model.Email, model.NewPassword);

            TempData["Message"] = "Your password has been reset successfully.";
            return RedirectToAction("Login");
        }

        #endregion
    }
}