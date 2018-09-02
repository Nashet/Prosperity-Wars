using System.Collections.Generic;
using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class PoliticsPanelTable : UITableNew<AbstractReform>
    {
        protected override IEnumerable<AbstractReform> ContentSelector()
        {
            return Game.Player.Politics.AllReforms;
        }

        protected override void AddRow(AbstractReform reform, int number)
        {
            // Adding reform name
            AddCell(reform.ShortName, reform);

            ////Adding Status
            AddCell(reform.Value.ToString(), reform);

            ////Adding Can change possibility
            //if (next.canChange())
            //    AddButton("Yep", next);
            //else
            //    AddButton("Nope", next);
        }

        protected override void AddHeader()
        {
            // Adding reform name
            AddCell("Reform");

            ////Adding Status
            AddCell("Status");

            ////Adding Can change possibility
            // AddButton("Can change", null);
        }
    }
}