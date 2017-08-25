using System;
using System.Text.RegularExpressions;

using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models;
using Dapper.Criteria.Models.Enumerations;

using Xunit;

namespace Dapper.Criteria.Tests
{
    [Table(Name = "TableName", Alias = "[tn]")]
    internal class TestCriteria : Models.Criteria
    {
        [Where(TableAlias = "[tn]")]
        public int? Id { get; set; }

        [Where(WhereType = WhereType.Like)]
        public string Name { get; set; }
    }

    public class TableAliasTests
    {
        [Fact]
        public void ShouldUseTableAliasInWhereClauseTest()
        {
            QueryBuilder<TestCriteria> builder = new QueryBuilder<TestCriteria>(new TestCriteria
            {
                Id = 1
            });
            Query query = builder.Build();

            Assert.Equal("SELECT [tn].* FROM TableName [tn] WHERE [tn].Id = @tnId", SimplifyString(query.Sql));
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