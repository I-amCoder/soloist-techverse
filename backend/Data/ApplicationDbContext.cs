using AspnetCoreMvcFull.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

         public DbSet<Item> Items { get; set; }
         public DbSet<ClaimRequest> ClaimRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed roles
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole 
                { 
                    Id = "1", 
                    Name = "Admin", 
                    NormalizedName = "ADMIN", 
                    Description = "Administrator with full access" 
                },
                new ApplicationRole 
                { 
                    Id = "2", 
                    Name = "Manager", 
                    NormalizedName = "MANAGER", 
                    Description = "Manager with limited access" 
                },
                new ApplicationRole 
                { 
                    Id = "3", 
                    Name = "User", 
                    NormalizedName = "USER", 
                    Description = "Regular user" 
                }
            );

            // Configure Item-User relationship
            builder.Entity<Item>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ClaimRequest relationships
            builder.Entity<ClaimRequest>()
                .HasOne(c => c.Item)
                .WithMany(i => i.ClaimRequests)
                .HasForeignKey(c => c.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClaimRequest>()
                .HasOne(c => c.Claimant)
                .WithMany()
                .HasForeignKey(c => c.ClaimantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 
