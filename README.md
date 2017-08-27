Dapper.Criteria [![Build status](https://ci.appveyor.com/api/projects/status/fh37rvrq5jyvw14o?svg=true)](https://ci.appveyor.com/project/spzSource/dapper-criteria)
==========================
Library lets to build sql queries for Dapper in a declarative manner by using criteria class with attributes applied to properties.


Examples
==========================

### Where criteria:

```cs
[Table(Name = "TableName", Alias = "[tn]")]
public class TestWhereCriteria : Models.Criteria
{
    [Where]
    public int? Id { get; set; }

    [Where(WhereType = WhereType.Like)]
    public string Name { get; set; }
}
```

```cs
QueryBuilder<TestWhereCriteria> builder = new QueryBuilder<TestWhereCriteria>(
    new TestWhereCriteria
    {
        Id = 1,
        Name = "Lala"
    });

Query query = builder.Build();
 
```
Produced sql:

```sql
SELECT 
    [tn].* 
FROM TableName [tn]  
WHERE [tn].Id = @tnId  
    AND [tn].Name Like @tnName 
```

### SimpleJoin criteria:

```cs
[Table(Name = "Persons", Alias = "[p]")]
internal class TestJoinCriteria : Models.Criteria
{
    [SimpleJoin(
        "Id", 
        JoinType.Left, 
        "Houses",
        "[h]",
        JoinedTableField = "PersonId")]
    public bool WithHouses { get; set; }

    [SimpleJoin(
        "Id", 
        JoinType.Left, 
        "Airplans", 
        "[a]",
        JoinedTableField = "PersonId", 
        Order = 2)]
    public bool WithAirplans { get; set; }

    [SimpleJoin(
        "Id", 
        JoinType.Left, 
        "Cars", 
        "[c]",
        JoinedTableField = "PersonId", 
        Order = 1)]
    public bool WithCars { get; set; }

    [SimpleJoin(
        "InstrId",
        JoinType.Left,
        "Instruments",
        "[i]",
        JoinedTableField = "Instrument",
        CurrentTableAlias = "[c]",
        CurrentTable = "Cars")]
    public bool WithInstruments { get; set; }
}
```

```cs
QueryBuilder<TestJoinCriteria> builder = new QueryBuilder<TestJoinCriteria>(
    new TestJoinCriteria
    {
        WithCars = true,
        WithHouses = true,
        WithAirplans = true,
        WithInstruments = true
    });

Query query = builder.Build();

```
Produced sql:

```sql
SELECT 
    [p].* , 0 as SplitOnCarsPersonId , 
    [c].* , 0 as SplitOnAirplansPersonId , 
    [a].* , 0 as SplitOnHousesPersonId , 
    [h].* , 0 as SplitOnInstrumentsInstrument , 
    [i].* 
FROM Persons [p]
    LEFT JOIN Cars [c] on [c].PersonId = [p].Id
    LEFT JOIN Airplans [a] on [a].PersonId = [p].Id
    LEFT JOIN Houses [h] on [h].PersonId = [p].Id
    LEFT JOIN Instruments [i] on [i].Instrument = [c].InstrId
```

### Many-to-many join criteria:

```C#
[Table(Name = "Persons", Alias = "[p]")]
public class TestManyToManyJoinCriteria : Models.Criteria
{
    [ManyToManyJoin(
        currentTableField: "CompanyId", 
        joinType: JoinType.Left, 
        joinedTable: "Company", 
        joinedTableAlias: "[c]",
        communicationTable: "CompanyPersons", 
        communicationTableAlias: "[cp]",
        communicationTableCurrentTableField: "PersonId",
        communicationTableJoinedTableField: "CompanyId", 
        JoinedTableField = "Id")]
    public bool WithCompany { get; set; }

    [Where]
    public int? Id { get; set; }
}
```

```cs
QueryBuilder<TestManyToManyJoinCriteria> builder =
    new QueryBuilder<TestManyToManyJoinCriteria>(
        new TestManyToManyJoinCriteria
        {
            Id = 1,
            WithCompany = true
        });

Query query = builder.Build();
```
Produced sql:
```sql
SELECT 
    [p].* , 0 as SplitOnCompanyId , 
    [c].* 
FROM Persons [p]
    LEFT JOIN CompanyPersons [cp] on [cp].PersonId = [p].CompanyId
    LEFT JOIN Company [c] on [c].Id = [cp].CompanyId
WHERE [p].Id = @pId
```
