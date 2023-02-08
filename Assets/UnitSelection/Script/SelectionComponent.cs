using Nashet.EconomicSimulation;
using Nashet.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Nashet.UnitSelection
{
    public class SelectionComponent : MonoBehaviour
    {
        [SerializeField] private KeyCode AdditionKey;

        private bool isFrameSelecting = false;
        private Vector3 selectionFrameMousePositionStart;

        //public GameObject selectionCirclePrefab;
        private static Camera camera; // it's OK
        private int nextArmyToSelect;

        private void Start()
        {
            camera = GetComponent<Camera>();
        }

        //TODO need to get rid of Update()
        private DateTime last = new DateTime(1991, 12, 24);
        private void Update()
        {
            var endedSelection = HandleFrameSelection();
            //if (Input.touchCount > 1)
            //    return;
            // if (!endedSelection)
            HandleUnitOrProvinceClick();
            HandleSendUnitTo();

            // Highlight all objects within the selection box
            //if (isSelecting)
            //{
            //    foreach (var selectableObject in FindObjectsOfType<SelectableUnitComponent>())
            //    {
            //        if (IsWithinSelectionBounds(selectableObject.gameObject))
            //        {
            //            if (selectableObject.selectionCircle == null)
            //            {
            //                selectableObject.selectionCircle = Instantiate(selectionCirclePrefab);
            //                selectableObject.selectionCircle.transform.SetParent(selectableObject.transform, false);
            //                selectableObject.selectionCircle.transform.eulerAngles = new Vector3(90, 0, 0);
            //            }
            //        }
            //        else
            //        {
            //            if (selectableObject.selectionCircle != null)
            //            {
            //                Destroy(selectableObject.selectionCircle.gameObject);
            //                selectableObject.selectionCircle = null;
            //            }
            //        }
            //    }
            //}
            Game.previoslySelectedProvince = Game.selectedProvince;
        }

        private void HandleSendUnitTo()
        {
            // MOUSE RIGHT BUTTON clicked or Left clicked after SendButon clicked
            if (!Game.selectedArmies.IsEmpty() && (Input.GetMouseButtonUp(1) || Game.isInSendArmyMode && Input.GetMouseButtonUp(0)))
            {
                last = DateTime.Now;
                SendUnitTo();
            }
        }

        private void HandleUnitOrProvinceClick()
        {
            //left mouse button
            if (Input.GetMouseButtonUp(0) && !Game.isInSendArmyMode && (DateTime.Now - last).Seconds > 0.1f)// !ignoreIreviousIsInSendArmyModeState
            {
                if (!EventSystem.current.IsPointerOverGameObject())//!hovering over UI) 
                {
                    var collider = getRayCastMeshNumber();
                    if (collider != null)
                    {
                        int provinceNumber = Province.FindByCollider(collider);
                        if (provinceNumber > 0)
                        {
                            if (!isFrameSelecting)
                                MainCamera.selectProvince(provinceNumber);
                            if (!Input.GetKey(AdditionKey)) // don't de select units if AdditionKey is pressed
                                Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                        }
                        else
                        {
                            var unit = collider.transform.parent.GetComponent<Unit>();
                            if (unit != null)
                            {
                                var army = unit.Province.AllStandingArmies().Where(x => x.getOwner() == Game.Player).Next(ref nextArmyToSelect);
                                if (army != null)
                                {
                                    if (Input.GetKey(AdditionKey))
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
                        if (!Input.GetKey(AdditionKey))
                            Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                    }
                }
            }
        }

        private bool HandleFrameSelection()
        {
            // If we press the left mouse button, begin selection and remember the location of the mouse
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !Game.isInSendArmyMode && !isFrameSelecting)
            {
                StartFrameSelection();
            }

            if (Input.GetMouseButtonUp(0) && isFrameSelecting)
            {
                EndFrameSelection();// If we let go of the left mouse button, end selection
                return true;
            }
            return false;
        }

        private void SendUnitTo()
        {
            var collider = getRayCastMeshNumber();
            if (collider != null)
            {
                Province sendToPovince = null;
                int meshNumber = Province.FindByCollider(collider);
                if (meshNumber > 0) // send armies to another province
                    sendToPovince = World.FindProvince(meshNumber);
                else // better do here sort of collider layer, hitting provinces only
                {
                    var unit = collider.transform.GetComponent<Unit>();
                    if (unit != null)
                    {
                        sendToPovince = unit.Province;
                    }
                }
                var addPath = Input.GetKey(AdditionKey);

                foreach (var item in Game.selectedArmies)
                {
                    if (addPath)
                        item.AddToPath(sendToPovince);
                    else
                        item.SetPathTo(sendToPovince);
                    Game.provincesToRedrawArmies.Add(item.Province);
                }
                //Unit.RedrawAll();
                Game.ChangeIsInSendArmyMode(false);
            }
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
                if (!Input.GetKey(AdditionKey))
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