using System.ComponentModel.DataAnnotations;
using System;
namespace APFEEDRequisitionApp.Models
{
    public class Requisition
    {
        [Key]
        public int Id { get; set; } // SI No (auto-increment)

        [Required]
        [StringLength(50)]
        public string ReferenceNo { get; set; }

        [DataType(DataType.Date)]
        public DateTime RequisitionDate { get; set; }

        [Required]
        [StringLength(100)]
        public string RequisitionBy { get; set; }

        [Required]
        public string RequiredItems { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }

        public string Remarks { get; set; }
    }
}
