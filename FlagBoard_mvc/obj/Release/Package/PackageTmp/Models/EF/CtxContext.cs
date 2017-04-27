namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Data.Entity.Core.EntityClient;

    public partial class CtxContext : DbContext
    {
        public CtxContext()
            : base("name=CtxContext")
        {
        }

        public CtxContext(string sConnectionString)
         : base(sConnectionString)
        {
        }

        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Machine> Machines { get; set; }
        public virtual DbSet<Maintenance_Codes> Maintenance_Codes { get; set; }
        public virtual DbSet<Maintenance_Flagboard> Maintenance_Flagboard { get; set; }
        public virtual DbSet<Maintenance_Groups> Maintenance_Groups { get; set; }
        public virtual DbSet<Maintenance_Schedule> Maintenance_Schedule { get; set; }
        public virtual DbSet<Maintenance_Types> Maintenance_Types { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                .Property(e => e.EMP_Pay_Rate)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Machine>()
                .HasMany(e => e.Maintenance_Schedule)
                .WithRequired(e => e.Machine)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Maintenance_Codes>()
                .HasMany(e => e.Maintenance_Schedule)
                .WithRequired(e => e.Maintenance_Codes)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Maintenance_Types>()
                .HasMany(e => e.Maintenance_Schedule)
                .WithRequired(e => e.Maintenance_Types)
                .WillCascadeOnDelete(false);
        }
    }
}
