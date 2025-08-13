using System.ComponentModel.DataAnnotations;
using System;
namespace APFEEDRequisitionApp.Models
{
    public class Requisition
    {
        [Key]
        public int Id { get; set; } // SI No (auto-increment)

        [Required(ErrorMessage = "Reference Number is required.")]
        [StringLength(50)]
        public string ReferenceNo { get; set; }

        [Required(ErrorMessage = "Requisition Date is required.")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime RequisitionDate { get; set; }
       
        [Required(ErrorMessage = "Requisition By is required.")]
        [StringLength(100)]
        public string RequisitionBy { get; set; }

        [Required(ErrorMessage = "Required Items is required.")]
        public string RequiredItems { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? CompletedDate { get; set; }

        [Required(ErrorMessage = "Remarks is required.")]
        public string Remarks { get; set; }
    }
}
