using UnityEngine;

namespace Nashet.Utils
{
    /// <summary>
    /// Describes ability to select & deselect some GameObject.
    /// Supposed to be a component
    /// </summary>
    public interface ISelector
    {
        void Select(GameObject someObject);
        void Deselect(GameObject someObject);
    }
}