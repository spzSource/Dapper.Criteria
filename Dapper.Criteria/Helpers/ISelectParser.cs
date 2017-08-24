using System.Collections.Generic;
using Dapper.Criteria.Helpers.Select;

namespace Dapper.Criteria.Helpers
{
    public interface ISelectParser
    {
        IDictionary<string, ICollection<SelectClause>> Parse(string str, bool strict = true);
    }
}