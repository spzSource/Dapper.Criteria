using System;
using System.Linq;
using Dapper.Criteria.Helpers.Join;
using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;
using Xunit;

namespace Dapper.Criteria.Tests.Helpers.Join
{
    public class SimpleJoinClauseCreatorTests
    {
        [Fact]
        public void CreateTest()
        {
            var creator = new SimpleJoinClauseCreator();
            var attr = new SimpleJoinAttribute("CurrentTableField", JoinType.Left, "JoinedTable")
            {
                CurrentTable = "CurrentTable",
                JoinedTableField = "JoinedField"
            };
            var res = creator.Create(attr);
            Assert.Equal(JoinType.Left, res.JoinType);
            Assert.Equal(1, res.JoinSqls.Count());
            Assert.Equal(1, res.SelectsSql.Count());
            Assert.Equal("JoinedTable.*", res.SelectsSql.First());
            Assert.Equal("SplitOnJoinedTableJoinedField", res.Splitter);
            Assert.Equal("JoinedTable on JoinedTable.JoinedField = CurrentTable.CurrentTableField",
                res.JoinSqls.First());
            Assert.True(res.HasJoin);
        }

        [Fact]
        public void CreateNotJoinTest()
        {
            var creator = new SimpleJoinClauseCreator();
            var attr = new SimpleJoinAttribute("CurrentTableField", JoinType.Left, "JoinedTable")
            {
                CurrentTable = "CurrentTable",
                JoinedTableField = "JoinedField"
            };
            var res = creator.CreateNotJoin(attr);
            Assert.Equal(JoinType.Left, res.JoinType);
            Assert.True(res.JoinSqls == null || !res.JoinSqls.Any());
            Assert.Equal("SplitOnJoinedTableJoinedField", res.Splitter);
            Assert.False(res.HasJoin);
        }

        [Fact]
        public void CreateTestException()
        {
            var creator = new SimpleJoinClauseCreator();
            Assert.Throws<ArgumentException>(() => creator.Create(new TestJoinAttr(JoinType.Inner)));
        }

        private class TestJoinAttr : JoinAttribute
        {
            public TestJoinAttr(JoinType joinType) : base("cf", joinType)
            {
            }
        }
    }
}