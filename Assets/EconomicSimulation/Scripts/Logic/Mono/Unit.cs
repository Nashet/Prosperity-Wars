using Nashet.UnityUIUtils;
using QPathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Unit : Hideable, IHasProvince//, IHasCountry//MonoBehaviour,
    {
        [SerializeField]
        private GameObject selectionPart;

        [SerializeField]
        private LineRenderer enemyDirection;
        [SerializeField]
        private float enemyDirectionScale = 0.6f;

        [SerializeField]
        private float unitPanelYOffset = -2f;

        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        public MeshCollider Collider;

        //[SerializeField]
        private LineRenderer lineRenderer;

        private GameObject unitPanelObject;
        public UnitPanel unitPanel;
        Animator m_Animator;

        private readonly static List<Unit> allUnits = new List<Unit>();

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
            unit.SetUnitShield(army.getOwner().Flag);
            unit.UpdateUnitShieldText(army.Province);
            unit.enemyDirection.enabled = unit.Province.Country != Game.Player;
            return unit;
        }

        public void Select()
        {
            selectionPart.SetActive(true);
            ArmiesSelectionWindow.Get.Show();
        }

        public void Deselect()
        {
            selectionPart.SetActive(false);
            ArmiesSelectionWindow.Get.Refresh();
        }

        private void SetUnitShield(Texture2D flag)
        {
            var panelPosition = gameObject.transform.position;
            panelPosition.y += unitPanelYOffset;
            panelPosition.z = -1f;
            unitPanelObject.transform.position = panelPosition;
            unitPanel.SetFlag(flag);
            //UpdateUnitShieldText(army.Province);

            meshRenderer.material = LinksManager.Get.defaultUnitSymbol;            
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

        public static Unit FindByID(int meshNumber)
        {
            return allUnits.Find(x => Int32.Parse(x.name) == meshNumber);
        }

        public override string ToString()
        {
            return "Army";
        }

        public void UpdateUnitShieldText(Province province)//static
        {
            int count = 0;
            //var sb = new StringBuilder();
            int size = 0;
            foreach (var item in province.AllStandingArmies())
            {
                size += item.getSize();
                count++;
            }
            if (count > 1)
                unitPanel.SetText(Army.SizeToString(size) + ":" + count);
            else
                unitPanel.SetText(Army.SizeToString(size));
        }


        public static void RedrawAll()
        {
            foreach (var province in Game.provincesToRedrawArmies)
            {
                foreach (var army in province.AllStandingArmies())
                //if (army.unit != null)
                {
                    army.unit.Province = province;
                    army.unit.transform.position = province.Position;// here it says that unit is destroyed
                    if (army.Path == null)
                    {
                        army.unit.Stop();
                    }
                    else
                    {
                        army.unit.Move(army.Path);
                    }
                    army.unit.SetUnitShield(army.getOwner().Flag);
                    army.unit.UpdateUnitShieldText(army.Province);
                }
                //else
                //    Debug.Log("Bugged army " + army);
                if (province.AllStandingArmies().Count() > 1)
                    for (int i = 0; i < province.AllStandingArmies().Count() - 1; i++)
                    {
                        province.AllStandingArmies().ElementAt(i).unit.unitPanel.Hide();
                    }
                else if (province.AllStandingArmies().Count() == 1)
                    province.AllStandingArmies().ElementAt(0).unit.unitPanel.Show();
            }
            Game.provincesToRedrawArmies.Clear();
        }
       
        private Vector3[] GetVector3Nodes(List<Node> path)
        {
            Vector3[] array = new Vector3[path.Count + 1];
            for (int i = 0; i < path.Count; i++)
            {
                array[i + 1] = path[i].Province.Position;
                array[i + 1].z = -2f;
            }
            return array;
        }

        private void Move(List<Node> nodes)
        {
            lineRenderer.positionCount = nodes.Count + 1;
            lineRenderer.SetPositions(GetVector3Nodes(nodes));
            lineRenderer.SetPosition(0, Province.Position);//currentProvince.getPosition()

            this.transform.LookAt(nodes[0].Province.Position, Vector3.back);
            if (m_Animator.gameObject.activeInHierarchy)
                m_Animator.SetFloat("Forward", 0.4f);//, 0.3f, Time.deltaTime
                                                     //if (where.armies.Count > 1)
                                                     //    for (int i = 0; i < where.armies.Count - 1; i++)
                                                     //    {
                                                     //        where.armies[i].unit.unitPanel.Hide();
                                                     //    }
                                                     //else
                                                     //    where.armies[0].unit.unitPanel.Show();
            enemyDirection.positionCount = 2;
            //todo must be fixed ssize
            var linePositions = GetVector3Nodes(nodes);
            linePositions[0] = Province.Position;
            linePositions[1] = Vector3.LerpUnclamped(linePositions[1], linePositions[0], enemyDirectionScale);
            enemyDirection.SetPositions(linePositions);
       
            enemyDirection.enabled = true;

        }

        private void Stop()
        {
            lineRenderer.positionCount = 0;
            if (m_Animator.gameObject.activeInHierarchy)
                m_Animator.SetFloat("Forward", 0f);
            this.transform.eulerAngles = new Vector3(270f, 0f, 0f);
            //SetUnitPanel(null);

            enemyDirection.enabled = false;
        }

        private void OnDestroy()
        {
            Destroy(unitPanelObject);
        }
    }
}