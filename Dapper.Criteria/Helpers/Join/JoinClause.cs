using System.Collections.Generic;
using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Helpers.Join
{
    public class JoinClause
    {
        public string Splitter { get; set; }

        public IEnumerable<string> JoinSqls { get; set; }

        public IEnumerable<string> SelectsSql { get; set; }

        public JoinType JoinType { get; set; }

        public bool HasJoin { get; set; }

        public int Order { get; set; }
    }
}