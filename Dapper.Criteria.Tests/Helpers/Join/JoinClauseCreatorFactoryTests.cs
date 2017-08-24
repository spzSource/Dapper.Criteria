using System;
using Dapper.Criteria.Helpers.Join;
using Dapper.Criteria.Metadata;
using Xunit;

namespace Dapper.Criteria.Tests.Helpers.Join
{
    public class JoinClauseCreatorFactoryTests
    {
        private readonly JoinClauseCreatorFactory _joinClauseCreatorFactory;

        public JoinClauseCreatorFactoryTests()
        {
            _joinClauseCreatorFactory = new JoinClauseCreatorFactory();
        }

        [Fact]
        public void GetTest()
        {
            IJoinClauseCreator res = _joinClauseCreatorFactory.Get(typeof (SimpleJoinAttribute));
            Assert.Equal(typeof (SimpleJoinClauseCreator), res.GetType());
        }

        [Fact]
        public void GetTestMany()
        {
            IJoinClauseCreator res = _joinClauseCreatorFactory.Get(typeof(ManyToManyJoinAttribute));
            Assert.Equal(typeof(ManyToManyClauseCreator), res.GetType());
        }

        [Fact]
        public void GetTestArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _joinClauseCreatorFactory.Get(typeof (JoinAttribute)));
        }

        [Fact]
        public void GetTestArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _joinClauseCreatorFactory.Get(typeof (string)));
        }
    }
}