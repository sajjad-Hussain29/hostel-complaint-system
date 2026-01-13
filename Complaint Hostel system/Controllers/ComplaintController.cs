using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Complaint_Hostel_system.Models;
using Complaint_Hostel_system.DAL;

public class ComplaintController : Controller
{
    // GET: Complaint/Create
    [HttpGet]
    public ActionResult Create()
    {
        // check if user is logged in
        if (Session["UserId"] == null)
        {
            return RedirectToAction("Login", "Account");
        }
        ViewBag.ShowHeader = true;
        ViewBag.Active = "Create";
        return View();
    }

    // POST: Complaint/Create
    [HttpPost]
    public ActionResult Create(Complaint complaint)
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // attach logged-in user's id
        complaint.UserId = (int)Session["UserId"];
        complaint.CreatedAt = DateTime.Now;
        complaint.Status = "Pending";

        DbManager db = new DbManager();
        bool isSaved = db.AddComplaint(complaint);

        if (isSaved)
        {
            ViewBag.Message = "Complaint submitted successfully!";
            return RedirectToAction("MyComplaints");
        }
        else
        {
            ViewBag.Message = "Something went wrong. Try again!";
        }

        return View();
    }

    // GET: Complaint/MyComplaints (to see logged in user's complaints)
    public ActionResult MyComplaints()
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserId"];
        DbManager db = new DbManager();
        var complaints = db.GetComplaintsByUser(userId);

        ViewBag.ShowHeader = true;
        ViewBag.Active = "MyComplaints";
        return View(complaints);
    }

    [HttpPost]
    public ActionResult DeleteComplaint(int complaintId) 
    {
        int userId = Convert.ToInt32(Session["UserId"]);

        DbManager db = new DbManager();
        db.DeleteComplaint(complaintId, userId);

        TempData["Message"] = "Complaint deleted successfully!";
        return RedirectToAction("MyComplaints");
    }


    // GET: Complaint/EditComplaint/5
    public ActionResult EditComplaint(int id)
    {
        DbManager db = new DbManager();
        var complaint = db.GetComplaintForEdit(id);
        if (complaint == null)
        {
            return HttpNotFound();
        }
        ViewBag.ShowHeader = true;
        return View("Edit" , complaint);  // opens Edit.cshtml with data
    }

    // POST: Complaint/EditComplaint/5
    [HttpPost]
    public ActionResult EditComplaint(Complaint model)
    {
        if (ModelState.IsValid)
        {
            DbManager db = new DbManager();
            bool updated = db.EditComplaintDetails(model.ComplaintId, model.Title, model.Description);
            if (updated)
            {
                return RedirectToAction("MyComplaints");
            }
            else
            {
                ViewBag.Message = "Error updating complaint.";
            }
        }
        return View(model);
    }
}
