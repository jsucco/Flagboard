namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee()
        {
            Maintenance_Schedule = new HashSet<Maintenance_Schedule>();
        }

        [Key]
        public int EMP_ID { get; set; }

        [StringLength(10)]
        public string EMP_Number { get; set; }

        [Required]
        [StringLength(15)]
        public string EMP_Last_Name { get; set; }

        [Required]
        [StringLength(10)]
        public string EMP_First_Name { get; set; }

        [StringLength(1)]
        public string EMP_MI { get; set; }

        [StringLength(11)]
        public string EMP_SSN { get; set; }

        [StringLength(25)]
        public string EMP_Street { get; set; }

        [StringLength(15)]
        public string EMP_City { get; set; }

        [StringLength(15)]
        public string EMP_State { get; set; }

        [StringLength(10)]
        public string EMP_Zip_Code { get; set; }

        public DateTime? EMP_Hire_Date { get; set; }

        public DateTime? EMP_Birth_Date { get; set; }

        [StringLength(15)]
        public string EMP_Phone_Number { get; set; }

        [StringLength(30)]
        public string EMP_Job_Title { get; set; }

        [Column(TypeName = "ntext")]
        public string EMP_Notes { get; set; }

        public short EMP_Type { get; set; }

        public short? EMP_Shift { get; set; }

        [Column(TypeName = "money")]
        public decimal EMP_Pay_Rate { get; set; }

        [StringLength(6)]
        public string EMP_Pay_Level { get; set; }

        public short EMP_Hours_Per_Period { get; set; }

        public bool EMP_Active { get; set; }

        [Required]
        [StringLength(5)]
        public string EMP_Hospital { get; set; }

        public short EMP_Area { get; set; }

        [StringLength(255)]
        public string EMP_Email { get; set; }

        [Required]
        [StringLength(15)]
        public string EMP_LoginLink { get; set; }

        [StringLength(15)]
        public string EMP_SubArea { get; set; }

        public bool EMP_Allow_All_Items { get; set; }

        [Required]
        [StringLength(2)]
        public string EMP_Status { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Maintenance_Schedule> Maintenance_Schedule { get; set; }
    }
}
