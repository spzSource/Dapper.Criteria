using System;
using System.Collections.Generic;
using System.Linq;

using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Helpers.Join
{
    public class ManyToManyClauseCreator : JoinClauseCreator
    {
        public override JoinClause Create(JoinAttribute joinAttribute)
        {
            ManyToManyJoinAttribute manyToManyJoinAttribute;
            if ((manyToManyJoinAttribute = joinAttribute as ManyToManyJoinAttribute) == null)
            {
                throw new ArgumentException("Attribute must be ManyToManyJoinAttribute");
            }

            var splitter = GetSplitter(manyToManyJoinAttribute);
            var selects = new List<string>();
            if (manyToManyJoinAttribute.TableSelectColumns != null && manyToManyJoinAttribute.TableSelectColumns.Any())
            {
                foreach (var tableSelectColumn in manyToManyJoinAttribute.TableSelectColumns)
                {
                    selects.AddRange(
                        tableSelectColumn.Value.Select(selectClause =>
                            selectClause.IsExpression
                                ? selectClause.Select
                                : $"{selectClause.Table}.{selectClause.Select}"));
                }
            }
            else
            {
                selects.Add(!String.IsNullOrEmpty(manyToManyJoinAttribute.JoinedTableAlias)
                    ? $"{manyToManyJoinAttribute.JoinedTableAlias}.*"
                    : $"{manyToManyJoinAttribute.JoinedTable}.*");
            }

            var joins = new List<string>
            {
                CreateJoinCommunication(manyToManyJoinAttribute),
                CreateJoinJoined(manyToManyJoinAttribute),
            };
            var result = new JoinClause
            {
                JoinSqls = joins,
                SelectsSql = selects,
                Splitter = splitter,
                JoinType = manyToManyJoinAttribute.JoinType,
                HasJoin = true,
                Order = joinAttribute.Order,
            };
            return result;
        }

        private string CreateJoinCommunication(ManyToManyJoinAttribute attribute)
        {
            if (!String.IsNullOrEmpty(attribute.CommunicationTableAlias))
            {
                return string.Format("{0} {1} on {1}.{2} = {3}.{4}{5}", 
                    attribute.CommunicationTable,
                    attribute.CommunicationTableAlias,
                    attribute.CommunicationTableCurrentTableField,
                    !String.IsNullOrEmpty(attribute.CurrentTableAlias) 
                        ? attribute.CurrentTableAlias 
                        : attribute.CurrentTable,
                    attribute.CurrentTableField,
                    attribute.AddOnType == AddOnType.ForCommunication
                        ? GetAddOnClauses(attribute)
                        : string.Empty);
            }
            
            return string.Format("{0} on {0}.{1} = {2}.{3}{4}", 
                attribute.CommunicationTable,
                attribute.CommunicationTableCurrentTableField,
                attribute.CurrentTable,
                attribute.CurrentTableField,
                attribute.AddOnType == AddOnType.ForCommunication
                    ? GetAddOnClauses(attribute)
                    : string.Empty);
            
            
        }

        private string CreateJoinJoined(ManyToManyJoinAttribute attribute)
        {
            if (!String.IsNullOrEmpty(attribute.JoinedTableAlias))
            {
                return string.Format("{0} {1} on {1}.{2} = {3}.{4}{5}", 
                    attribute.JoinedTable,
                    attribute.JoinedTableAlias,
                    attribute.JoinedTableField, 
                    !String.IsNullOrEmpty(attribute.CommunicationTableAlias) 
                        ? attribute.CommunicationTableAlias 
                        : attribute.CommunicationTable,
                    attribute.CommunicationTableJoinedTableField,
                    attribute.AddOnType == AddOnType.ForJoined
                        ? GetAddOnClauses(attribute)
                        : string.Empty);
            }
            
            return string.Format("{0} on {0}.{1} = {2}.{3}{4}", attribute.JoinedTable,
                attribute.JoinedTableField, 
                attribute.CommunicationTable,
                attribute.CommunicationTableJoinedTableField,
                attribute.AddOnType == AddOnType.ForJoined
                    ? GetAddOnClauses(attribute)
                    : string.Empty);
        }
    }
}