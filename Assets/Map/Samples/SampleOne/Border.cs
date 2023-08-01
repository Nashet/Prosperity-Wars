namespace Nashet.Map.Examples
{
	public class Border
	{
		public bool IsPassable;
		public bool IsRiverBorder;
		public Province Province;

		public Border(Province province)
		{
			Province = province;
			IsPassable = true;
		}
	}
}