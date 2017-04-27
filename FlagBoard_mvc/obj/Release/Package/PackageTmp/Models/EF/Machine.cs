namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Machine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Machine()
        {
            Maintenance_Schedule = new HashSet<Maintenance_Schedule>();
        }

        [Key]
        public int MM_Id { get; set; }

        public short? MM_Number { get; set; }

        [Required]
        [StringLength(20)]
        public string MM_Name { get; set; }

        [StringLength(30)]
        public string MM_Description { get; set; }

        [StringLength(20)]
        public string MM_Model { get; set; }

        [StringLength(15)]
        public string MM_Capacity { get; set; }

        [StringLength(4)]
        public string MM_Year_Manufactured { get; set; }

        [StringLength(4)]
        public string MM_Year_Installed { get; set; }

        [StringLength(20)]
        public string MM_Serial { get; set; }

        [StringLength(20)]
        public string MM_Vendor { get; set; }

        [StringLength(30)]
        public string MM_Sales_Rep { get; set; }

        [StringLength(30)]
        public string MM_Sales_Rep_Phone { get; set; }

        [StringLength(30)]
        public string MM_Service_Phone { get; set; }

        [Column(TypeName = "ntext")]
        public string MM_Safety_Requirements { get; set; }

        public bool MM_Active { get; set; }

        public bool MM_Sterilizer { get; set; }

        public bool MM_Truck { get; set; }

        [StringLength(2)]
        public string Prod_Attribute1 { get; set; }

        [StringLength(2)]
        public string Prod_Attribute2 { get; set; }

        [StringLength(2)]
        public string Prod_Attribute3 { get; set; }

        [StringLength(4)]
        public string Machine_Group { get; set; }

        [StringLength(200)]
        public string Mach_Filename1 { get; set; }

        [StringLength(200)]
        public string Mach_Filename2 { get; set; }

        [StringLength(200)]
        public string Mach_Filename3 { get; set; }

        [StringLength(200)]
        public string Mach_Filename4 { get; set; }

        [Column(TypeName = "image")]
        public byte[] Mach_Image1 { get; set; }

        [Column(TypeName = "image")]
        public byte[] Mach_Image2 { get; set; }

        [Column(TypeName = "image")]
        public byte[] Mach_Image3 { get; set; }

        [Column(TypeName = "image")]
        public byte[] Mach_Image4 { get; set; }

        [StringLength(3)]
        public string MM_MFB_Code1 { get; set; }

        [StringLength(3)]
        public string MM_MFB_Code2 { get; set; }

        [StringLength(30)]
        public string MM_MFB_Desc1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Maintenance_Schedule> Maintenance_Schedule { get; set; }
    }
}
