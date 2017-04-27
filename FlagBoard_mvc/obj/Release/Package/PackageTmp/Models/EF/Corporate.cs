namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Corporate")]
    public partial class Corporate
    {
        [Key]
        [StringLength(10)]
        public string CID { get; set; }

        [Required]
        [StringLength(45)]
        public string CorporateName { get; set; }

        [Required]
        [StringLength(15)]
        public string DBType { get; set; }

        [Required]
        [StringLength(50)]
        public string DBName { get; set; }

        [Required]
        [StringLength(256)]
        public string DBSource { get; set; }

        [Required]
        [StringLength(256)]
        public string DBProvider { get; set; }

        [Required]
        [StringLength(15)]
        public string DBUserID { get; set; }

        [Required]
        [StringLength(10)]
        public string DBPassword { get; set; }

        [Required]
        [StringLength(10)]
        public string DBTimeOut { get; set; }

        public bool Booth { get; set; }

        public bool Hospitality { get; set; }

        public bool Mission { get; set; }

        public int CountryCode { get; set; }

        public DateTime SubscriptionExpiration { get; set; }

        public bool SRBEnabled { get; set; }

        public bool GarmentsEnabled { get; set; }

        public bool PackroomEnabled { get; set; }

        public bool LaborEnabled { get; set; }

        public bool PMEnabled { get; set; }

        public bool OrderEnabled { get; set; }

        public bool OrderAutoprint { get; set; }

        public bool OrderZeroQty { get; set; }

        public byte Handheld { get; set; }

        [StringLength(100)]
        public string CustomReports { get; set; }

        public bool SwitchSoilFacilities { get; set; }

        public bool TransportationEnabled { get; set; }

        public bool CRMEnabled { get; set; }

        public bool WiFi_Shipping { get; set; }

        public bool APRPM_Enabled { get; set; }

        public bool APRUtility_Enabled { get; set; }

        public bool APRLoom_Enabled { get; set; }

        public bool APRInspection_Enabled { get; set; }

        public bool CurtainsEnabled { get; set; }

        public bool APRSPC_Enabled { get; set; }

        public bool CurtainsStandalone { get; set; }

        public bool StoreRoomEnabled { get; set; }
    }
}
