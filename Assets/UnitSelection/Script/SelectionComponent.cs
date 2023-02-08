using Nashet.EconomicSimulation;
using Nashet.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Nashet.UnitSelection
{
    public class SelectionComponent : MonoBehaviour
    {
        private bool isFrameSelecting = false;
        private Vector3 selectionFrameMousePositionStart;

        //public GameObject selectionCirclePrefab;
        private static Camera camera; // it's OK
        private int nextArmyToSelect;
        private DateTime lastClickTime = new DateTime(1991, 12, 24);
        private int holded;

        private void Start()
        {
            camera = GetComponent<Camera>();
        }

        //TODO need to get rid of Update()        
        private void Update()
        {
            HandleUnitOrProvinceClick();
            var endedSelection = HandleFrameSelection(); // breaks previous
          

            if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(0))
            {
                lastClickTime = DateTime.Now;
            }

            if (Input.GetMouseButton(0))
            {
                holded++;
            }
            else
            {
                holded = 0;
            }
          
            Game.previoslySelectedProvince = Game.selectedProvince;
        }

        public static Unit GetUnit(Collider collider)
        {
            return collider.transform.parent.GetComponent<Unit>();
        }

        private void HandleUnitOrProvinceClick()
        {
            //left mouse button
            if (Input.GetMouseButtonUp(0) && !Game.isInSendArmyMode)// !ignoreIreviousIsInSendArmyModeState // && (DateTime.Now - lastClickTime).Seconds > 0.1f
            {
                if (!EventSystem.current.IsPointerOverGameObject())//!hovering over UI) 
                {
                    var collider = getRayCastMeshNumber();
                    if (collider != null)
                    {
                        int provinceNumber = Province.FindByCollider(collider);
                        if (provinceNumber > 0)
                        {
                            //if (!isFrameSelecting)
                                MainCamera.selectProvince(provinceNumber);
                            if (!Input.GetKey(LinksManager.Get.AdditionKey)) // don't de select units if AdditionKey is pressed
                                Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                        }
                        else
                        {
                            var unit = GetUnit(collider);
                            if (unit != null)
                            {
                                var army = unit.Province.AllStandingArmies().Where(x => x.getOwner() == Game.Player).Next(ref nextArmyToSelect);
                                if (army != null)
                                {
                                    if (Input.GetKey(LinksManager.Get.AdditionKey))
                                    {
                                        if (Game.selectedArmies.Contains(army))
                                            army.Deselect();
                                        else
                                            army.Select();
                                    }
                                    else
                                    {
                                        if (Game.selectedArmies.Count > 0)
                                        {
                                            Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                                        }
                                        army.Select();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MainCamera.selectProvince(-1);
                        if (!Input.GetKey(LinksManager.Get.AdditionKey))
                            Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                    }
                }
            }
        }

        private bool HandleFrameSelection()
        {
            // If we press the left mouse button, begin selection and remember the location of the mouse
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()
                && !Game.isInSendArmyMode && !isFrameSelecting )// 
            {
                if (holded > 4)
                    StartFrameSelection(); // coun started only if holded some time
            }

            if (Input.GetMouseButtonUp(0) && isFrameSelecting)
            {
                EndFrameSelection();// If we let go of the left mouse button, end selection
                return true;
            }
            return false;
        }

        private void StartFrameSelection()
        {
            isFrameSelecting = true;
            selectionFrameMousePositionStart = Input.mousePosition;
        }

        private void EndFrameSelection()
        {
            if (selectionFrameMousePositionStart != Input.mousePosition)
            {
                if (!Input.GetKey(LinksManager.Get.AdditionKey))
                    Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                foreach (var selectableObject in Game.Player.AllArmies())
                {
                    if (IsWithinSelectionBounds(selectableObject.Position))
                    {
                        selectableObject.Select();
                    }
                }

                //var sb = new StringBuilder();
                //sb.AppendLine(string.Format("Selecting [{0}] Units", selectedObjects.Count));
                //foreach (var selectedObject in selectedObjects)
                //    sb.AppendLine("-> " + selectedObject.gameObject.name);
                //Debug.Log(sb.ToString());
            }
            isFrameSelecting = false;
        }

        public bool IsWithinSelectionBounds(Vector3 position)
        {
            if (!isFrameSelecting)
                return false;

            var camera = Camera.main;
            var viewportBounds = Utils.GetViewportBounds(camera, selectionFrameMousePositionStart, Input.mousePosition);
            return viewportBounds.Contains(camera.WorldToViewportPoint(position));
        }

        private void OnGUI()
        {
            if (isFrameSelecting)
            {
                // Create a rect from both mouse positions
                var rect = Utils.GetScreenRect(selectionFrameMousePositionStart, Input.mousePosition);
                Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));

                //// Left example
                //Utils.DrawScreenRectBorder(new Rect(32, 32, 256, 128), 2, Color.green);
                //// Right example
                //Utils.DrawScreenRect(new Rect(320, 32, 256, 128), new Color(0.8f, 0.8f, 0.95f, 0.25f));
                //Utils.DrawScreenRectBorder(new Rect(320, 32, 256, 128), 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        private static bool IsPointerOverGameObject()
        {
            //check touch. priorities on touches
            if (Input.touchCount > 0)
            {
                return (Input.touches[0].phase == TouchPhase.Ended && EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId));
            }

            //check mouse
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            return false;
        }

        // remake it to return mesh collider, on which will be chosen object
        public static Collider getRayCastMeshNumber()
        {
            RaycastHit hit;

            var isHovering = IsPointerOverGameObject();
            if (isHovering)
                return null;// -3; //hovering over UI
            else
            {
                if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit))
                    return null;// -1;
            }
            return hit.collider;
        }
    }
}