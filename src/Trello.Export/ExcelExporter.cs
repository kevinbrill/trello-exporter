using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TrelloNet;

namespace Trello.Export.Web
{
    public class ExcelExporter
    {
        //Board	Label	Status	Card Number	Card Title	Card Description	Activity	Members	Checklist	Due Date

        private enum ColumnNumbers
        {
            ListColumn = 1,
            LabelColumn,
            NumberColumn,
            TitleColumn,
            DescriptionColumn,
            CommentsColumn,
            MembersColumn,
            ToDoColumn,
            DueDateColumn,
            LastColumn
        }

        public ExcelPackage Export(List<Card> cards, Dictionary<string, List> lists)
        {
            var excelPackage = new ExcelPackage();

            var worksheet = excelPackage.Workbook.Worksheets.Add("Trello");

            BuildHeaderRow(worksheet);

            BuildCardRows(worksheet, cards, lists);

            SizeAndFormatCells(worksheet);

            excelPackage.Save();

            excelPackage.Stream.Position = 0;

            return excelPackage;
        }

        private void SizeAndFormatCells(ExcelWorksheet worksheet)
        {
            worksheet.Column((int)ColumnNumbers.ListColumn).AutoFit();
            worksheet.Column((int)ColumnNumbers.TitleColumn).Width = 60;
            worksheet.Column((int)ColumnNumbers.DescriptionColumn).Width = 125;

            var contentRange = worksheet.Cells[2, 1, 100, (int)ColumnNumbers.LastColumn];
            
            contentRange.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            contentRange.Style.Border.BorderAround(ExcelBorderStyle.Thick, System.Drawing.Color.Black);
            contentRange.Style.WrapText = true;
        }

        private void BuildCardRows(ExcelWorksheet worksheet, List<Card> cards, Dictionary<string, List> lists)
        {
            for (int i = 2; i <= cards.Count; i++)
            {
                var card = cards[i - 1];

                worksheet.Cells[i, (int) ColumnNumbers.LabelColumn].Value = GetLabels(card);
                worksheet.Cells[i, (int) ColumnNumbers.ListColumn].Value = lists[card.IdList].Name;
                worksheet.Cells[i, (int) ColumnNumbers.NumberColumn].Value = card.IdShort;
                worksheet.Cells[i, (int) ColumnNumbers.TitleColumn].Value = card.Name;
                worksheet.Cells[i, (int) ColumnNumbers.DescriptionColumn].Value = card.Desc;
                worksheet.Cells[i, (int) ColumnNumbers.MembersColumn].Value = GetMembers(card);
                worksheet.Cells[i, (int) ColumnNumbers.ToDoColumn].Value = GetToDos(card);
                worksheet.Cells[i, (int) ColumnNumbers.DueDateColumn].Value = card.Due != null
                                                                                  ? card.Due.Value.ToShortDateString()
                                                                                  : string.Empty;
            }
        }

        private void BuildHeaderRow(ExcelWorksheet worksheet)
        {
            worksheet.Cells[1, (int) ColumnNumbers.LabelColumn].Value = "Labels";
            worksheet.Cells[1, (int) ColumnNumbers.ListColumn].Value = "List";
            worksheet.Cells[1, (int) ColumnNumbers.NumberColumn].Value = "Card Number";
            worksheet.Cells[1, (int) ColumnNumbers.TitleColumn].Value = "Title";
            worksheet.Cells[1, (int) ColumnNumbers.DescriptionColumn].Value = "Description";
            worksheet.Cells[1, (int) ColumnNumbers.MembersColumn].Value = "Members";
            worksheet.Cells[1, (int) ColumnNumbers.ToDoColumn].Value = "To Dos";
            worksheet.Cells[1, (int) ColumnNumbers.DueDateColumn].Value = "Due Date";

            var style = worksheet.Cells[1, 1, 1, (int)ColumnNumbers.LastColumn].Style;
            style.Fill.PatternType = ExcelFillStyle.Solid;
            style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
            style.Font.Bold = true;
            style.Font.Color.SetColor(System.Drawing.Color.White);
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