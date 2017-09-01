using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Tests.Alias.Criteria
{
    [Table(Name = "Persons", Alias = "[p]")]
    public class TestManyToManyJoinCriteria : Models.Criteria
    {
        [ManyToManyJoin(
            currentTableField: "CompanyId", 
            joinType: JoinType.Left, 
            joinedTable: "Company", 
            joinedTableAlias: "[c]",
            communicationTable: "CompanyPersons", 
            communicationTableAlias: "[cp]",
            communicationTableCurrentTableField: "PersonId",
            communicationTableJoinedTableField: "CompanyId", 
            JoinedTableField = "Id")]
        public bool WithCompany { get; set; }

        [Where(
            field: "Id", 
            TableAlias = "[c]", 
            TableName = "Company")]
        public int? CompanyId { get; set; }

        [Where]
        public int? Id { get; set; }
    }
}