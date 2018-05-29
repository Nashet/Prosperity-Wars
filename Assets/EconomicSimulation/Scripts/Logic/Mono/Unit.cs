using Nashet.EconomicSimulation;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Nashet.EconomicSimulation
{
    public class Unit : MonoBehaviour, IHasProvince//, IHasCountry
    {

        [SerializeField]
        private GameObject selectionPart;

        [SerializeField]
        private float unitPanelYOffset = -2f;

        //[SerializeField]
        private LineRenderer lineRenderer;

        private GameObject unitPanelObject;
        public UnitPanel unitPanel;
        Animator m_Animator;


        private readonly static List<Unit> allUnits = new List<Unit>();

        public Army NextArmy
        {
            get
            {
                return Game.Player.getAllArmies().Where(x => x.Province == Province).Random();
            }
        }

        public Province Province { get; private set; }

        //public Country Country { get; private set; }

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
            allUnits.Add(this);
            lineRenderer = selectionPart.GetComponent<LineRenderer>();
            selectionPart.SetActive(false);


            gameObject.SetActive(true);
            unitPanelObject = Instantiate(LinksManager.Get.UnitPanelPrefab, LinksManager.Get.WorldSpaceCanvas);
            unitPanel = unitPanelObject.GetComponent<UnitPanel>();
            //unitPanel.Set(currentProvince.Country.Flag);
            //SetUnitPanel();
        }
        public static Unit Create(Army army)
        {
            var unitObject = GameObject.Instantiate(LinksManager.Get.UnitPrefab, LinksManager.Get.ArmiesHolder.transform);
            unitObject.name = army.FullName; //(World.GetAllProvinces().Count() + UnityEngine.Random.Range(0, 2000)).ToString();
            //army.getOwner()+"'s "
            unitObject.transform.position = army.Position;

            var unit = unitObject.GetComponent<Unit>();
            unit.Province = army.Province;
            //unit.Country = army.getOwner().Country;
            unit.SetUnitShield(army);

            return unit;
        }

        internal void Select()
        {

            selectionPart.SetActive(true);
            ArmiesSelectionWindow.Get.Show();
        }

        internal void DeSelect()
        {
            selectionPart.SetActive(false);
            ArmiesSelectionWindow.Get.Refresh();
        }

        private void SetUnitShield(Army army)
        {
            var panelPosition = gameObject.transform.position;
            panelPosition.y += unitPanelYOffset;
            panelPosition.z = -1f;
            unitPanelObject.transform.position = panelPosition;
            UpdateUnitShield(army.Province);
            if (army != null)
                unitPanel.SetFlag(army.getOwner().Country.Flag);
        }
        public static IEnumerable<Unit> AllUnits()
        {
            foreach (var item in allUnits)
            {
                yield return item;
            }
        }
        public void Simulate()
        { }

        internal static Unit FindByID(int meshNumber)
        {
            return allUnits.Find(x => Int32.Parse(x.name) == meshNumber);
        }




        public override string ToString()
        {
            return "Army";
        }

        internal void UpdateUnitShield(Province province)//static
        {
            int count = 0;
            //var sb = new StringBuilder();
            int size = 0;
            foreach (var item in province.standingArmies)
            {
                size += item.getSize();
                count++;
            }
            if (count > 1)
                unitPanel.SetText(Army.SizeToString(size) + ":" + count);
            else
                unitPanel.SetText(Army.SizeToString(size));
        }


        internal static void RedrawAll()
        {
            foreach (var province in Game.provincesToRedraw)
            {
                foreach (var army in province.standingArmies)
                {
                    army.unit.Province = province;
                    army.unit.transform.position = province.getPosition();
                    if (army.Path == null)
                    {
                        army.unit.Stop();
                    }
                    else
                    {
                        army.unit.Move(army.Path);
                    }
                    army.unit.SetUnitShield(army);
                }
                if (province.standingArmies.Count > 1)
                    for (int i = 0; i < province.standingArmies.Count - 1; i++)
                    {
                        province.standingArmies[i].unit.unitPanel.Hide();
                    }
                else if (province.standingArmies.Count == 1)
                    province.standingArmies[0].unit.unitPanel.Show();
            }
            Game.provincesToRedraw.Clear();
        }

        private void Move(Path path)
        {
            lineRenderer.positionCount = path.nodes.Count + 1;
            lineRenderer.SetPositions(path.GetVector3Nodes());
            lineRenderer.SetPosition(0, Province.getPosition());//currentProvince.getPosition()
            this.transform.LookAt(path.nodes[0].Province.getPosition(), Vector3.back);
            m_Animator.SetFloat("Forward", 0.4f);//, 0.3f, Time.deltaTime
                                                 //if (where.armies.Count > 1)
                                                 //    for (int i = 0; i < where.armies.Count - 1; i++)
                                                 //    {
                                                 //        where.armies[i].unit.unitPanel.Hide();
                                                 //    }
                                                 //else
                                                 //    where.armies[0].unit.unitPanel.Show();

        }

        private void Stop()
        {
            lineRenderer.positionCount = 0;
            m_Animator.SetFloat("Forward", 0f);

            //SetUnitPanel(null);
        }

        private void OnDestroy()
        {
            Destroy(unitPanelObject);
        }
    }
}