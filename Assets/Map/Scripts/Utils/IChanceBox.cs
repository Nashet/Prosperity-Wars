namespace Nashet.Map.Utils
{
	public interface IChanceBox<T>
	{
		void Add(T obj, float chance);
		T GetRandom();
		void Initiate();
	}
}