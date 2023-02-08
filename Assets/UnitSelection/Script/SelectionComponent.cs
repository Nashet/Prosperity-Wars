using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Nashet.UnitSelection
{
    public class SelectionComponent : MonoBehaviour
    {
        public Action<SelectionData> OnEntitySelected;
        public Action<SelectionData> OnProvinceSelected;
        public static Func<int, IEnumerable<Collider>> ArmiesGetter;

        private bool isFrameSelecting = false;
        private Vector3 selectionFrameMousePositionStart;
        private ulong buttonHoldTicks;

        //TODO need to get rid of Update()        
        private void Update()
        {
            HandleUnitOrProvinceClick();
            HandleFrameSelection();

            if (Input.GetMouseButton(0))
            {
                buttonHoldTicks++;
            }
            else
            {
                buttonHoldTicks = 0;
            }
        }

        private void HandleUnitOrProvinceClick()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())//!hovering over UI) 
                {
                    var collider = Utils.getRayCastMeshNumber();
                    if (collider == null)
                    {
                        OnEntitySelected?.Invoke(null);
                        OnProvinceSelected?.Invoke(null);
                    }
                    else
                    {
                        OnProvinceSelected?.Invoke(new SelectionData(collider));
                        OnEntitySelected?.Invoke(new SelectionData(collider));
                    }
                }
            }
        }

        private void HandleFrameSelection()
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject() && !isFrameSelecting)
            {
                if (buttonHoldTicks > 4)
                    StartFrameSelection(); // count started only if holded some time
            }

            if (Input.GetMouseButtonUp(0) && isFrameSelecting)
            {
                EndFrameSelection();// If we let go of the left mouse button, end selection
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
                OnEntitySelected?.Invoke(new SelectionData(ArmiesGetter(-1).Where(x => IsWithinSelectionBounds(x.transform.position))));
            }
            isFrameSelecting = false;
        }

        private bool IsWithinSelectionBounds(Vector3 position)
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
    }
}