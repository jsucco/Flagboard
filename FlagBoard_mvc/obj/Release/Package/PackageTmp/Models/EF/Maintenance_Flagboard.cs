namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Maintenance_Flagboard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Maintenance_Flagboard()
        {
            Maintenance_Schedule = new HashSet<Maintenance_Schedule>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MFB_Id { get; set; }

        [Required]
        [StringLength(15)]
        public string UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string Loc_Description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Maintenance_Schedule> Maintenance_Schedule { get; set; }
    }
}
