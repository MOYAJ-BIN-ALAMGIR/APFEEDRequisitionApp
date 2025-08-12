using APFEEDRequisitionApp.Models;
using APFEEDRequisitionApp.Models.Data;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using ClosedXML.Excel;  
using iText.Kernel.Pdf; 
using iText.Layout;
using iText.Layout.Element;
namespace APFEEDRequisitionApp.Controllers
{
    public class RequisitionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequisitionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string referenceNo, string from, string to, string status, string item)
        {
            var query = _context.Requisitions.AsQueryable();

            if (!string.IsNullOrEmpty(referenceNo))
                query = query.Where(r => r.ReferenceNo.Contains(referenceNo));

            if (!string.IsNullOrEmpty(item))
                query = query.Where(r => r.RequiredItems.Contains(item));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            // Parse dates in dd/MM/yyyy format
            if (!string.IsNullOrEmpty(from) && DateTime.TryParseExact(from, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime fromDate))
            {
                query = query.Where(r => r.RequisitionDate >= fromDate);
            }

            if (!string.IsNullOrEmpty(to) && DateTime.TryParseExact(to, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime toDate))
            {
                query = query.Where(r => r.RequisitionDate <= toDate);
            }

            var vm = new RequisitionIndexViewModel
            {
                ReferenceNo = referenceNo ?? "",
                Item = item ?? "",
                From = from ?? "",
                To = to ?? "",
                Status = status ?? "",
                Results = query.OrderBy(r => r.Id).ToList()
            };

            return View(vm);
        }


        public IActionResult Create()
        {
            string nextRef = GenerateNextReferenceNo();
            ViewBag.NextReferenceNo = nextRef;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Requisition model)
        {            

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Requisitions.Add(model);
                    _context.SaveChanges();

                    // Prepare for next entry
                    string nextRef = GenerateNextReferenceNo();
                    ViewBag.NextReferenceNo = nextRef;
                    ViewBag.Message = "✅ Record saved successfully!";

                    ModelState.Clear(); // clears previous values so form is empty
                    return View();
                }
                catch (Exception)
                {
                    ViewBag.ErrorMessage = "❌ An error occurred while saving. Please try again.";
                    ViewBag.NextReferenceNo = model.ReferenceNo; // keep the same reference
                    return View(model);
                }
            }

            // Validation failed
            ViewBag.ErrorMessage = "⚠ Please correct the Invalid data. Please check your input";
            ViewBag.NextReferenceNo = model.ReferenceNo;
            return View(model);
        }


        private string GenerateNextReferenceNo()
        {
            string prefix = "APFM/PRO/";
            string yearPart = DateTime.Now.ToString("yy") + "-"; // e.g. "25-"

            // Filter only requisitions for current year prefix, e.g. APFM/PRO/25-
            var lastReq = _context.Requisitions
                .Where(r => r.ReferenceNo.StartsWith(prefix + yearPart))
                .OrderByDescending(r => r.Id)
                .FirstOrDefault();

            int nextNumber = 1;

            if (lastReq != null)
            {
                // Extract the serial number after the year part
                string numPart = lastReq.ReferenceNo.Substring((prefix + yearPart).Length);

                if (int.TryParse(numPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{yearPart}{nextNumber:D4}"; // e.g. APFM/PRO/25-0001
        }


        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var requisition = _context.Requisitions.Find(id);
            if (requisition == null)
                return NotFound();

            return View(requisition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Requisition requisition)
        {
            
            if (id != requisition.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(requisition);
                    int result = _context.SaveChanges();

                    if (result > 0)
                        TempData["SuccessMessage"] = "✅ Record updated successfully!";
                    else
                        TempData["ErrorMessage"] = "⚠ No changes were made.";
                }
                catch
                {
                    TempData["ErrorMessage"] = "❌ An error occurred while updating.";
                }

                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "❌ Invalid data. Please check your input.";
            return View(requisition);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var requisition = _context.Requisitions.Find(id);
            if (requisition == null)
                return NotFound();

            return View(requisition);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var requisition = _context.Requisitions.Find(id);
            if (requisition != null)
            {
                _context.Requisitions.Remove(requisition);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Export()
        {
            return View();
        }

        public IActionResult ExportExcel()
        {
            var data = _context.Requisitions.OrderBy(r => r.Id).ToList();
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Requisitions");
            worksheet.Cell(1, 1).Value = "SI No";
            worksheet.Cell(1, 2).Value = "Reference No";
            worksheet.Cell(1, 3).Value = "Requisition Date";
            worksheet.Cell(1, 4).Value = "Requisition By";
            worksheet.Cell(1, 5).Value = "Required Items";
            worksheet.Cell(1, 6).Value = "Status";
            worksheet.Cell(1, 7).Value = "Completed Date";
            worksheet.Cell(1, 8).Value = "Remarks";

            int row = 2;
            foreach (var r in data)
            {
                worksheet.Cell(row, 1).Value = r.Id;
                worksheet.Cell(row, 2).Value = r.ReferenceNo;
                worksheet.Cell(row, 3).Value = r.RequisitionDate.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 4).Value = r.RequisitionBy;
                worksheet.Cell(row, 5).Value = r.RequiredItems;
                worksheet.Cell(row, 6).Value = r.Status;
                worksheet.Cell(row, 7).Value = r.CompletedDate?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 8).Value = r.Remarks;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Requisitions.xlsx");
        }

        public IActionResult ExportPdf()
        {
            var data = _context.Requisitions.OrderBy(r => r.Id).ToList();

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            document.Add(new Paragraph("Requisitions Report").SetFontSize(20));

            var table = new Table(8, true);

            string[] headers = { "SI No", "Reference No", "Requisition Date", "Requisition By", "Required Items", "Status", "Completed Date", "Remarks" };
            foreach (var h in headers)
                table.AddHeaderCell(h);

            foreach (var r in data)
            {
                table.AddCell(r.Id.ToString());
                table.AddCell(r.ReferenceNo);
                table.AddCell(r.RequisitionDate.ToString("dd/MM/yyyy"));
                table.AddCell(r.RequisitionBy);
                table.AddCell(r.RequiredItems);
                table.AddCell(r.Status);
                table.AddCell(r.CompletedDate?.ToString("dd/MM/yyyy") ?? "");
                table.AddCell(r.Remarks);
            }

            document.Add(table);
            document.Close();

            return File(ms.ToArray(), "application/pdf", "Requisitions.pdf");
        }

        //public IActionResult Update(string referenceNo, DateTime? from, DateTime? to, string status, string item)
        //{
        //    var query = _context.Requisitions.AsQueryable();

        //    if (!string.IsNullOrEmpty(referenceNo))
        //        query = query.Where(r => r.ReferenceNo.Contains(referenceNo));

        //    if (!string.IsNullOrEmpty(item))
        //        query = query.Where(r => r.RequiredItems.Contains(item));

        //    if (!string.IsNullOrEmpty(status))
        //        query = query.Where(r => r.Status == status);

        //    if (from.HasValue)
        //        query = query.Where(r => r.RequisitionDate >= from.Value);

        //    if (to.HasValue)
        //        query = query.Where(r => r.RequisitionDate <= to.Value);

        //    var vm = new RequisitionIndexViewModel
        //    {
        //        ReferenceNo = referenceNo ?? "",
        //        Item = item ?? "",
        //        From = from?.ToString("dd/MM/yyyy") ?? "",
        //        To = to?.ToString("dd/MM/yyyy") ?? "",
        //        Status = status ?? "",
        //        Results = query.OrderBy(r => r.Id).ToList()
        //    };

        //    return View(vm);
        //}
    }
}
