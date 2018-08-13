namespace Nashet.Utils
{
    public abstract class Name : INameable, ISortableName
    {
        private readonly string name;
        private readonly float nameWeight;

        protected Name(string name)
        {
            this.name = name;
            if (name != null)
                nameWeight = name.GetWeight();
        }

        public float NameWeight
        {
            get
            {
                return nameWeight;
            }
        }

        //public string getShortName()
        //{
        //    return name;
        //}
        public virtual string ShortName
        {
            get { return name; }
        }

        public virtual string FullName
        {
            get { return name + " longed"; }
        }

        public override string ToString()
        {
            return name;
        }
    }
}