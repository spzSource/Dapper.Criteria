using System;
using System.Collections.Generic;
using System.Linq;
using Dapper.Criteria.Metadata;

namespace Dapper.Criteria.Helpers.Join
{
    public sealed class SimpleJoinClauseCreator : JoinClauseCreator
    {
        public override JoinClause Create(JoinAttribute joinAttribute)
        {
            SimpleJoinAttribute simpleJoinAttribute;
            if ((simpleJoinAttribute = joinAttribute as SimpleJoinAttribute) == null)
            {
                throw new ArgumentException("Attribute must be SimpleJoinAttribute");
            }

            var splitter = GetSplitter(simpleJoinAttribute);
            var selects = new List<string>();
            if (simpleJoinAttribute.TableSelectColumns != null && simpleJoinAttribute.TableSelectColumns.Any())
            {
                foreach (var tableSelectColumn in simpleJoinAttribute.TableSelectColumns)
                {
                    selects.AddRange(
                        tableSelectColumn.Value.Select(column => column.IsExpression
                            ? column.Select
                            : $"{column.Table}.{column.Select}"));
                }
            }
            else
            {
                selects.Add(String.IsNullOrEmpty(simpleJoinAttribute.JoinedTableAlias)
                    ? $"{simpleJoinAttribute.JoinedTable}.*"
                    : $"{simpleJoinAttribute.JoinedTableAlias}.*");
            }

            string joinSql;

            if (String.IsNullOrEmpty(simpleJoinAttribute.JoinedTableAlias))
            {
                joinSql = string.Format("{0} on {0}.{1} = {2}.{3}{4}", simpleJoinAttribute.JoinedTable,
                    simpleJoinAttribute.JoinedTableField, simpleJoinAttribute.CurrentTable,
                    simpleJoinAttribute.CurrentTableField, GetAddOnClauses(simpleJoinAttribute));
            }
            else
            {
                joinSql = string.Format("{0} {1} on {1}.{2} = {3}.{4}{5}", simpleJoinAttribute.JoinedTable, 
                    simpleJoinAttribute.JoinedTableAlias,
                    simpleJoinAttribute.JoinedTableField, simpleJoinAttribute.CurrentTableAlias,
                    simpleJoinAttribute.CurrentTableField, GetAddOnClauses(simpleJoinAttribute));
            }

            var result = new JoinClause
            {
                JoinSqls = new List<string>
                {
                    joinSql,
                },
                SelectsSql = selects,
                Splitter = splitter,
                JoinType = simpleJoinAttribute.JoinType,
                HasJoin = true,
                Order = joinAttribute.Order,
            };
            return result;
        }
    }
}