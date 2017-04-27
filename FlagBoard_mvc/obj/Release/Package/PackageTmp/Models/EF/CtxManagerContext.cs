namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CtxManagerContext : DbContext
    {
        public CtxManagerContext()
            : base("name=CtxManagerContext")
        {
        }

        public virtual DbSet<Corporate> Corporates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Corporate>()
                .Property(e => e.CID)
                .IsFixedLength();
        }
    }
}
