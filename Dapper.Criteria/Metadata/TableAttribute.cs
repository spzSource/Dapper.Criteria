using System;

namespace Dapper.Criteria.Metadata
{
    public class TableAttribute : Attribute
    {
        public TableAttribute()
        {
            
        }

        public TableAttribute(string name)
        {
            Name = name;
            Alias = null;
        }

        public TableAttribute(string name, string alias)
        {
            Name = name;
            Alias = alias;
        }

        public string Name { get; set; }

        public string Alias { get; set; }
    }
}