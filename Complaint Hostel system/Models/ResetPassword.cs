using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Complaint_Hostel_system.Models
{
    public class ResetPassword
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

             [Required, DataType(DataType.Password)]
             public string NewPassword { get; set; }

        [Required, DataType(DataType.Password)]
        [Compare("NewPassword" , ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}