using System;

namespace Dapper.Criteria.Helpers.Join
{
    public interface IJoinClauseCreatorFactory
    {
        IJoinClauseCreator Get(Type joinAttributeType);
    }
}