namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AprManagerContext : DbContext
    {
        public AprManagerContext()
            : base("name=AprManagerContext")
        {
        }

        public virtual DbSet<LocationMaster> LocationMasters { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocationMaster>()
                .Property(e => e.Name)
                .IsFixedLength();

            modelBuilder.Entity<LocationMaster>()
                .Property(e => e.Abreviation)
                .IsUnicode(false);

            modelBuilder.Entity<LocationMaster>()
                .Property(e => e.DBname)
                .IsFixedLength();

            modelBuilder.Entity<LocationMaster>()
                .Property(e => e.CID)
                .IsFixedLength();

            modelBuilder.Entity<LocationMaster>()
                .Property(e => e.ConnectionString)
                .IsFixedLength();
        }
    }
}
