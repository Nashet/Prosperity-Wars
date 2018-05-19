using Nashet.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Nashet.EconomicSimulation
{


    public class PreArmy
    {
        //protected Province destination;

        //[SerializeField]
        //protected Province currentProvince;

        [SerializeField]
        protected Path path;

        protected readonly Staff owner;

        //protected Unit unit;

        public PreArmy(Staff owner, Province where)
        {
            Province = where;
            owner.addArmy(this);
            this.owner = owner;

            

            //unit = unitObject.GetComponent<Unit>();
            
            

            Redraw();
        }
        internal void SendTo(Province destinationProvince)
        {
            if (destinationProvince != null)
                path = World.Get.graph.GetShortestPath(Province, destinationProvince, x => x.Country == Province.Country || x.Country == World.UncolonizedLand);
            Redraw();
        }

        public void Select()
        {
            if (!Game.selectedUnits.Contains(this))
            {
                Game.selectedUnits.Add(this);
                Province.SelectUnit();
                //IsSelected
            }
        }

        public void DeSelect()
        {
            Game.selectedUnits.Remove(this);
            //selectionPart.SetActive(false);
            Province.SelectUnit();
        }
        public Vector3 Position
        {
            get { return Province.getPosition(); }
        }

        public Province Province { get; internal set; }

        protected void Redraw()
        {
            //throw new NotImplementedException();
        }
    }
}
