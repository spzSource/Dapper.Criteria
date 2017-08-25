using System;
using System.Text.RegularExpressions;

using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models;
using Dapper.Criteria.Models.Enumerations;

using Xunit;

namespace Dapper.Criteria.Tests
{
    [Table(Name = "TableName", Alias = "[tn]")]
    internal class TestWhereCriteria : Models.Criteria
    {
        [Where(TableAlias = "[tn]")]
        public int? Id { get; set; }

        [Where(WhereType = WhereType.Like, TableAlias = "[tn]")]
        public string Name { get; set; }
    }

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

    public class TableAliasTests
    {
        [Fact]
        public void ShouldUseTableAliasInWhereClauseTest()
        {
            QueryBuilder<TestWhereCriteria> builder = new QueryBuilder<TestWhereCriteria>(
                new TestWhereCriteria
                {
                    Id = 1,
                    Name = "Lala"
                });

            Query query = builder.Build();

            Assert.Equal("SELECT [tn].* FROM TableName [tn] WHERE [tn].Id = @tnId AND [tn].Name Like @tnName", SimplifyString(query.Sql));
        }

        [Fact]
        public void ShouldUseTableAliasInJoinClauseTest()
        {
            QueryBuilder<TestJoinCriteria> builder = new QueryBuilder<TestJoinCriteria>(
                new TestJoinCriteria
                {
                    WithCars = true,
                    WithHouses = true,
                    WithAirplans = true,
                    WithInstruments = true
                });

            Query query = builder.Build();

            Assert.Equal(
                "SELECT [p].* , 0 as SplitOnCarsPersonId , [c].* , 0 as SplitOnAirplansPersonId , [a].* , 0 as SplitOnHousesPersonId , [h].* , 0 as SplitOnInstrumentsInstrument , [i].* FROM Persons [p] " +
                "LEFT JOIN Cars [c] on [c].PersonId = [p].Id " +
                "LEFT JOIN Airplans [a] on [a].PersonId = [p].Id " +
                "LEFT JOIN Houses [h] on [h].PersonId = [p].Id " +
                "LEFT JOIN Instruments [i] on [i].Instrument = [c].InstrId"
                , SimplifyString(query.Sql)
            );
        }

        private static string SimplifyString(string str)
        {
            return
                new Regex("\\s+").Replace(
                    str.Replace("\\r\\n", " ").Replace("\\r", " ").Replace("\\n", " ").Replace(Environment.NewLine, " "),
                    " ").Trim().Replace("  ", " ");
        }
    }
}