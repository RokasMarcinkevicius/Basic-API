using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tamro.Models;
namespace Tamro.Persistence
{
    public class Seed
    {
        public static async Task SeedData(RosterDbContext context)
        {
            if (context.Users.Any()) return;

            var activities = new List<User>
            {
                new User
                {
                    Name = "Bojack",
                    Surname = "Horseman",
                    Phone = "Private Number",
                    Email = "horse@gmail.com",
                },
                new User
                {
                    Name = "Sarah",
                    Surname = "Lynn",
                    Phone = "+37012345678",
                    Email = "rip@gmail.com",
                },
                new User
                {
                    Name = "Diane",
                    Surname = "Nguyne",
                    Phone = "+37012345678",
                    Email = "human@gmail.com",
                },
                new User
                {
                    Name = "Todd",
                    Surname = "Chavez",
                    Phone = "Private Number",
                    Email = "difficult@gmail.com",
                },
                new User
                {
                    Name = "Mr.",
                    Surname = "Peanutbutter",
                    Phone = "+37012345678",
                    Email = "dog@gmail.com",
                },
                new User
                {
                    Name = "Princess",
                    Surname = "Carolyn",
                    Phone = "+37012345678",
                    Email = "cat@gmail.com",
                },
            };

            await context.Users.AddRangeAsync(activities);
            await context.SaveChangesAsync();
        }
    }
}
