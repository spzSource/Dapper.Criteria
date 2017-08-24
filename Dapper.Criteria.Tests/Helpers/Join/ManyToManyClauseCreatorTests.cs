using System.Linq;
using Dapper.Criteria.Helpers.Join;
using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;
using Xunit;

namespace Dapper.Criteria.Tests.Helpers.Join
{
    public class ManyToManyClauseCreatorTests
    {
        [Fact]
        public void CreateTest()
        {
            var attr = new ManyToManyJoinAttribute("CurrentTableField", JoinType.Left, "JoinedTable",
                "CommunicationTable", "CommunicationTableCurrentTableField",
                "CommunicationTableJoinedTableField")
            {
                SelectColumns = "CurrentTable:Id,Name;CommunicationTable:Required;JoinedTable:Id,Name",
                CurrentTable = "CurrentTable",
                JoinedTableField = "Id",
                CurrentTableField = "CurrentTableField",
            };
            var creator = new ManyToManyClauseCreator();
            var res = creator.Create(attr);
            Assert.True(res.HasJoin);
            Assert.Equal(JoinType.Left, res.JoinType);
            Assert.Equal(2, res.JoinSqls.Count());
            Assert.Equal(5, res.SelectsSql.Count());
            Assert.Equal("SplitOnJoinedTableId", res.Splitter);

            Assert.Equal("CurrentTable.Id", res.SelectsSql.ToArray()[0]);
            Assert.Equal("CurrentTable.Name", res.SelectsSql.ToArray()[1]);
            Assert.Equal("CommunicationTable.Required", res.SelectsSql.ToArray()[2]);
            Assert.Equal("JoinedTable.Id", res.SelectsSql.ToArray()[3]);
            Assert.Equal("JoinedTable.Name", res.SelectsSql.ToArray()[4]);

            Assert.Equal(
                "CommunicationTable on CommunicationTable.CommunicationTableCurrentTableField = CurrentTable.CurrentTableField",
                res.JoinSqls.ToArray()[0]);
            Assert.Equal("JoinedTable on JoinedTable.Id = CommunicationTable.CommunicationTableJoinedTableField",
                res.JoinSqls.ToArray()[1]);
        }
    }
}