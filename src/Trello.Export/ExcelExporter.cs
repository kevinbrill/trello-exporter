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

        private enum ColumnNumbers
        {
            LabelColumn = 1,
            ListColumn,
            NumberColumn,
            TitleColumn,
            DescriptionColumn,
            CommentsColumn,
            MembersColumn,
            ToDoColumn,
            DueDateColumn
        }

        public ExcelPackage Export(List<Card> cards, Dictionary<string, List> lists)
        {
            var excelPackage = new ExcelPackage();

            var worksheet = excelPackage.Workbook.Worksheets.Add("Trello");

            BuildHeaderRow(worksheet);

            BuildCardRows(worksheet, cards, lists);

            excelPackage.Save();

            excelPackage.Stream.Position = 0;

            return excelPackage;
        }

        private void BuildCardRows(ExcelWorksheet worksheet, List<Card> cards, Dictionary<string, List> lists)
        {
            for (int i = 2; i <= cards.Count; i++)
            {
                var card = cards[i - 1];

                worksheet.Cells[i, (int)ColumnNumbers.LabelColumn].Value = GetLabels(card);
                worksheet.Cells[i, (int)ColumnNumbers.ListColumn].Value = lists[card.IdList].Name;
                worksheet.Cells[i, (int)ColumnNumbers.NumberColumn].Value = card.IdShort;
                worksheet.Cells[i, (int)ColumnNumbers.TitleColumn].Value = card.Name;
                worksheet.Cells[i, (int)ColumnNumbers.DescriptionColumn].Value = card.Desc;
                worksheet.Cells[i, (int)ColumnNumbers.MembersColumn].Value = GetMembers(card);
                worksheet.Cells[i, (int)ColumnNumbers.ToDoColumn].Value = GetToDos(card);
                worksheet.Cells[i, (int)ColumnNumbers.DueDateColumn].Value = card.Due != null ? card.Due.Value.ToShortDateString() : string.Empty;
            }                       
        }

        private void BuildHeaderRow(ExcelWorksheet worksheet)
        {
            
        }

        private string GetLabels(Card card)
        {
            var labels = card.Labels ?? new List<Card.Label>();

            return string.Join(",", labels.Select(x => x.Name));
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