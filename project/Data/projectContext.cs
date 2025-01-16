using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project;

namespace project.Data
{
    public class projectContext : DbContext
    {
        public projectContext (DbContextOptions<projectContext> options)
            : base(options)
        {
        }

        public DbSet<project.Models.usersaccounts> usersaccounts { get; set; } = default!;
        public DbSet<project.customer_search> customer_search { get; set; } = default!;
        public DbSet<project.Models.orders> orders { get; set; } = default!;
        public DbSet<project.Models.items> items { get; set; } = default!;
        public DbSet<project.Models.orderline> orderline { get; set; } = default!;
        public DbSet<project.report> report { get; set; } = default!;
    }
}
