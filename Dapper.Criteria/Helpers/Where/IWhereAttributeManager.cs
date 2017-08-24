using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Helpers.Where
{
    public interface IWhereAttributeManager
    {
        bool IsWithoutValue(WhereType whereType);
        string GetExpression(WhereType whereType, string paramName);
        string GetSelector(WhereType whereType);
    }
}