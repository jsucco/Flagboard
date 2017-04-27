namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Maintenance_Groups
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Maintenance_Groups()
        {
            Maintenance_Types = new HashSet<Maintenance_Types>();
        }

        [Key]
        public int MG_Id { get; set; }

        public short? MG_Number { get; set; }

        [Required]
        [StringLength(20)]
        public string MG_Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Maintenance_Types> Maintenance_Types { get; set; }
    }
}
