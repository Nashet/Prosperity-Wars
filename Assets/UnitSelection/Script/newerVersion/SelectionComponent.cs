
using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
namespace Nashet.UnitSelection
{
    public class SelectionComponentNewer : MonoBehaviour
    {
        private bool isFrameSelectionInProgress = false;
        private Vector3 selectionFrameCornerPosition;        

        [SerializeField]
        [Header("Used to edit unit selection")]
        private KeyCode selectionEditKey;


        protected List<ISelectableObject> selectedUnits = new List<ISelectableObject>();
        //[SerializeField] protected List<ISelectable> selectableObjects = new List<ISelectable>(); 

        private void Start()
        {
            
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())//!hovering over UI) 
                {
                    OnClickedOnObject();
                }
                if (isFrameSelectionInProgress)
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
            if (selectedUnits.Count != 0 && Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }
        }

        private void StartFrameSelection()
        {
            isFrameSelectionInProgress = true;
            selectionFrameCornerPosition = Input.mousePosition;
        }

        private void EndFrameSelection()
        {
            if (selectionFrameCornerPosition != Input.mousePosition)
            {
                if (!Input.GetKey(selectionEditKey))
                {
                    selectedUnits.ForEach(x => x.Deselect());
                    selectedUnits.Clear();
                }

                foreach (var selectableObject in GetComponentsInChildren<ISelectableObject>())
                {
                    if (IsWithinSelectionBounds(selectableObject.Position))
                    {
                        selectableObject.Select();
                        selectedUnits.Add(selectableObject);
                    }
                }
            }
            isFrameSelectionInProgress = false;
        }

        private void OnClickedOnObject()
        {
            bool clickedOnSelectableObject = false;
            var gameObject = GetRayCastObject();
            if (gameObject != null)
            {
                var selectable = gameObject.GetComponent<ISelectableObject>();
                if (selectable != null)
                {
                    clickedOnSelectableObject = true;
                    if (Input.GetKey(selectionEditKey)) // add/remove to selection
                    {
                        if (selectedUnits.Contains(selectable))
                        {
                            selectable.Deselect();
                            selectedUnits.Remove(selectable);
                        }
                        else
                        {
                            selectable.Select();
                            selectedUnits.Add(selectable);
                        }
                    }
                    else
                    {
                        selectedUnits.ForEach(x => x.Deselect());
                        selectedUnits.Clear();
                        if (selectable.IsSelected)
                        {
                            selectable.Deselect();
                        }
                        else
                        {
                            selectable.Select();
                            selectedUnits.Add(selectable);
                        }
                    }

                }

            }

            // don't deselect units if selectionEditKey is pressed
            if (!clickedOnSelectableObject && !Input.GetKey(selectionEditKey))
            {
                selectedUnits.ForEach(x => x.Deselect());
                selectedUnits.Clear();
            }
        }

        public event EventHandler GoToCommandMade;

        private void HandleRightClick()
        {
            if (selectedUnits.Count > 0)
            {
                var gameObject = GetRayCastObject();
                if (gameObject != null)
                {
                    var movementTarget = gameObject.GetComponent<IMovementTarget>();
                    if (movementTarget != null)
                    {
                        EventHandler handler = GoToCommandMade;
                        if (handler != null)
                        {

                            //var unitList = new List<IUnitView>();
                            //foreach (var item in selectedUnits)
                            //{
                            //    var unit = item as UnitVisualizator;
                            //    if (unit != null)
                            //        unitList.Add(unit.serverUnit);
                            //}
                            //handler(this, new GoToCommandMadeArgs(movementTarget.Position, unitList));
                        }

                    }
                }
            }
        }

        //public class GoToCommandMadeArgs : EventArgs
        //{
        //    public Position Destination { get; protected set; }
        //    public List<IUnitView> list;

        //    public GoToCommandMadeArgs(Position destination, List<IUnitView> list)
        //    {
        //        Destination = destination;
        //        this.list = list;
        //    }
        //}

        protected bool IsWithinSelectionBounds(Vector3 position)
        {
            if (!isFrameSelectionInProgress)
                return false;

            var camera = Camera.main;
            var viewportBounds = Utils.GetViewportBounds(camera, selectionFrameCornerPosition, Input.mousePosition);
            return viewportBounds.Contains(camera.WorldToViewportPoint(position));
        }

        private void OnGUI()
        {
            if (isFrameSelectionInProgress)
            {
                // Create a rect from both mouse positions
                var rect = Utils.GetScreenRect(selectionFrameCornerPosition, Input.mousePosition);
                Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));

                //// Left example
                //Utils.DrawScreenRectBorder(new Rect(32, 32, 256, 128), 2, Color.green);
                //// Right example
                //Utils.DrawScreenRect(new Rect(320, 32, 256, 128), new Color(0.8f, 0.8f, 0.95f, 0.25f));
                //Utils.DrawScreenRectBorder(new Rect(320, 32, 256, 128), 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        protected static GameObject GetRayCastObject()
        {
            RaycastHit hit;

            if (EventSystem.current.IsPointerOverGameObject())
                return null; //hovering over UI
            else
            {
                if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                    return null; // emptiness
            }
            return hit.collider.gameObject;
        }
    }
}
