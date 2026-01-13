using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Complaint_Hostel_system.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage ="Email is required!")]
        [EmailAddress(ErrorMessage ="Invalid Email Address!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}