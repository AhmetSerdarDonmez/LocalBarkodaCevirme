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
        /// Arranges them in a grid (4 columns per row) where each row is followed by an empty row.
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

                int cellCount = 0;
                foreach (var textValue in dataList)
                {
                    // Create a cell for the barcode.
                    var cell = new PdfPCell
                    {
                        Border = iTextSharp.text.Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5f,
                        FixedHeight = 80f // Ensures the barcode stays within its cell.
                    };

                    // Generate the barcode image.
                    System.Drawing.Image barcodeImage = GenerateBarcodeImage(textValue);
                    iTextSharp.text.Image pdfImage = iTextSharp.text.Image.GetInstance(barcodeImage, System.Drawing.Imaging.ImageFormat.Png);
                    pdfImage.ScalePercent(70f); // Adjust scale as needed.

                    // Add the barcode image to the cell.
                    cell.AddElement(pdfImage);

                    table.AddCell(cell);
                    cellCount++;

                    // After a full row (4 cells), insert an empty row for readability.
                    if (cellCount % 4 == 0)
                    {
                        AddEmptyRow(table, 4);
                    }
                }

                // If the last row isn't complete, fill the remaining cells,
                // then add an empty row.
                if (cellCount % 4 != 0)
                {
                    int remainder = 4 - (cellCount % 4);
                    for (int i = 0; i < remainder; i++)
                    {
                        PdfPCell filler = new PdfPCell { Border = iTextSharp.text.Rectangle.NO_BORDER };
                        table.AddCell(filler);
                    }
                    AddEmptyRow(table, 4);
                }

                doc.Add(table);
                doc.Close();
                writer.Close();

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Adds a row of empty cells with a fixed height to serve as spacing between rows.
        /// </summary>
        private static void AddEmptyRow(PdfPTable table, int numColumns)
        {
            for (int i = 0; i < numColumns; i++)
            {
                PdfPCell emptyCell = new PdfPCell(new Phrase(""))
                {
                    Border = iTextSharp.text.Rectangle.NO_BORDER,
                    FixedHeight = 10f // Adjust this height for more or less spacing.
                };
                table.AddCell(emptyCell);
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
