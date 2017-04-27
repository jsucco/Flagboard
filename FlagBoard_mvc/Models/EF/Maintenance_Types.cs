namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Maintenance_Types
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Maintenance_Types()
        {
            Maintenance_Schedule = new HashSet<Maintenance_Schedule>();
        }

        [Key]
        public int MT_Id { get; set; }

        public short? MT_Number { get; set; }

        [Required]
        [StringLength(30)]
        public string MT_Name { get; set; }

        [StringLength(40)]
        public string MT_Description { get; set; }

        [Column(TypeName = "ntext")]
        public string MT_Supplies_Notes { get; set; }

        public int? MG_Id { get; set; }

        public bool MT_Active { get; set; }

        [StringLength(3)]
        public string MT_MFB_Code1 { get; set; }

        [StringLength(3)]
        public string MT_MFB_Code2 { get; set; }

        [StringLength(30)]
        public string MT_MFB_Desc1 { get; set; }

        public virtual Maintenance_Groups Maintenance_Groups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Maintenance_Schedule> Maintenance_Schedule { get; set; }
    }
}
