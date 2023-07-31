namespace Nashet.UnitSelection
{
	public interface ISelectionComponent
	{
		event EntityClickedDelegate OnEntityClicked;
		event EntityClickedDelegate OnProvinceClicked;
	}
}