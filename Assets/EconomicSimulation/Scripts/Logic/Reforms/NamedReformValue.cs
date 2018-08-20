using Nashet.Conditions;
using Nashet.Utils;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    //public abstract class NamedReform : AbstractReform
    //{
    //    public NamedReform(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(name, indescription, country, possibleValues)
    //    {
    //    }
    //}

    public abstract class NamedReformValue : AbstractReformValue, INameable
    {
        protected readonly string description;
        protected readonly string name;

        internal NamedReformValue(string name, string description, int id, DoubleConditionsList condition) : base(id, condition)
        {
            this.description = description;
            this.name = name;
        }

        public string FullName { get { return description; } }

        public string ShortName { get { return name; } }

      
        public override string ToString()
        {
            return ShortName;
        }
    }

}