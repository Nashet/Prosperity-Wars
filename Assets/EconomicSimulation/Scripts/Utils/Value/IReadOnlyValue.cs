namespace Nashet.ValueSpace
{
    public interface IReadOnlyValue
    {
        float get();

        bool isBiggerOrEqual(ReadOnlyValue invalue);

        bool isBiggerThan(ReadOnlyValue invalue);

        bool isBiggerThan(ReadOnlyValue invalue, ReadOnlyValue barrier);

        bool IsEqual(ReadOnlyValue invalue);

        bool isNotZero();

        bool isSmallerOrEqual(ReadOnlyValue invalue);

        bool isSmallerThan(ReadOnlyValue invalue);

        bool isZero();

        string ToString();
    }
}
