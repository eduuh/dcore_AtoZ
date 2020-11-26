using System;
using System.Collections.Generic;
using System.Linq;
using Domain;

namespace Persistence
{
    public class SeedData
    {
        public static void SeedActivities(DataContext context){
            if(!context.Activities.Any()){
                var activities = new List<Activity>{
                  new Activity {
                     Title = "Future activity 1" ,
                     Date = DateTime.Now.AddMonths(5),
                     Description = "Activity Number 1",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },
                  new Activity {
                     Title = "Future activity 2" ,
                     Date = DateTime.Now.AddMonths(2),
                     Description = "Activity Number 2",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Freedom hall"
            },     new Activity {
                     Title = "Future activity 3" ,
                     Date = DateTime.Now.AddMonths(3),
                     Description = "Activity Number 3",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },     new Activity {
                     Title = "Future activity 4" ,
                     Date = DateTime.Now.AddMonths(4),
                     Description = "Activity Number 4",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },     new Activity {
                     Title = "Future activity 5" ,
                     Date = DateTime.Now.AddMonths(5),
                     Description = "Activity Number 5",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },     new Activity {
                     Title = "Future activity 6" ,
                     Date = DateTime.Now.AddMonths(6),
                     Description = "Activity Number 6",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },     new Activity {
                     Title = "Future activity 7" ,
                     Date = DateTime.Now.AddMonths(7),
                     Description = "Activity Number 7",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },     new Activity {
                     Title = "Future activity 8" ,
                     Date = DateTime.Now.AddMonths(8),
                     Description = "Activity Number 8",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },     new Activity {
                     Title = "Future activity 9" ,
                     Date = DateTime.Now.AddMonths(9),
                     Description = "Activity Number 9",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },     new Activity {
                     Title = "Future activity 9" ,
                     Date = DateTime.Now.AddMonths(9),
                     Description = "Activity Number 9",
                     Category = "Drinks",
                     City = "Nyeri",
                     Venue = "Just another pub"
            },
                };
                context.Activities.AddRange(activities);
                context.SaveChangesAsync();
            }
        }
        
    }
}