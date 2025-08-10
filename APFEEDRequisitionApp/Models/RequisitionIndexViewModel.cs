namespace APFEEDRequisitionApp.Models
{
    public class RequisitionIndexViewModel
    {
        public string ReferenceNo { get; set; }
        public string Item { get; set; }
        public string From { get; set; }   // yyyy-MM-dd format for date inputs
        public string To { get; set; }
        public string Status { get; set; }

        public List<Requisition> Results { get; set; } = new List<Requisition>();

    }
}
