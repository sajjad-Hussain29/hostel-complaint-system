using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Complaint_Hostel_system.Models
{
    public class AdminModel
    {
        public int AdminId { get; set; }
        public string AdminName { get; set; }
        public string AdminEmail { get; set; }
        public string Admin_PasswordHash { get; set; }
    }
}