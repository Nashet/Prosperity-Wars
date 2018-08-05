using UnityEngine;

namespace Nashet.Utils
{
    public interface INameable
    {
        string FullName { get; }
        string ShortName { get; }
    }

    public interface ISortableName
    {
        float GetNameWeight();
    }

    public interface IStatisticable
    {
        void SetStatisticToZero();
    }
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