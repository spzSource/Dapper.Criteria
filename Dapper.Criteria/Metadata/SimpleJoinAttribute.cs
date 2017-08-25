using System;
using System.Collections.Generic;
using Dapper.Criteria.Helpers;
using Dapper.Criteria.Helpers.Select;
using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Metadata
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class SimpleJoinAttribute : JoinAttribute
    {
        public readonly string JoinedTable;
        private Type _parserType;

        public readonly string JoinedTableAlias;

        private IDictionary<string, ICollection<SelectClause>> _tableSelectColumns;
        private string _joinedTableField;

        public SimpleJoinAttribute(string currentTableField, JoinType joinType, string joinedTable)
            : base(currentTableField, joinType)
        {
            JoinedTable = joinedTable;
            JoinedTableAlias = null;
        }

        public SimpleJoinAttribute(string currentTableField, JoinType joinType, string joinedTable, string joinedTableAlias)
            : base(currentTableField, joinType)
        {
            JoinedTable = joinedTable;
            JoinedTableAlias = joinedTableAlias;
        }

        public IDictionary<string, ICollection<SelectClause>> TableSelectColumns
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SelectColumns))
                {
                    return null;
                }
                if (_tableSelectColumns == null)
                {
                    ISelectParser selectParser;
                    if (ParserType != null)
                    {
                        selectParser = (ISelectParser) Activator.CreateInstance(ParserType);
                    }
                    else
                    {
                        selectParser = new SelectParser();
                    }
                    _tableSelectColumns = selectParser.Parse(SelectColumns);
                }
                return _tableSelectColumns;
            }
        }

        public string JoinedTableField
        {
            get { return string.IsNullOrWhiteSpace(_joinedTableField) ? CurrentTableField : _joinedTableField; }
            set { _joinedTableField = value; }
        }

        public string SelectColumns { get; set; }

        public Type ParserType
        {
            get { return _parserType; }
            set
            {
                if (!typeof (ISelectParser).IsAssignableFrom(value))
                {
                    throw new InvalidCastException("Parser must implement ISelectParser");
                }
                _parserType = value;
            }
        }
    }
}