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
        //Board	Label	Status	Card Number	Card Title	Card Description	Activity	Members	Checklist	Due Date

        private const int BoardColumn = 1;
        private const int LabelColumn = 2;
        private const int ListColumn = 3;
        private const int NumberColumn = 4;
        private const int TitleColumn = 5;
        private const int DescriptionColumn = 6;
        private const int CommentsColumn = 7;
        private const int MembersColumn = 8;
        private const int ToDoColumn = 9;
        private const int DueDateColumn = 10;

        public ExcelPackage Export(List<Card> cards)
        {
            var excelPackage = new ExcelPackage();

            var worksheet = excelPackage.Workbook.Worksheets.Add("Trello");

            for( int i = 1; i <= cards.Count; i++ )
            {
                var card = cards[i - 1];

                worksheet.Cells[i, BoardColumn].Value = card.IdBoard;
                worksheet.Cells[i, LabelColumn].Value = string.Join(",", card.Labels.Select(x => x.Name).ToArray());
                worksheet.Cells[i, ListColumn].Value = card.IdList;
                worksheet.Cells[i, NumberColumn].Value = card.IdShort;
                worksheet.Cells[i, TitleColumn].Value = card.Name;
                worksheet.Cells[i, DescriptionColumn].Value = card.Desc;
            }

            excelPackage.Save();

            excelPackage.Stream.Position = 0;

            return excelPackage;
        }
    }
}