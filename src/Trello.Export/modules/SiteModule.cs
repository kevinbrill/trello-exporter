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

        public SiteModule()
        {
            trello = new TrelloNet.Trello(ConfigurationManager.AppSettings["AuthKey"]);
            trello.Authorize(ConfigurationManager.AppSettings["AuthToken"]);

            Get["/"] = o =>
                {
                    var boards = GetBoards();

                    dynamic model = new
                        {
                            Boards = boards
                        };

                    return View["views/default", model];
                };

            Get["/api/boards"] = o => GetBoards();

            Get["/api/boards/{id}/lists"] = o =>
                {
                    var board = trello.Boards.WithId(o.id);

                    return trello.Lists.ForBoard(board);
                };

            Get["/api/boards/{boardId}/lists/{listId}/cards"] = o =>
                {
                    var list = trello.Lists.WithId(o.listId);

                    var cards = trello.Cards.ForList(list);
                    
                    return cards;
                };
        }

        private List<dynamic> GetBoards()
        {
            var organization = trello.Organizations.WithId("researchteam2");

            var boards = trello.Boards.ForOrganization(organization);

            return boards.Select(x => new {x.Id, x.Name}).ToList<dynamic>();
        }
    }
}