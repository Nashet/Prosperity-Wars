using Nashet.EconomicSimulation;
using Nashet.UnityUIUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Nashet.EconomicSimulation
{
    public class Unit : MonoBehaviour
    {
        [SerializeField]
        private Province currentProvince;

        [SerializeField]
        private int ID;

        [SerializeField]
        private GameObject selectionPart;

        [SerializeField]
        private Path path;

        [SerializeField]
        private float unitPanelYOffset = -2f;

        //[SerializeField]
        private LineRenderer lineRenderer;

        private GameObject unitPanelObject;
        private UnitPanel unitPanel;
        Animator m_Animator;

        private readonly static List<Unit> allUnits = new List<Unit>();
        void Start()
        {
            m_Animator = GetComponent<Animator>();
            allUnits.Add(this);
            lineRenderer = selectionPart.GetComponent<LineRenderer>();
            selectionPart.SetActive(false);
            World.DayPassed += DayPassed;
            Province.OwnerChanged += CheckPathOnProvinceOwnerChanged;
            gameObject.SetActive(true);
            unitPanelObject = Instantiate(LinksManager.Get.UnitPanelPrefab, LinksManager.Get.WorldSpaceCanvas);
            unitPanel = unitPanelObject.GetComponent<UnitPanel>();
            unitPanel.Set(currentProvince.Country.Flag);
            SetUnitPanel();
        }

        private void SetUnitPanel()
        {
            var panelPosition = gameObject.transform.position;
            panelPosition.y += unitPanelYOffset;
            panelPosition.z = -1f;
            unitPanelObject.transform.position = panelPosition;
        }

        private void CheckPathOnProvinceOwnerChanged(object sender, Province.OwnerChangedEventArgs e)
        {
            if (e.oldOwner == currentProvince.Country && path != null && path.nodes.Any(x => x.Province == sender))//changed owner, probably, on our way
            {
                path = null;
                UpdateStatus();
                Message.NewMessage(this + " arrived!", "Commander, " + this + " stopped at " + currentProvince + " province", "Fine", false, currentProvince.getPosition());
            }

        }

        private void DayPassed(object sender, EventArgs e)
        {
            if (path != null)
            {
                if (path.nodes.Count > 0)
                {
                    currentProvince = path.nodes[0].Province;
                    path.nodes.RemoveAt(0);
                    transform.position = currentProvince.getPosition();
                    SetUnitPanel();
                    if (path.nodes.Count == 0)
                    {
                        path = null;
                        Message.NewMessage(this + " arrived!", "Commander, "+this + " arrived to " + currentProvince+ " province", "Fine", false, currentProvince.getPosition());
                    }
                    UpdateStatus();
                }
            }
        }

        /// <summary>
        /// initializer
        /// </summary>
        internal void SetPosition(Province province)
        {
            transform.position = province.getPosition();
            currentProvince = province;
        }

        internal void SendTo(Province destinationProvince)
        {
            path = World.Get.graph.GetShortestPath(currentProvince, destinationProvince, x => x.Country == currentProvince.Country);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (path == null)
            {
                lineRenderer.positionCount = 0;
                m_Animator.SetFloat("Forward", 0f);
            }
            else
            {
                lineRenderer.positionCount = path.nodes.Count + 1;
                lineRenderer.SetPositions(path.GetVector3Nodes());
                lineRenderer.SetPosition(0, currentProvince.getPosition());
                this.transform.LookAt(path.nodes[0].Province.getPosition(), Vector3.back);
                m_Animator.SetFloat("Forward", 0.4f);//, 0.3f, Time.deltaTime
            }
        }
        public void Simulate()
        { }

        internal static Unit FindByID(int meshNumber)
        {
            return allUnits.Find(x => Int32.Parse(x.name) == meshNumber);
        }


        public void Select()
        {
            Game.selectedUnits.Add(this);
            selectionPart.SetActive(true);
            
            ArmiesSelectionWindow.Get.Show();
        }

        public void DeSelect()
        {
            Game.selectedUnits.Remove(this);
            selectionPart.SetActive(false);
            ArmiesSelectionWindow.Get.Refresh();
        }

        public override string ToString()
        {
            return "Army";
        }
    }
}