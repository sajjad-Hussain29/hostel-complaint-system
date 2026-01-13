using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Complaint_Hostel_system.Models
{
    public class Complaint
    {
      
            public int UserId { get; set; }
            public int ComplaintId { get; set; }
            public string Title { get; set; }
            public string Email { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Status { get; set; }
        
    }

}
