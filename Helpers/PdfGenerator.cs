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
        /// Places barcodes in a grid (4 columns per row).
        /// </summary>
        public static byte[] Generate(List<string> dataList)
        {
            using (var ms = new MemoryStream())
            {
                // iTextSharp Document setup
                var doc = new Document(PageSize.A4, 20f, 20f, 20f, 20f);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Create a table with 4 columns
                var table = new PdfPTable(4);
                table.WidthPercentage = 100;
                // Explicitly use iTextSharp.text.Rectangle for NO_BORDER
                table.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                table.SpacingBefore = 10f;

                // For each item in dataList, generate a barcode cell
                foreach (var textValue in dataList)
                {
                    // Generate barcode image
                    System.Drawing.Image barcodeImage = GenerateBarcodeImage(textValue);

                    // Convert System.Drawing.Image to iTextSharp.text.Image
                    iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(barcodeImage, System.Drawing.Imaging.ImageFormat.Png);
                    pdfImage.ScalePercent(70f); // adjust size if needed

                    // Create a cell, add the barcode image
                    var cell = new PdfPCell();
                    cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;

                    // Add the barcode image
                    cell.AddElement(pdfImage);

                    // Optionally add the text label below the barcode
                    var paragraph = new Paragraph(textValue, FontFactory.GetFont(FontFactory.HELVETICA, 10));
                    paragraph.Alignment = Element.ALIGN_CENTER;
                    cell.AddElement(paragraph);

                    table.AddCell(cell);
                }

                // If the last row doesn't have 4 columns, fill them with empty cells
                int remainder = dataList.Count % 4;
                if (remainder != 0)
                {
                    for (int i = 0; i < 4 - remainder; i++)
                    {
                        table.AddCell(new PdfPCell() { Border = iTextSharp.text.Rectangle.NO_BORDER });
                    }
                }

                // Add table to document
                doc.Add(table);

                doc.Close();
                writer.Close();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Generates a barcode image (Code128) from a string using ZXing.NET
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
