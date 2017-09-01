using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Dapper.Criteria.Formatters;
using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;

namespace Dapper.Criteria.Helpers.Where
{
    public class WhereClauseManager : IClauseManager<WhereClause>
    {
        private readonly IWhereAttributeManager _whereAttributeManager;

        public WhereClauseManager(IWhereAttributeManager whereAttributeManager)
        {
            _whereAttributeManager = whereAttributeManager;
        }

        public IEnumerable<WhereClause> Get(
            Models.Criteria criteria, string criteriaTableName, string criteriaTableAlias)
        {
            var type = criteria.GetType();
            var propertyInfos = type.GetProperties()
                .Where(pi => pi.HasAttribute<WhereAttribute>());
            var whereClauses = new List<WhereClause>();
            foreach (var propertyInfo in propertyInfos)
            {
                object value;
                var whereAttributes = propertyInfo.GetCustomAttributes<WhereAttribute>();
                if ((value = propertyInfo.GetValue(criteria, null)) == null)
                {
                    continue;
                }
                if (propertyInfo.PropertyType == typeof(bool) && !(bool)value)
                {
                    continue;
                }
                foreach (var whereAttribute in whereAttributes)
                {
                    string tableName;

                    if (!String.IsNullOrEmpty(whereAttribute.TableAlias))
                    {
                        tableName = whereAttribute.TableAlias;
                    }
                    else if (!String.IsNullOrEmpty(whereAttribute.TableName))
                    {
                        tableName = whereAttribute.TableName;
                    }
                    else if (!String.IsNullOrEmpty(criteriaTableAlias))
                    {
                        tableName = criteriaTableAlias;
                    }
                    else
                    {
                        tableName = criteriaTableName;
                    }
                    
                    string paramName = $"@{NormalizeTableName(tableName)}{propertyInfo.Name}";
                    string str = GeWheretSting(whereAttribute, propertyInfo, tableName, paramName, ref value);
                    whereClauses.Add(new WhereClause
                    {
                        ParameterName = paramName,
                        ParameterValue = value,
                        Sql = str,
                        WithoutValue = _whereAttributeManager.IsWithoutValue(whereAttribute.WhereType)
                    });
                }
            }
            return whereClauses;
        }

        private static string NormalizeTableName(string tableName)
        {
            return tableName.Replace("[", "").Replace("]", "");
        }

        private static string GetWhereStringByExpression(WhereAttribute whereAttribute, string tableName,
            string fieldName, string compareOperation, string paramName)
        {
            return whereAttribute.Expression
                .Replace(GetNameForReplace("TableName"), tableName)
                .Replace(GetNameForReplace("FieldName"), fieldName)
                .Replace(GetNameForReplace("CompareOperation"), compareOperation)
                .Replace(GetNameForReplace("Parameter"), paramName);
        }

        private static string GetNameForReplace(string replaced)
        {
            return $"/**{replaced}**/";
        }

        private static void SetValueByWhereType(WhereType whereType, ref object value, IFormatter formatter = null)
        {
            if (formatter == null)
            {
                formatter = GetFormatter(whereType);
            }
            formatter.Format(ref value);
        }

        private static IFormatter GetFormatter(WhereType whereType)
        {
            if (whereType == WhereType.Like)
            {
                return new SimpleLikeFormatter();
            }
            return new DummyFormatter();
        }

        private string GeWheretSting(WhereAttribute whereAttribute, PropertyInfo propertyInfo, string tableName,
            string paramName, ref object value)
        {
            var fieldName = !string.IsNullOrWhiteSpace(whereAttribute.Field)
                ? whereAttribute.Field
                : propertyInfo.Name;
            string str;
            var formatAttr = propertyInfo.GetCustomAttribute<FormatAttribute>();
            IFormatter formatter = null;
            if (formatAttr != null)
            {
                formatter = (IFormatter)Activator.CreateInstance(formatAttr.FormatterType);
            }
            SetValueByWhereType(whereAttribute.WhereType, ref value, formatter);
            if (string.IsNullOrWhiteSpace(whereAttribute.Expression))
            {
                str = $"{tableName}.{fieldName} {_whereAttributeManager.GetExpression(whereAttribute.WhereType, paramName)} ";
            }
            else
            {
                var whereString = GetWhereStringByExpression(whereAttribute, tableName, fieldName,
                    _whereAttributeManager.GetSelector(
                        whereAttribute.WhereType),
                    paramName);
                str = $"({whereString})";
            }
            return str;
        }
    }
}