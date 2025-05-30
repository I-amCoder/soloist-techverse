using AspnetCoreMvcFull.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace AspnetCoreMvcFull.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager)
        {
            // Seed roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new ApplicationRole 
                { 
                    Name = "Admin", 
                    Description = "Administrator with full access" 
                });
            }

            if (!await roleManager.RoleExistsAsync("Student"))
            {
                await roleManager.CreateAsync(new ApplicationRole 
                { 
                    Name = "Student", 
                    Description = "Student user" 
                });
            }

            // Seed admin user
            var adminEmail = "admin@umt.edu.pk";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };
                
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed test student
            var studentEmail = "student@umt.edu.pk";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            
            if (studentUser == null)
            {
                studentUser = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true,
                    FirstName = "Test",
                    LastName = "Student"
                };
                
                await userManager.CreateAsync(studentUser, "Student@123");
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }
    }
} 