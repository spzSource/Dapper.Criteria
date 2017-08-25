using System;
using Dapper.Criteria.Helpers.Where;
using Dapper.Criteria.Models.Enumerations;
using Xunit;

namespace Dapper.Criteria.Tests.Helpers
{
    public class WhereAttributeManagerTests
    {
        private readonly WhereAttributeManager _whereAttributeManager;

        public WhereAttributeManagerTests()
        {
            _whereAttributeManager = new WhereAttributeManager();
        }

        [Fact]
        public void TestIsWithoutValue()
        {
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.Eq));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.Gt));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.GtEq));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.In));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.Like));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.Lt));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.LtEq));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.NotEq));
            Assert.False(_whereAttributeManager.IsWithoutValue(WhereType.NotIn));

            Assert.True(_whereAttributeManager.IsWithoutValue(WhereType.IsNotNull));
            Assert.True(_whereAttributeManager.IsWithoutValue(WhereType.IsNull));
        }

        [Fact]
        public void TestIsWithoutValueExpExc()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _whereAttributeManager.IsWithoutValue((WhereType) 100000));
        }

        [Fact]
        public void TestGetSelector()
        {
            Assert.Equal("=", _whereAttributeManager.GetSelector(WhereType.Eq));
            Assert.Equal("<>", _whereAttributeManager.GetSelector(WhereType.NotEq));
            Assert.Equal(">", _whereAttributeManager.GetSelector(WhereType.Gt));
            Assert.Equal("<", _whereAttributeManager.GetSelector(WhereType.Lt));
            Assert.Equal(">=", _whereAttributeManager.GetSelector(WhereType.GtEq));
            Assert.Equal("<=", _whereAttributeManager.GetSelector(WhereType.LtEq));
            Assert.Equal("Like", _whereAttributeManager.GetSelector(WhereType.Like));
            Assert.Equal("is null", _whereAttributeManager.GetSelector(WhereType.IsNull));
            Assert.Equal("is not null", _whereAttributeManager.GetSelector(WhereType.IsNotNull));
            Assert.Equal("in", _whereAttributeManager.GetSelector(WhereType.In));
            Assert.Equal("not in", _whereAttributeManager.GetSelector(WhereType.NotIn));
        }

        [Fact]
        public void TestGetSelectorExpExc()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _whereAttributeManager.GetSelector((WhereType) 100000));
        }

        [Fact]
        public void TestGetExpression()
        {
            Assert.Equal("= @Name", _whereAttributeManager.GetExpression(WhereType.Eq, "@Name"));
            Assert.Equal("<> @Name", _whereAttributeManager.GetExpression(WhereType.NotEq, "@Name"));
            Assert.Equal("> @Name", _whereAttributeManager.GetExpression(WhereType.Gt, "@Name"));
            Assert.Equal("< @Name", _whereAttributeManager.GetExpression(WhereType.Lt, "@Name"));
            Assert.Equal(">= @Name", _whereAttributeManager.GetExpression(WhereType.GtEq, "@Name"));
            Assert.Equal("<= @Name", _whereAttributeManager.GetExpression(WhereType.LtEq, "@Name"));
            Assert.Equal("Like @Name", _whereAttributeManager.GetExpression(WhereType.Like, "@Name"));
            Assert.Equal("in @Name", _whereAttributeManager.GetExpression(WhereType.In, "@Name"));
            Assert.Equal("not in @Name", _whereAttributeManager.GetExpression(WhereType.NotIn, "@Name"));
            Assert.Equal("is null", _whereAttributeManager.GetExpression(WhereType.IsNull, "@Name"));
            Assert.Equal("is not null", _whereAttributeManager.GetExpression(WhereType.IsNotNull, "@Name"));
            Assert.Equal("is null", _whereAttributeManager.GetExpression(WhereType.IsNull, string.Empty));
            Assert.Equal("is not null", _whereAttributeManager.GetExpression(WhereType.IsNotNull, string.Empty));
            Assert.Equal("is null", _whereAttributeManager.GetExpression(WhereType.IsNull, null));
            Assert.Equal("is not null", _whereAttributeManager.GetExpression(WhereType.IsNotNull, null));
        }

        [Fact]
        public void TestGetExpressionExpExc()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _whereAttributeManager.GetExpression((WhereType) 100000, "ParameterNAme"));
        }
    }
}