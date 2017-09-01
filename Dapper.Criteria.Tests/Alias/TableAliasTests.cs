using Dapper.Criteria.Models;
using Dapper.Criteria.Tests.Alias.Criteria;
using Dapper.Criteria.Tests.Utils;

using FluentAssertions;

using Xunit;

namespace Dapper.Criteria.Tests.Alias
{
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

            query.Sql
                .Simplify()
                .Should()
                .Be(@"
                    SELECT
                        [tn].* 
                    FROM TableName [tn] 
                    WHERE [tn].Id = @tnId 
                        AND [tn].Name Like @tnName
                ".Simplify());
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
                    WithInstruments = true,
                    Name = "Instrument #1"
                });

            Query query = builder.Build();

            query.Sql
                .Simplify()
                .Should()
                .Be(@"
                    SELECT 
                        [p].* , 0 as SplitOnCarsPersonId , 
                        [c].* , 0 as SplitOnAirplansPersonId , 
                        [a].* , 0 as SplitOnHousesPersonId , 
                        [h].* , 0 as SplitOnInstrumentsInstrument , 
                        [i].* 
                    FROM Persons [p]
                        LEFT JOIN Cars [c] on [c].PersonId = [p].Id
                        LEFT JOIN Airplans [a] on [a].PersonId = [p].Id
                        LEFT JOIN Houses [h] on [h].PersonId = [p].Id
                        LEFT JOIN Instruments [i] on [i].Instrument = [c].InstrId
                    WHERE [i].Name = @iName
                ".Simplify());
        }

        [Fact]
        public void ShouldUseTableAliasInManyToManyJoinTest()
        {
            QueryBuilder<TestManyToManyJoinCriteria> builder =
                new QueryBuilder<TestManyToManyJoinCriteria>(
                    new TestManyToManyJoinCriteria
                    {
                        Id = 1,
                        WithCompany = true
                    });

            Query query = builder.Build();

            query.Sql
                .Simplify()
                .Should()
                .Be(@"
                    SELECT 
                        [p].* , 0 as SplitOnCompanyId , 
                        [c].* 
                    FROM Persons [p]
                        LEFT JOIN CompanyPersons [cp] on [cp].PersonId = [p].CompanyId
                        LEFT JOIN Company [c] on [c].Id = [cp].CompanyId
                    WHERE [p].Id = @pId
                ".Simplify());
        }
    }
}