using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Tests.Alias.Criteria
{
    [Table(Name = "Persons", Alias = "[p]")]
    internal class TestJoinCriteria : Models.Criteria
    {
        [SimpleJoin(
            "Id", 
            JoinType.Left, 
            "Houses",
            "[h]",
            JoinedTableField = "PersonId")]
        public bool WithHouses { get; set; }

        [SimpleJoin(
            "Id", 
            JoinType.Left, 
            "Airplans", 
            "[a]",
            JoinedTableField = "PersonId", 
            Order = 2)]
        public bool WithAirplans { get; set; }

        [SimpleJoin(
            "Id", 
            JoinType.Left, 
            "Cars", 
            "[c]",
            JoinedTableField = "PersonId", 
            Order = 1)]
        public bool WithCars { get; set; }

        [SimpleJoin(
            "InstrId",
            JoinType.Left,
            "Instruments",
            "[i]",
            JoinedTableField = "Instrument",
            CurrentTableAlias = "[c]",
            CurrentTable = "Cars")]
        public bool WithInstruments { get; set; }
    }
}