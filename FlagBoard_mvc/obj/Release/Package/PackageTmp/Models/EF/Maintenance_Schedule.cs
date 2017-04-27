namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Maintenance_Schedule
    {
        public int MM_Id { get; set; }

        public int MT_Id { get; set; }

        public DateTime MS_Next_Main_Date { get; set; }

        [StringLength(15)]
        public string MS_Workorder { get; set; }

        public short MS_Frequency { get; set; }

        public DateTime MS_Last_Main_Date { get; set; }

        public DateTime? MS_Main_Comp_Date { get; set; }

        public int? EMP_ID { get; set; }

        public short MS_Maint_Code { get; set; }

        public short MS_Maint_Time_Alotted { get; set; }

        public short MS_Main_Time_Required { get; set; }

        public int MS_Machine_Hours { get; set; }

        [StringLength(50)]
        public string MS_Unscheduled_Reason { get; set; }

        [Column(TypeName = "ntext")]
        public string MS_Notes { get; set; }

        public float MS_Total_Machine_Downtime { get; set; }

        public bool MS_Inventory_Decremented { get; set; }

        public int? MFB_Id { get; set; }

        [Key]
        public int MS_Id { get; set; }

        public DateTime? MS_WOCreate_Timestamp { get; set; }

        public DateTime? MS_WOClosed_Timestamp { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual Machine Machine { get; set; }

        public virtual Maintenance_Codes Maintenance_Codes { get; set; }

        public virtual Maintenance_Flagboard Maintenance_Flagboard { get; set; }

        public virtual Maintenance_Types Maintenance_Types { get; set; }
    }
}
