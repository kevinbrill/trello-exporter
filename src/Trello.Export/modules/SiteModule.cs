using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using MoreLinq;
using Nancy;
using Nancy.Helpers;
using Nancy.ModelBinding;
using Nancy.Responses;
using TrelloNet;

namespace Trello.Export.Web.modules
{
    public class SiteModule : NancyModule
    {
        private readonly TrelloNet.Trello trello;
        private const string Orginization = "researchteam2";

        public SiteModule()
        {
            trello = new TrelloNet.Trello(ConfigurationManager.AppSettings["AuthKey"]);
            trello.Authorize(ConfigurationManager.AppSettings["AuthToken"]);

            Get["/"] = o =>
                {
                    var boards = GetBoards();

                    var primaryBoard = boards.First();

                    dynamic model = new
                        {
                            Boards = boards,
                            Lists = GetLists(primaryBoard.Id)
                        };

                    return View["views/default", model];
                };

            Post["/export"] = o =>
                {
                    using (var stringReader = new StreamReader(Request.Body))
                    {
                        var body = stringReader.ReadToEnd();

                        var queryString = HttpUtility.ParseQueryString(body);
                        var selectedCardIds = queryString["selectedCards"].Split(',');

                        // Pull all the cards for the provided Ids
                        var cards = selectedCardIds.AsParallel()
                                                   .Select(id => trello.Cards.WithId(id))
                                                   .ToList();

                        // Given the cards, fetch the associated lists and drop them
                        //  into a dictionary
                        var lists = cards.DistinctBy(x => x.IdList)
                                         .AsParallel()
                                         .Select(x => trello.Lists.WithId(x.IdList))
                                         .ToDictionary(x => x.Id);

                        var exporter = new ExcelExporter();
                        var export = exporter.Export(cards,lists);

                        var response = Response.FromStream(export.Stream,
                                                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                        response.WithHeader("Content-Length", export.Stream.Length.ToString());
                        
                        return response;
                    }
                };

            Get["/api/boards"] = o => GetBoards();

            Get["/api/boards/{id}/lists"] = o => GetLists(o.id);

            Get["/api/boards/{boardId}/lists/{listId}/cards"] = o =>
                {
                    var results = new List<dynamic>();

                    var listIdsAsString = (string) o.listId;

                    var listIds = listIdsAsString.Split(',');

                    foreach (var listId in listIds)
                    {
                        var list = trello.Lists.WithId(listId);

                        var cards = trello.Cards.ForList(list);

                        results.AddRange(cards);
                    }
                    
                    return results;
                };

            Get[@"/(.*)"] = o => Response.AsRedirect("/", RedirectResponse.RedirectType.Permanent);
        }

        private List<dynamic> GetBoards()
        {
            var organization = trello.Organizations.WithId(Orginization);

            var boards = trello.Boards.ForOrganization(organization);

            return boards.OrderBy(x => x.Name)
                         .Select(x => new
                             {
                                 x.Id,
                                 x.Name,
                                 Selected = x.Id == "511cdb9c5984dad16a0021c9"
                             })
                         .ToList<dynamic>();
        }
        
        private List<dynamic> GetLists(string boardId)
        {
            var board = trello.Boards.WithId(boardId);

            return trello.Lists.ForBoard(board).ToList<dynamic>();
        }
    }
}