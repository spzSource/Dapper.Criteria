using System.Data;
using System.Linq;
using Dapper.Criteria.Helpers;
using Xunit;

namespace Dapper.Criteria.Tests.Helpers
{
    public class SelectParserTests
    {
        private readonly SelectParser _selectParser;

        public SelectParserTests()
        {
            _selectParser = new SelectParser();
        }

        [Fact]
        public void ParseTest1()
        {
            const string str = "Table:column1,column2,column3";
            var res = _selectParser.Parse(str);

            Assert.NotNull(res);
            Assert.Equal(1, res.Count);
            Assert.Equal(3, res["Table"].Count);
            var resultColumns = res["Table"].ToArray();
            Assert.Equal("column1", resultColumns[0].Select);
            Assert.Equal("column2", resultColumns[1].Select);
            Assert.Equal("column3", resultColumns[2].Select);
        }

        [Fact]
        public void ParseTest2()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column100";
            var res = _selectParser.Parse(str);

            Assert.NotNull(res);
            Assert.Equal(2, res.Count);
            Assert.Equal(3, res["Table"].Count);
            Assert.Equal(2, res["TableTwo"].Count);
            var result1Columns = res["Table"].ToArray();
            var result2Columns = res["TableTwo"].ToArray();
            Assert.Equal("column1", result1Columns[0].Select);
            Assert.Equal("column2", result1Columns[1].Select);
            Assert.Equal("column3", result1Columns[2].Select);

            Assert.Equal("column", result2Columns[0].Select);
            Assert.Equal("column100", result2Columns[1].Select);
        }

        [Fact]
        public void ParseTest3()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column100;Table:column4";
            var res = _selectParser.Parse(str);

            Assert.NotNull(res);
            Assert.Equal(2, res.Count);
            Assert.Equal(4, res["Table"].Count);
            Assert.Equal(2, res["TableTwo"].Count);
            var result1Columns = res["Table"].ToArray();
            var result2Columns = res["TableTwo"].ToArray();
            Assert.Equal("column1", result1Columns[0].Select);
            Assert.Equal("column2", result1Columns[1].Select);
            Assert.Equal("column3", result1Columns[2].Select);
            Assert.Equal("column4", result1Columns[3].Select);

            Assert.Equal("column", result2Columns[0].Select);
            Assert.Equal("column100", result2Columns[1].Select);
        }

        [Fact]
        public void ParseTestDuplicateNameException()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column1;Table:column1";
            Assert.Throws<DuplicateNameException>(() => _selectParser.Parse(str));
        }

        [Fact]
        public void ParseTestDuplicateNameNotStrict()
        {
            const string str = "Table:column1,column2,column3;TableTwo:column,column1;Table:column1";
            var res = _selectParser.Parse(str, false);

            Assert.NotNull(res);
            Assert.Equal(2, res.Count);
            Assert.Equal(4, res["Table"].Count);
            Assert.Equal(2, res["TableTwo"].Count);
            var result1Columns = res["Table"].ToArray();
            var result2Columns = res["TableTwo"].ToArray();
            Assert.Equal("column1", result1Columns[0].Select);
            Assert.Equal("column2", result1Columns[1].Select);
            Assert.Equal("column3", result1Columns[2].Select);
            Assert.Equal("column1", result1Columns[3].Select);

            Assert.Equal("column", result2Columns[0].Select);
            Assert.Equal("column1", result2Columns[1].Select);
        }

        [Fact]
        public void ParseTestWithExpressions()
        {
            const string str =
                "Table:{{sum(x)}},one,two,{{three, four}},five,{{next}};" +
                "SecondTable:one,{{(select id from table2 where code=Table.Code)}}";
            var res = _selectParser.Parse(str);
            Assert.Equal(2, res.Count);
            var result = res["Table"].ToArray();
            Assert.True(result.All(x => x.Table.Equals("Table")));
            Assert.Equal(6, result.Length);
            Assert.Equal("sum(x)", result[0].Select);
            Assert.Equal(true, result[0].IsExpression);
            Assert.Equal("one", result[1].Select);
            Assert.Equal(false, result[1].IsExpression);
            Assert.Equal("two", result[2].Select);
            Assert.Equal(false, result[2].IsExpression);
            Assert.Equal("three, four", result[3].Select);
            Assert.Equal(true, result[3].IsExpression);
            Assert.Equal("five", result[4].Select);
            Assert.Equal(false, result[4].IsExpression);
            Assert.Equal("next", result[5].Select);
            Assert.Equal(true, result[5].IsExpression);

            var result2 = res["SecondTable"].ToArray();
            Assert.True(result2.All(x => x.Table.Equals("SecondTable")));
            Assert.Equal(2, result2.Length);
            Assert.Equal("one", result2[0].Select);
            Assert.Equal(false, result2[0].IsExpression);
            Assert.Equal("(select id from table2 where code=Table.Code)", result2[1].Select);
            Assert.Equal(true, result2[1].IsExpression);
        }
    }
}