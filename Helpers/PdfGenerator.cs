using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ZXing;

namespace BarkodaCevirme.Helpers
{
    public static class PdfGenerator
    {
        /// <summary>
        /// Generates a PDF (as byte[]) containing barcodes for each string in dataList.
        /// Arranges them in a grid (4 columns per row) with styling similar to your sample.
        /// The barcode text (code) is shown only once, as part of the barcode image.
        /// </summary>
        public static byte[] Generate(List<string> dataList)
        {
            using (var ms = new MemoryStream())
            {
                // Setup document with A4 size and margins.
                var doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Create a table with 4 columns.
                var table = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };
                table.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                // For each barcode value, generate a barcode image and add to a cell.
                foreach (var textValue in dataList)
                {
                    // Generate barcode image (ZXing returns a System.Drawing.Image).
                    System.Drawing.Image barcodeImage = GenerateBarcodeImage(textValue);

                    // Convert to iTextSharp image.
                    iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(barcodeImage, System.Drawing.Imaging.ImageFormat.Png);
                    pdfImage.ScalePercent(70f); // Adjust scale as needed.

                    // Create a cell for this barcode.
                    var cell = new PdfPCell
                    {
                        Border = iTextSharp.text.Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5f
                    };

                    // Add only the barcode image (removing the duplicate text label).
                    cell.AddElement(pdfImage);

                    table.AddCell(cell);
                }

                // Fill empty cells if the last row is incomplete.
                int remainder = dataList.Count % 4;
                if (remainder != 0)
                {
                    for (int i = 0; i < 4 - remainder; i++)
                    {
                        table.AddCell(new PdfPCell { Border = iTextSharp.text.Rectangle.NO_BORDER });
                    }
                }

                doc.Add(table);
                doc.Close();
                writer.Close();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Generates a barcode image (Code128) from a string using ZXing.NET.
        /// </summary>
        private static System.Drawing.Image GenerateBarcodeImage(string text)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Height = 60,
                    Width = 200,
                    Margin = 1
                }
            };
            return barcodeWriter.Write(text);
        }
    }
}
