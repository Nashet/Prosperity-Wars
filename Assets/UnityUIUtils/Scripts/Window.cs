namespace Nashet.UnityUIUtils
{
    /// <summary>
    /// Represent UI object that can be refreshed and hidden
    /// </summary>
    public abstract class Window : Hideable, IRefreshable
    {
        public abstract void Refresh();

        public override void Show()
        {
            base.Show();
            Refresh();
        }
    }
}