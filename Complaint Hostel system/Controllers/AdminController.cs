using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Complaint_Hostel_system.DAL;
using Complaint_Hostel_system.Models;

public class AdminController : Controller
{
    private DbManager db = new DbManager();

    // Login Page
    public ActionResult AdminLogin()
    {
        ViewBag.HideSidebar = true;
        return View();
    }

    // Login POST
    [HttpPost]
    public ActionResult Login(string AdminEmail, string Admin_PasswordHash)
    {
        var admin = db.ValidateAdminLogin(AdminEmail, Admin_PasswordHash);

        if (admin != null)
        {
            Session["AdminId"] = admin.AdminId;
            Session["AdminName"] = admin.AdminName;
            return RedirectToAction("AllComplaints");
        }

        ViewBag.Error = "Invalid login credentials";
        return View("AdminLogin");
    }

    // Show all complaints
    public ActionResult AllComplaints()
    {
        if (Session["AdminId"] == null) return RedirectToAction("Login");

        var complaints = db.GetAllComplaints();
        ViewBag.ShowHeader = true;
        return View(complaints);
    }

    // Update complaint status
    [HttpPost]
    public ActionResult UpdateStatus(int complaintId, string status)
    {
       if (Session["AdminId"] == null) 
            return RedirectToAction("Login");
        System.Diagnostics.Debug.WriteLine($"DEBUG: Received complaintId = {complaintId}, status = '{status}'");

        try
        {
            db.UpdateComplaintStatus(complaintId, status);
            System.Diagnostics.Debug.WriteLine($"DEBUG: UpdateComplaintStatus called successfully");

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Error updating status - {ex.Message}");
        }
        return RedirectToAction("AllComplaints");
    }

    public ActionResult AdminProfile()
    {
        // Get current user ID from session
        if (Session["AdminId"] == null)
        {
            return RedirectToAction("AdminLogin", "Admin");
        }
        int adminId = Convert.ToInt32(Session["AdminId"]);

        var admin = db.GetAdminById(adminId);

        if (admin == null)
        {
            
            return HttpNotFound();
        }
       
        return View(admin);
    }


    public ActionResult Logout()
    {
        Session.Clear();
        return RedirectToAction("Index" , "Home");
    }
}
