using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Nancy;
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

                    var primaryBoard = boards.FirstOrDefault(x => x.Id == "511cdb9c5984dad16a0021c9");

                    dynamic model = new
                        {
                            Boards = boards,
                            Lists = GetLists(primaryBoard.Id)
                        };

                    return View["views/default", model];
                };

            Get["/api/boards"] = o => GetBoards();

            Get["/api/boards/{id}/lists"] = o => GetLists(o.id);

            Get["/api/boards/{boardId}/lists/{listId}/cards"] = o =>
                {
                    var list = trello.Lists.WithId(o.listId);

                    var cards = trello.Cards.ForList(list);
                    
                    return cards;
                };
        }

        private List<dynamic> GetBoards()
        {
            var organization = trello.Organizations.WithId(Orginization);

            var boards = trello.Boards.ForOrganization(organization);

            return boards.OrderBy(x => x.Name).Select(x => new {x.Id, x.Name}).ToList<dynamic>();
        }
        
        private List<dynamic> GetLists(string boardId)
        {
            var board = trello.Boards.WithId(boardId);

            return trello.Lists.ForBoard(board).ToList<dynamic>();
        }
    }
}