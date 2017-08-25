using System.Collections.Generic;

namespace Dapper.Criteria.Helpers
{
    public interface IClauseManager<out T>
    {
        IEnumerable<T> Get(Models.Criteria criteria, string tableName, string alias);
    }
}