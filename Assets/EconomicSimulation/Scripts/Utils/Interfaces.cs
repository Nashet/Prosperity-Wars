using UnityEngine;
using UnityEditor;
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
    interface IStatisticable
    {
        void SetStatisticToZero();
    }
}