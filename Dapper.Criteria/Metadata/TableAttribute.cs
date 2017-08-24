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
        }

        public string Name { get; set; }
    }
}