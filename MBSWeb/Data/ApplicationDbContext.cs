using MBSWeb.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MBSWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<MBSUsers, MBSAccessRoles, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for your models, e.g.,

        public DbSet<Companies> Companies { get; set; }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<InvoiceTransactions> InvoiceTransactions { get; set; }
        public DbSet<ItemLines> ItemLines { get; set; }

        //public DbSet<MBSUsers> MBSUsers { get; set; } // Example for another model
        //public DbSet<MBSAccessRoles> MBSAccessRoles { get; set; } // Example for another model

    }
}
