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

    internal interface IStatisticable
    {
        void SetStatisticToZero();
    }
}