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
        private bool isSelecting = false;
        private Vector3 mousePosition1;

        //public GameObject selectionCirclePrefab;
        private static Camera camera; // it's OK
        [SerializeField]
        private KeyCode AdditionKey;
        private int nextArmyToSelect;

        private void Start()
        {
            camera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())//!hovering over UI) 
                {
                    SelectUnitOrProvince();
                }
                if (isSelecting)
                    EndFrameSelection();// If we let go of the left mouse button, end selection
            }
            else
            {
                // If we press the left mouse button, begin selection and remember the location of the mouse
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    StartFrameSelection();
                }
            }

            // MOUSE RIGHT BUTTON clicked
            if (!Game.selectedArmies.IsEmpty() && Input.GetMouseButtonDown(1))
            {
                SendUnitTo();
            }
           

            Game.previoslySelectedProvince = Game.selectedProvince;
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
            }
        }
        private void StartFrameSelection()
        {
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
        }

        private void EndFrameSelection()
        {
            if (mousePosition1 != Input.mousePosition)
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
            isSelecting = false;
        }

        private void SelectUnitOrProvince()
        {
            var collider = getRayCastMeshNumber();
            if (collider != null)
            {
                int provinceNumber = Province.FindByCollider(collider);
                if (provinceNumber > 0)
                {
                    MainCamera.selectProvince(provinceNumber);
                    if (!Input.GetKey(AdditionKey)) // don't de select units if shift is pressed
                        Game.selectedArmies.ToList().PerformAction(x => x.Deselect());
                }
                else
                {
                    var unit = collider.transform.GetComponent<Unit>();
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
        public bool IsWithinSelectionBounds(Vector3 position)
        {
            if (!isSelecting)
                return false;

            var camera = Camera.main;
            var viewportBounds = Utils.GetViewportBounds(camera, mousePosition1, Input.mousePosition);
            return viewportBounds.Contains(camera.WorldToViewportPoint(position));
        }

        private void OnGUI()
        {
            if (isSelecting)
            {
                // Create a rect from both mouse positions
                var rect = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
                Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));

                //// Left example
                //Utils.DrawScreenRectBorder(new Rect(32, 32, 256, 128), 2, Color.green);
                //// Right example
                //Utils.DrawScreenRect(new Rect(320, 32, 256, 128), new Color(0.8f, 0.8f, 0.95f, 0.25f));
                //Utils.DrawScreenRectBorder(new Rect(320, 32, 256, 128), 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }
        // remake it to return mesh collider, on which will be chosen object
        public static Collider getRayCastMeshNumber()
        {
            RaycastHit hit;

            if (EventSystem.current.IsPointerOverGameObject())
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