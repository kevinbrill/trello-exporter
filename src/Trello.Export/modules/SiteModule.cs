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

                    return View["views/default", boards];
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

            return new List<dynamic>
                {
                    from b in boards
                    select new
                        {
                            b.Id,
                            b.Name,
                        }
                };
        }
    }
}