using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                worksheet.Cells[i, MembersColumn].Value = GetMembers(card);
                worksheet.Cells[i, ToDoColumn].Value = GetToDos(card);
                worksheet.Cells[i, DueDateColumn].Value = card.Due != null ? card.Due.Value.ToShortDateString() : string.Empty;
            }
            
            excelPackage.Save();

            excelPackage.Stream.Position = 0;

            return excelPackage;
        }

        private string GetMembers(Card card)
        {
            var members = card.Members ?? new List<Member>();

            return string.Join(",", members.Select(x => x.FullName));
        }

        private string GetToDos(Card card)
        {
            var incompleteToDos = new StringBuilder();

            var checklist = card.Checklists.FirstOrDefault();

            if (checklist == null)
            {
                return string.Empty;
            }

            foreach (var checkItem in checklist.CheckItems)
            {
                incompleteToDos.AppendFormat("{0}{1}", checkItem.Name, Environment.NewLine);
            }

            return incompleteToDos.ToString();
        }
    }
}