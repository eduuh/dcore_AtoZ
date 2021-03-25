using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Domain
{
    public class UserActivity
    {
        public  string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public string ActivityId { get; set; }
        public virtual  Activity Activity { get; set; }
        
    }
}