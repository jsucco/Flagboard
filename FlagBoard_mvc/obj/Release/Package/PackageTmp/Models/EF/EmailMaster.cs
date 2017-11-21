namespace FlagBoard_mvc.Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("EmailMaster")]
    public partial class EmailMaster
    {
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        public string Address { get; set; }

        public bool SPC_CAR_RPT { get; set; }

        public bool MCS_CAR_RPT { get; set; }

        public bool SPC_STT_RPT { get; set; }

        public bool ADMIN { get; set; }

        public bool? INS_ALERT_EMAIL { get; set; }

        public bool? SPEC_ALERT_EMAIL { get; set; }

        [StringLength(50)]
        public string HomeLocation { get; set; }
    }
}
