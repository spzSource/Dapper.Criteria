using Dapper.Criteria.Metadata;

namespace Dapper.Criteria.Helpers.Join
{
    public interface IJoinClauseCreator
    {
        JoinClause Create(JoinAttribute joinAttribute);
        JoinClause CreateNotJoin(JoinAttribute joinAttribute);
    }
}