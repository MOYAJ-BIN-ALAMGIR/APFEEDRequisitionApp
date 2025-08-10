using APFEEDRequisitionApp.Models;
using APFEEDRequisitionApp.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace APFEEDRequisitionApp.Controllers
{
    public class RequisitionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequisitionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string referenceNo, DateTime? from, DateTime? to, string status, string item)
        {
            var query = _context.Requisitions.AsQueryable();

            if (!string.IsNullOrEmpty(referenceNo))
                query = query.Where(r => r.ReferenceNo.Contains(referenceNo));

            if (!string.IsNullOrEmpty(item))
                query = query.Where(r => r.RequiredItems.Contains(item));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            if (from.HasValue)
                query = query.Where(r => r.RequisitionDate >= from.Value);

            if (to.HasValue)
                query = query.Where(r => r.RequisitionDate <= to.Value);

            var vm = new RequisitionIndexViewModel
            {
                ReferenceNo = referenceNo ?? "",
                Item = item ?? "",
                From = from?.ToString("yyyy-MM-dd") ?? "",
                To = to?.ToString("yyyy-MM-dd") ?? "",
                Status = status ?? "",
                Results = query.OrderByDescending(r => r.Id).ToList()
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
            if(ModelState.IsValid)
            {
                _context.Requisitions.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        private string GenerateNextReferenceNo()
        {
            string prefix = "APFM/PRO/25-";
            var lastReq = _context.Requisitions
                .OrderByDescending(r => r.Id)
                .FirstOrDefault();
            
            int nextNumber = 1;

            if (lastReq != null && lastReq.ReferenceNo.StartsWith(prefix))
            {
                var numPart = lastReq.ReferenceNo.Substring(prefix.Length);
                if (int.TryParse(numPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            return $"{prefix}{nextNumber:D4}";
        }

        public IActionResult Edit (int? id)
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
                _context.Update(requisition);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
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
    }
}
