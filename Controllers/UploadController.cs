using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BarkodaCevirme.Helpers; // Updated namespace

namespace BarkodaCevirme.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload/Index
        public ActionResult Index()
        {
            return View();
        }

        // POST: Upload/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("", "Please choose a valid Excel file.");
                return View();
            }

            // Validate extension (.xls or .xlsx)
            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".xls", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Only .xls or .xlsx files are allowed.");
                return View();
            }

            try
            {
                // 1) Parse Excel
                List<string> rowData;
                using (var stream = file.InputStream)
                {
                    rowData = ExcelParser.Parse(stream);
                }

                // 2) Generate PDF with barcodes
                var pdfBytes = PdfGenerator.Generate(rowData);

                // 3) Store PDF in Session for Preview/Download
                Session["GeneratedPdf"] = pdfBytes;

                // 4) Redirect to Preview
                return RedirectToAction("Preview");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error processing file: " + ex.Message);
                return View();
            }
        }

        // GET: Upload/Preview
        public ActionResult Preview()
        {
            if (Session["GeneratedPdf"] == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Upload/GetTempPdf
        public FileResult GetTempPdf()
        {
            if (Session["GeneratedPdf"] != null)
            {
                var pdfBytes = Session["GeneratedPdf"] as byte[];
                return File(pdfBytes, "application/pdf");
            }
            return File(new byte[0], "application/pdf");
        }

        // GET: Upload/Download
        public ActionResult Download()
        {
            if (Session["GeneratedPdf"] != null)
            {
                var pdfBytes = Session["GeneratedPdf"] as byte[];
                var fileName = "Barcodes.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            return RedirectToAction("Index");
        }
    }
}
