using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Tests.Alias.Criteria
{
    [Table(Name = "TableName", Alias = "[tn]")]
    internal class TestWhereCriteria : Models.Criteria
    {
        [Where(TableAlias = "[tn]")]
        public int? Id { get; set; }

        [Where(WhereType = WhereType.Like, TableAlias = "[tn]")]
        public string Name { get; set; }
    }
}