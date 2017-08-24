using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Dapper.Criteria.Formatters;
using Dapper.Criteria.Helpers.Select;
using Dapper.Criteria.Metadata;
using Dapper.Criteria.Models.Enumerations;
using Xunit;

namespace Dapper.Criteria.Tests
{
    public class QueryBuilderTest
    {
        [Fact]
        public void TestSimple()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria());
            var query = builder.Build();
            Assert.Equal("Select TableName.* from TableName", SimplifyString(query.Sql));
            Assert.Equal("Id", query.SplitOn);
            DynamicParameters tmp;
            Assert.NotNull((tmp = query.Parameters as DynamicParameters));
            Assert.Equal(0, tmp.ParameterNames.Count());
        }

        [Fact]
        public void TestWhereEq()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
                {
                    Id = 1,
                });
            var query = builder.Build();
            Assert.Equal("Select TableName.* from TableName WHERE TableName.Id = @TableNameId",
                            SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);

            Assert.Equal(1, dynamicParameters.ParameterNames.Count());
            Assert.Equal("TableNameId", dynamicParameters.ParameterNames.Single());
            Assert.Equal(1, parameters["TableNameId"]);
        }

        [Fact]
        public void TestLike()
        {
            var builder = new TestQueryBuilder<TestCriteria>(new TestCriteria
                {
                    Name = "123",
                });
            var query = builder.Build();
            Assert.Equal("Select TableName.* from TableName WHERE TableName.Name Like @TableNameName",
                            SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);

            Assert.Equal(1, dynamicParameters.ParameterNames.Count());
            Assert.Equal("TableNameName", dynamicParameters.ParameterNames.Single());
            Assert.Equal("%123%", parameters["TableNameName"]);
        }

        [Fact]
        public void TestGtEq()
        {
            var crit = new TestCriteria
                {
                    DateFrom = DateTime.Now,
                };
            var builder = new TestQueryBuilder<TestCriteria>(crit);
            var query = builder.Build();
            Assert.Equal("Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom",
                            SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
            Assert.Equal(1, dynamicParameters.ParameterNames.Count());
            Assert.Equal("TableNameDateFrom", dynamicParameters.ParameterNames.Single());
            Assert.Equal(crit.DateFrom, parameters["TableNameDateFrom"]);
        }

        [Fact]
        public void TestGtEqAndLtEq()
        {
            var testCriteria = new TestCriteria
                {
                    DateFrom = DateTime.Now,
                    DateTo = DateTime.Now
                };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* from TableName WHERE TableName.Date >= @TableNameDateFrom AND TableName.Date <= @TableNameDateTo",
                SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
            Assert.Equal(2, dynamicParameters.ParameterNames.Count());
            Assert.Equal("TableNameDateFrom", dynamicParameters.ParameterNames.First());
            Assert.Equal("TableNameDateTo", dynamicParameters.ParameterNames.Last());
            Assert.Equal(testCriteria.DateFrom, parameters["TableNameDateFrom"]);
            Assert.Equal(testCriteria.DateTo, parameters["TableNameDateTo"]);
        }

        [Fact]
        public void TestIn()
        {
            var testCriteria = new TestCriteria
                {
                    Codes = new[] {"1", "2", "3"},
                };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* from TableName WHERE TableName.Code in @TableNameCodes",
                SimplifyString(query.Sql));

            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);

            Assert.Equal(1, dynamicParameters.ParameterNames.Count());
            Assert.Equal("TableNameCodes", dynamicParameters.ParameterNames.Single());
            Assert.Equal(testCriteria.Codes.ToList(), (string[]) parameters["TableNameCodes"]);
        }

        [Fact]
        public void TestExpression()
        {
            var testCriteria = new TestCriteria
                {
                    DateWithExpression = DateTime.Now,
                };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* from TableName WHERE ((TableName.Date is not null and TableName.Date >= @TableNameDateWithExpression) or (TableName.DateSecond >= @TableNameDateWithExpression))"
                , SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
            Assert.Equal(1, dynamicParameters.ParameterNames.Count());
            Assert.Equal("TableNameDateWithExpression", dynamicParameters.ParameterNames.Single());
            Assert.Equal(testCriteria.DateWithExpression, parameters["TableNameDateWithExpression"]);
        }

        [Fact]
        public void TestFormatter()
        {
            var testCriteria = new TestCriteria
                {
                    DateTimeWithFormatter = DateTime.Now,
                };
            var builder = new TestQueryBuilder<TestCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* from TableName WHERE TableName.DateTimeWithFormatter = @TableNameDateTimeWithFormatter"
                , SimplifyString(query.Sql));
            var dynamicParameters = ToDynamicParameters(query.Parameters);
            var parameters = GetKeyValues(dynamicParameters);
            Assert.Equal(1, dynamicParameters.ParameterNames.Count());
            Assert.Equal("TableNameDateTimeWithFormatter", dynamicParameters.ParameterNames.Single());
            Assert.Equal("1", parameters["TableNameDateTimeWithFormatter"]);
        }

        [Fact]
        public void TestSimpleJoin()
        {
            var testCriteria = new TestJoinCriteria
                {
                    WithAnotherTable = true,
                };
            var builder = new TestQueryBuilder<TestJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* , 0 as SplitOnAnotherTableCurrentTableId , AnotherTable.* from TableName LEFT JOIN AnotherTable on AnotherTable.CurrentTableId = TableName.CurrentTableId"
                , SimplifyString(query.Sql)
                );
        }

        [Fact]
        public void TestSimpleJoinEmpty()
        {
            var testCriteria = new TestJoinCriteria
                {
                    WithAnotherTable = false,
                };
            var builder = new TestQueryBuilder<TestJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* , 0 as SplitOnAnotherTableCurrentTableId from TableName"
                , SimplifyString(query.Sql)
                );
        }

        [Fact]
        public void TestManyToManyJoin()
        {
            var testCriteria = new TestManyToManyJoinCriteria
                {
                    WithAnotherTable = true,
                };
            var builder = new TestQueryBuilder<TestManyToManyJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* , 0 as SplitOnAnotherTableAnotherId , AnotherTable.* from TableName " +
                "LEFT JOIN AnotherTableCurrentTable on AnotherTableCurrentTable.CurrentId = TableName.CurrentId " +
                "LEFT JOIN AnotherTable on AnotherTable.AnotherId = AnotherTableCurrentTable.AnotherId"
                , SimplifyString(query.Sql)
                );
        }

        [Fact]
        public void TestManyToManyJoinEmpty()
        {
            var testCriteria = new TestManyToManyJoinCriteria
                {
                    WithAnotherTable = false,
                };
            var builder = new TestQueryBuilder<TestManyToManyJoinCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select TableName.* , 0 as SplitOnAnotherTableAnotherId from TableName"
                , SimplifyString(query.Sql)
                );
        }

        [Fact]
        public void TestJoinOrder()
        {
            var testCriteria = new TestJoinOrderCriteria
                {
                    WithAirplans = true,
                    WithCars = true,
                    WithHouses = true,
                };
            var builder = new TestQueryBuilder<TestJoinOrderCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Persons.* , 0 as SplitOnCarsPersonId , Cars.* , 0 as SplitOnAirplansPersonId , Airplans.* , 0 as SplitOnHousesPersonId , Houses.* from Persons " +
                "LEFT JOIN Cars on Cars.PersonId = Persons.Id " +
                "LEFT JOIN Airplans on Airplans.PersonId = Persons.Id " +
                "LEFT JOIN Houses on Houses.PersonId = Persons.Id"
                , SimplifyString(query.Sql)
                );
        }

        [Fact]
        public void TestSelect()
        {
            var testCriteria = new TestSelectCriteria
                {
                    WithSum = true,
                    SelectClause = null,
                    AddSelect = "Shipments:Name,Mass"
                };
            var builder = new TestQueryBuilder<TestSelectCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Shipments.Name , Shipments.Mass , Sum(Shipments.Price) from Shipments",
                SimplifyString(query.Sql));
        }

        [Fact]
        public void TestJoinOrderAnotherJoins()
        {
            var criteria = new TestAnotherJoinCriteria
                {
                    WithOwner = true,
                    WithPersons = true,
                };
            var builder = new QueryBuilder<TestAnotherJoinCriteria>(criteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnPersonsHouseId , Persons.* , 0 as SplitOnOwnersId , Owners.* from Houses LEFT JOIN Persons on Persons.HouseId = Houses.Id INNER JOIN Owners on Owners.Id = Houses.OwnerId",
                SimplifyString(query.Sql));
        }

        private static string SimplifyString(string str)
        {
            return
                new Regex("\\s+").Replace(
                    str.Replace("\\r\\n", " ").Replace("\\r", " ").Replace("\\n", " ").Replace(Environment.NewLine, " "),
                    " ").Trim().Replace("  ", " ");
        }

        private static Dictionary<string, object> GetKeyValues(DynamicParameters dp)
        {
            var all = Enum.GetValues(typeof (BindingFlags))
                          .Cast<BindingFlags>()
                          .Aggregate((BindingFlags) 0, (flags, bindingFlags) => flags | bindingFlags);
            var fieldInfo = dp.GetType().GetField("parameters", all);
            if (fieldInfo == null)
            {
                throw new InvalidOperationException();
            }
            var paramInfos = fieldInfo.GetValue(dp);
            var dictionary = new Dictionary<string, object>();
            foreach (var name in dp.ParameterNames)
            {
                var paramInfo = (paramInfos as IDictionary);
                if (paramInfo == null)
                {
                    throw new InvalidOperationException();
                }
                var value = paramInfo[name];
                dictionary.Add(name, value.GetType().GetProperty("Value").GetValue(value));
            }
            return dictionary;
        }

        private static DynamicParameters ToDynamicParameters(object o)
        {
            return o as DynamicParameters;
        }

        [Fact]
        public void TestMultipleJoin()
        {
            var criteria = new TestMultipleJoinCriteria
                {
                    WithPersonsAndPersonInfo = true
                };
            var builder = new QueryBuilder<TestMultipleJoinCriteria>(criteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnPersonsHouseId , Persons.* , 0 as SplitOnPersonInfosPersonId , PersonInfos.* from Houses LEFT JOIN Persons on Persons.HouseId = Houses.Id INNER JOIN PersonInfos on PersonInfos.PersonId = Persons.Id",
                SimplifyString(query.Sql));
        }

        [Fact]
        public void BoolWhereCriteriaTrueTest()
        {
            var criteria = new TestWhereBoolCriteria
                {
                    OnlySingleStorey = true,
                };
            var builder = new QueryBuilder<TestWhereBoolCriteria>(criteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* from Houses WHERE (Houses.FloorsCount = 1)",
                SimplifyString(query.Sql));
        }

        [Fact]
        public void BoolWhereCriteriaFalseTest()
        {
            var criteria = new TestWhereBoolCriteria
                {
                    OnlySingleStorey = false,
                };
            var builder = new QueryBuilder<TestWhereBoolCriteria>(criteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* from Houses",
                SimplifyString(query.Sql));
        }


        [Fact]
        public void TableNameWithSquareBracketsTest()
        {
            var testCriteria = new SquareBracketsTableNameTestCriteria { TestPropertyId = 1 };
            var builder = new TestQueryBuilder<SquareBracketsTableNameTestCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select [TestTable].* from [TestTable] WHERE [TestTable].TestProperty = @TestTableTestPropertyId",
                SimplifyString(query.Sql));
        }

        [Table("[TestTable]")]
        private class SquareBracketsTableNameTestCriteria : Models.Criteria
        {
            [Where("TestProperty", WhereType = WhereType.Eq)]
            public int? TestPropertyId { get; set; }
        }

        private class FormatterTest : IFormatter
        {
            public void Format(ref object input)
            {
                input = "1";
            }
        }

        [Table(Name = "Houses")]
        private class TestAnotherJoinCriteria : Models.Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "Persons", JoinedTableField = "HouseId", Order = 1)]
            public bool WithPersons { get; set; }

            [SimpleJoin("OwnerId", JoinType.Inner, "Owners", JoinedTableField = "Id", Order = 2)]
            public bool WithOwner { get; set; }
        }

        [Table(Name = "TableName")]
        private class TestCriteria : Models.Criteria
        {
            [Where]
            public int? Id { get; set; }

            [Where(WhereType = WhereType.Like)]
            public string Name { get; set; }

            [Where("Date", WhereType = WhereType.GtEq)]
            public DateTime? DateFrom { get; set; }

            [Where("Date", WhereType = WhereType.LtEq)]
            public DateTime? DateTo { get; set; }

            [Where("Code", WhereType = WhereType.In)]
            public IEnumerable<string> Codes { get; set; }

            [Where("Date",
                Expression =
                    "(/**TableName**/./**FieldName**/ is not null and /**TableName**/./**FieldName**/ /**CompareOperation**/ /**Parameter**/)" +
                    " or " +
                    "(/**TableName**/.DateSecond /**CompareOperation**/ /**Parameter**/)",
                WhereType = WhereType.GtEq)]
            public DateTime? DateWithExpression { get; set; }

            [Where]
            [Format(typeof (FormatterTest))]
            public DateTime? DateTimeWithFormatter { get; set; }
        }

        [Table(Name = "TableName")]
        private class TestJoinCriteria : Models.Criteria
        {
            [SimpleJoin("CurrentTableId", JoinType.Left, "AnotherTable", JoinedTableField = "CurrentTableId")]
            public bool WithAnotherTable { get; set; }

            [Where]
            public int? Id { get; set; }
        }

        [Table(Name = "Persons")]
        private class TestJoinOrderCriteria : Models.Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "Houses", JoinedTableField = "PersonId")]
            public bool WithHouses { get; set; }

            [SimpleJoin("Id", JoinType.Left, "Airplans", JoinedTableField = "PersonId", Order = 2)]
            public bool WithAirplans { get; set; }

            [SimpleJoin("Id", JoinType.Left, "Cars", JoinedTableField = "PersonId", Order = 1)]
            public bool WithCars { get; set; }
        }

        [Table(Name = "TableName")]
        private class TestManyToManyJoinCriteria : Models.Criteria
        {
            [ManyToManyJoin("CurrentId", JoinType.Left, "AnotherTable", "AnotherTableCurrentTable", "CurrentId",
                "AnotherId", JoinedTableField = "AnotherId")]
            public bool WithAnotherTable { get; set; }

            [Where]
            public int? Id { get; set; }
        }

        [Table(Name = "Houses")]
        private class TestMultipleJoinCriteria : Models.Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "Persons", JoinedTableField = "HouseId", Order = 1)]
            [SimpleJoin("Id", JoinType.Inner, "PersonInfos", CurrentTable = "Persons", JoinedTableField = "PersonId",
                Order = 2)]
            public bool WithPersonsAndPersonInfo { get; set; }
        }

        private class TestQueryBuilder<T> : QueryBuilder<T> where T : Models.Criteria
        {
            public TestQueryBuilder(T criteria)
                : base(criteria)
            {
            }
        }

        [Table(Name = "Shipments")]
        private class TestSelectCriteria : Models.Criteria
        {
            [AddSelect]
            public string AddSelect { get; set; }

            [AddSelect(SelectColumns = "TableName:{{Sum(Shipments.Price)}}")]
            public bool WithSum { get; set; }
        }

        [Table(Name = "Houses")]
        private class TestWhereBoolCriteria : Models.Criteria
        {
            [Where("FloorsCount", Expression = "/**TableName**/./**FieldName**/ = 1")]
            public bool OnlySingleStorey { get; set; }
        }

        [Table(Name = "Houses")]
        private class TestWhereMultipleCriteria : Models.Criteria
        {
            [Where("OwnerId")]
            [Where("OwnerId", WhereType = WhereType.IsNotNull)]
            public Guid? HasOwnerNotThis { get; set; } 
        }

        [Fact]
        public void TestWhereMultiple()
        {
            var testCriteria = new TestWhereMultipleCriteria
            {
                HasOwnerNotThis = Guid.NewGuid()
            };
            var builder = new TestQueryBuilder<TestWhereMultipleCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* from Houses WHERE Houses.OwnerId = @HousesHasOwnerNotThis AND Houses.OwnerId is not null",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class TestIncludingCriteria : Models.Criteria
        {
            [SimpleJoin("OwnerId", JoinType.Left, "Owners", JoinedTableField = "Id", Including = "WithOwners", SelectColumns = "Owners:")]
            [Where("OwnerName", TableName = "Owners", WhereType = WhereType.Like)]
            public string OwnerName { get; set; }

            [SimpleJoin("OwnerId", JoinType.Left, "Owners", JoinedTableField = "Id")]
            public bool WithOwners { get; set; }
        }

        [Fact]
        public void TestIncludingIncludingFieldIsExists()
        {
            var testCriteria = new TestIncludingCriteria
            {
                OwnerName = "Vasya",
                WithOwners = true,
            };
            var builder = new TestQueryBuilder<TestIncludingCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnOwnersId , Owners.* from Houses LEFT JOIN Owners on Owners.Id = Houses.OwnerId WHERE Owners.OwnerName Like @OwnersOwnerName",
                SimplifyString(query.Sql));
        }

        [Fact]
        public void TestIncludingIncludingFieldIsNotExists()
        {
            var testCriteria = new TestIncludingCriteria
            {
                OwnerName = "Vasya",
                WithOwners = false,
            };
            var builder = new TestQueryBuilder<TestIncludingCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnOwnersId from Houses LEFT JOIN Owners on Owners.Id = Houses.OwnerId WHERE Owners.OwnerName Like @OwnersOwnerName",
                SimplifyString(query.Sql));
        }
        [Table("Houses")]
        private class JoinWithoutJoinedFieldCriteria : Models.Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Persons")]
            public bool WithPersons { get; set; } 
        }

        [Fact]
        public void JoinWithoutJoinedField()
        {
            var testCriteria = new JoinWithoutJoinedFieldCriteria
            {
                WithPersons = true
            };
            var builder = new TestQueryBuilder<JoinWithoutJoinedFieldCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnPersonsHouseId , Persons.* from Houses LEFT JOIN Persons on Persons.HouseId = Houses.HouseId",
                SimplifyString(query.Sql));
        }

        [Fact]
        public void JoinNoSplitTest()
        {
            var testCriteria = new JoinNoSplitCriteria
            {
                OwnerId = 1
            };
            var builder = new TestQueryBuilder<JoinNoSplitCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnOwnersId from Houses LEFT JOIN HouseOwners on HouseOwners.HouseId = Houses.Id WHERE HouseOwners.OwnerId = @HouseOwnersOwnerId",
                SimplifyString(query.Sql));
            Assert.Equal("SplitOnOwnersId", query.SplitOn);
            
        }

        [Table("Houses")]
        private class JoinNoSplitCriteria : Models.Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "HouseOwners", JoinedTableField = "HouseId", SelectColumns = "HouseOwners:", NoSplit = true, Including = "WithOwners")]
            [Where("OwnerId", TableName = "HouseOwners")]
            public int? OwnerId { get; set; }

            [ManyToManyJoin("Id", JoinType.Left, "Owners", "HouseOwners", "HouseId", "OwnerId", JoinedTableField = "Id")]
            public bool WithOwners { get; set; }
        }

        [Fact]
        public void JoinNoSplitTest2()
        {
            var testCriteria = new JoinNoSplitCriteria2
            {
                OwnerId = 1
            };
            var builder = new TestQueryBuilder<JoinNoSplitCriteria2>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* from Houses LEFT JOIN HouseOwners on HouseOwners.HouseId = Houses.Id WHERE HouseOwners.OwnerId = @HouseOwnersOwnerId",
                SimplifyString(query.Sql));
            Assert.Equal("", query.SplitOn);

        }

        [Table("Houses")]
        private class JoinNoSplitCriteria2 : Models.Criteria
        {
            [SimpleJoin("Id", JoinType.Left, "HouseOwners", JoinedTableField = "HouseId", SelectColumns = "HouseOwners:", NoSplit = true)]
            [Where("OwnerId", TableName = "HouseOwners")]
            public int? OwnerId { get; set; }
        }

        [Table("Houses")]
        private class JoinAddOnCriteria : Models.Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Owners", AddOnClause = "Owners.OwnerId in (1,2,3)", SelectColumns = "Owners:")]
            public bool WithOwners { get; set; }
        }

        [Fact]
        public void JoinAddOnTest()
        {
            var testCriteria = new JoinAddOnCriteria
            {
                WithOwners = true
            };
            var builder = new TestQueryBuilder<JoinAddOnCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnOwnersHouseId from Houses LEFT JOIN Owners on Owners.HouseId = Houses.HouseId AND Owners.OwnerId in (1,2,3)",
                SimplifyString(query.Sql));
            
        }

        [Fact]
        public void JoinAddOnTypeTest()
        {
            var testCriteria = new JoinAddOnTypeCriteria
            {
                WithPeople = true
            };
            var builder = new TestQueryBuilder<JoinAddOnTypeCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnPeoplePeopleId , People.* from Houses LEFT JOIN HousePeople on HousePeople.HouseId = Houses.HouseId AND HousesPeople.Required = 1 LEFT JOIN People on People.PeopleId = HousePeople.PeopleId",
                SimplifyString(query.Sql));

        }

        [Table("Houses")]
        private class JoinAddOnTypeCriteria : Models.Criteria
        {
            [ManyToManyJoin("HouseId", JoinType.Left, "People", "HousePeople", "HouseId", "PeopleId", JoinedTableField = "PeopleId", AddOnType = AddOnType.ForCommunication, AddOnClause = "HousesPeople.Required = 1")]
            public bool WithPeople { get; set; }
        }

        [Fact]
        public void SelectNullableTest()
        {
            var testCriteria = new SelectNullableCriteria
                {
                    Id = 1
                };
            var builder = new TestQueryBuilder<SelectNullableCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , Houses.Name from Houses WHERE Houses.Id = @HousesId",
                SimplifyString(query.Sql));
        }

        [Fact]
        public void SelectNullableNullTest()
        {
            var testCriteria = new SelectNullableCriteria
            {
            };
            var builder = new TestQueryBuilder<SelectNullableCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* from Houses",
                SimplifyString(query.Sql));
        }


        [Table("Houses")]
        private class SelectNullableCriteria : Models.Criteria
        {
            [Where]
            [AddSelect(SelectColumns = "Houses:Name")]
            public int? Id { get; set; }
        }

        [Fact]
        public void JoinSelectTest()
        {
            var testCriteria = new JoinSelectCriteria
            {
                WithOwners = true
            };
            var builder = new TestQueryBuilder<JoinSelectCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* , 0 as SplitOnOwnersHouseId , Owners.Name , Owners.Id , Type as OwnerType from Houses LEFT JOIN Owners on Owners.HouseId = Houses.HouseId",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class JoinSelectCriteria : Models.Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Owners", SelectColumns = "Owners:Name,Id,{{Type as OwnerType}}")]
            public bool WithOwners { get; set; }
        }

        [Fact]
        public void JoinReferenceTest()
        {
            var testCriteria = new JoinReferenceCriteria
            {
                OwnerIds = new List<int>()
                    {
                        1,2,3,4
                    }
            };
            var builder = new TestQueryBuilder<JoinReferenceCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Houses.* from Houses LEFT JOIN Owners on Owners.HouseId = Houses.HouseId WHERE Owners.Id in @OwnersOwnerIds",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class JoinReferenceCriteria : Models.Criteria
        {
            [SimpleJoin("HouseId", JoinType.Left, "Owners", NoSplit = true, SelectColumns = "Owners:")]
            [Where("Id", TableName = "Owners", WhereType = WhereType.In)]
            public IEnumerable<int> OwnerIds { get; set; }
        }

        [Fact]
        public void GroupByTest()
        {
            var testCriteria = new GroupByCriteria
            {
                GroupBy = new []{"Houses.OwnerId", "Houses.Category"},
                SelectClause = new SelectClause
                    {
                        Select = "Count(1) , Houses.OwnerId , Houses.Category",
                        IsExpression = true
                    }
            };
            var builder = new TestQueryBuilder<GroupByCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select Count(1) , Houses.OwnerId , Houses.Category from Houses GROUP BY Houses.OwnerId , Houses.Category",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class GroupByCriteria : Models.Criteria
        {
        }

        [Fact]
        public void BaseTest()
        {
            var testCriteria = new RealCriteria
            {
                Id = Guid.NewGuid(),
                CustomerId = 1,
                WithCustomers = true
            };
            var builder = new TestQueryBuilder<BaseCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select RealHouses.* , 0 as SplitOnCustomersCustomerId , Customers.* from RealHouses INNER JOIN Customers on Customers.CustomerId = RealHouses.CustomerId WHERE RealHouses.HouseId = @RealHousesId AND RealHouses.CustomerId = @RealHousesCustomerId",
                SimplifyString(query.Sql));
        }

        [Fact]
        public void SumTest()
        {
            var testCriteria = new SumCriteria
            {
                Ids = new []{1,2,3},
                QueryType = QueryType.Sum,
                SelectClause = new SelectClause("sum(Houses.Price)"),
                WithCustomers = true,
            };
            var builder = new TestQueryBuilder<SumCriteria>(testCriteria);
            var query = builder.Build();
            Assert.Equal(
                "Select sum(Houses.Price) from Houses INNER JOIN Customers on Customers.CustomerId = Houses.CustomerId WHERE Houses.Id in @HousesIds",
                SimplifyString(query.Sql));
        }

        [Table("Houses")]
        private class SumCriteria : Models.Criteria
        {
            [Where("Id", WhereType = WhereType.In)]
            public IEnumerable<int> Ids { get; set; }

            [SimpleJoin("CustomerId", JoinType.Inner, "Customers")]
            public bool WithCustomers { get; set; }
        }

        private abstract class BaseCriteria : Models.Criteria
        {
            public abstract Guid? Id { get; set; }

            [Where]
            public int? CustomerId { get; set; }

            [SimpleJoin("CustomerId", JoinType.Inner, "Customers")]
            public bool WithCustomers { get; set; }
        }

        [Table("RealHouses")]
        private class RealCriteria : BaseCriteria
        {
            [Where("HouseId")]
            public override sealed Guid? Id { get; set; }
        }
    }
}