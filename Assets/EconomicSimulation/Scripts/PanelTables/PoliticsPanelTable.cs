using System.Collections.Generic;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    public class PoliticsPanelTable : UITableNew<AbstrRefrm>
    {
        protected override IEnumerable<AbstrRefrm> ContentSelector()
        {
            return Game.Player.reforms;
        }

        protected override void AddRow(AbstrRefrm reform, int number)
        {
            // Adding reform name
            AddCell(reform.ToString(), reform);

            ////Adding Status
            AddCell(reform.ToString(), reform);

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