using UnityEngine;

namespace Nashet.UnitSelection
{    
    public interface ISelectableObject
    {
        Vector3 Position { get; }
        void Select();
        void Deselect();
        bool IsSelected { get; }
    }
}