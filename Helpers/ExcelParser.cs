using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDataReader;

namespace BarkodaCevirme.Helpers
{
    public static class ExcelParser
    {
        /// <summary>
        /// Parses the first column of the first sheet in an Excel file.
        /// Returns up to 40 rows of string data.
        /// </summary>
        public static List<string> Parse(Stream excelStream)
        {
            // Register the code pages provider (make sure the NuGet package is installed)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var result = new List<string>();

            using (var reader = ExcelReaderFactory.CreateReader(excelStream))
            {
                int rowCount = 0;
                while (reader.Read())
                {
                    // read the first column (0-based index)
                    if (reader.FieldCount > 0)
                    {
                        var cellValue = reader.GetValue(0);
                        if (cellValue != null)
                        {
                            result.Add(cellValue.ToString());
                        }
                    }
                    rowCount++;
                    if (rowCount >= 40) // limit to 40 rows
                    {
                        break;
                    }
                }
            }
            return result;
        }
    }
}
