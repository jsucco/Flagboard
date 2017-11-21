namespace FlagBoard_mvc.Models.EF
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("LocationMaster")]
    public partial class LocationMaster
    {
        public int id { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(10)]
        public string Abreviation { get; set; }

        [StringLength(30)]
        public string DBname { get; set; }

        [StringLength(10)]
        public string CID { get; set; }

        [StringLength(30)]
        public string ConnectionString { get; set; }

        public bool? InspectionResults { get; set; }

        public bool? ProductionResults { get; set; }

        public bool? AS400_Connection { get; set; }

        [StringLength(50)]
        public string AS400_Abr { get; set; }

        [StringLength(10)]
        public string CtxCID { get; set; }
    }
}
