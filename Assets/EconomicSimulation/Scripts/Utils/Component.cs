namespace Nashet.Utils
{
    /// <summary>
    /// General class for my components implementing composition
    /// </summary>    
    public abstract class Component<T>
    {
        protected T owner;

        public Component(T owner)
        {
            this.owner = owner;
        }
    }
}
