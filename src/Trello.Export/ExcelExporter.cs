using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OfficeOpenXml;
using TrelloNet;

namespace Trello.Export.Web
{
    public class ExcelExporter
    {
        public ExcelPackage Export(List<Card> cards)
        {
            var excelPackage = new ExcelPackage();

            var worksheet = excelPackage.Workbook.Worksheets.Add("Trello");

            for( int i = 1; i <= cards.Count; i++ )
            {
                var card = cards[i - 1];

                worksheet.Cells[i, 1].Value = card.Name;
            }

            excelPackage.Save();

            excelPackage.Stream.Position = 0;

            return excelPackage;
        }
    }
}