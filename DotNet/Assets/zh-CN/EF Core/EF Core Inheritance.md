## 继承

- [继承](#继承)
  - [实体类型层次结构映射](#实体类型层次结构映射)
  - [每个层次结构一张表和鉴别器配置](#每个层次结构一张表和鉴别器配置)
  - [每个类型一张表配置](#每个类型一张表配置)
  - [每个具体类型一张表配置](#每个具体类型一张表配置)
  - [键生成](#键生成)
  - [外键约束](#外键约束)
  - [总结和指南](#总结和指南)

EF 可以将 .NET 类型层次结构映射到数据库。这允许你像往常一样使用基类型和派生类型在代码中编写 .NET 实体，并让 EF 无缝创建适当的数据库架构、发出查询等。

### 实体类型层次结构映射

按照约定，EF 不会自动扫描基类型或派生类型，如果要映射层次结构中的 CLR 类型，就必须在模型上显式指定该类型。

以下示例将为 `Blog` 及其子类 `RssBlog` 公开 DbSet。如果 `Blog` 有任何其他子类，它不会包含在模型中。

```csharp
internal class MyContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<RssBlog> RssBlogs { get; set; }
}

public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }
}

public class RssBlog : Blog
{
    public string RssUrl { get; set; }
}
```

如果不依赖约定，则可以使用 `HasBaseType` 显式指定基类型。还可以使用 `.HasBaseType((Type)null)` 从层次结构中删除实体类型。


### 每个层次结构一张表和鉴别器配置

默认情况下，EF 使用每个层次结构一张表 (TPH) 模式来映射继承。TPH 使用单个表来存储层次结构中所有类型的数据，并使用鉴别器列来标识每行表示的类型。

- 单个表：层次结构中的所有实体都存储在单个数据库表中，架构简单。
- 鉴别器列：此列区分实体类型。
- 可为 Null 的列：特定于派生类型的属性可能会导致数据库中实体的某些列值为 null，可导致稀疏性。

实现 TPH 继承：
1. 创建基类和派生类：定义共享属性的基类和特定属性的派生类。
2. 配置鉴别器列（可选）：EF Core 会自动添加鉴别器列，也可以使用 Fluent API 自定义。
3. 使用 Fluent API 配置 TPH：使用 Fluent API 指定继承映射的发生方式。使用 `HasDiscriminator()` 方法定义每个派生类的鉴别器列和值。

`Blog` 默认将映射到以下数据库架构，其中 `Discriminator` 是自动添加的鉴别器列影子属性，标识了每行中存储的 `Blog` 类型：

| BlogId | Discriminator | Url                                      | RssUrl                                       |
|--------|---------------|------------------------------------------|----------------------------------------------|
| 1      | Blog          | http://blogs.msdn.com/dotnet             | NULL                                         |
| 2      | RssBlog       | http://blogs.msdn.com/adonet             | http://blogs.msdn.com/b/adonet/atom.aspx     |

可选自定义鉴别器列以及用于标识层次结构中每种类型的值，也可以将鉴别器映射到实体中的常规 .NET 属性：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>()
        .HasDiscriminator<string>("blog_type")
        .HasValue<Blog>("blog_base")
        .HasValue<RssBlog>("blog_rss");
    /*
    modelBuilder.Entity<Blog>()
        .HasDiscriminator(b => b.BlogType);
    */
}
```

查询使用 TPH 模式的派生实体时，EF Core 会在查询中添加一个基于鉴别器列的谓词。如果有数据库中的行具有未映射到 EF 
模型的鉴别器值，将引发异常，可以将 EF Core 模型中的鉴别器映射标记为不完整，以确保总是添加筛选器谓词来查询层次结构中的任意类型：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>()
        .HasDiscriminator()
        .IsComplete(false);
}
```

默认情况下，当层次结构中的两个同级实体类型具有同名的属性时，它们将映射到两个单独的列。但是，如果它们的类型相同，
则可以映射到相同的数据库列：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>()
        .Property(b => b.Url)
        .HasColumnName("Url");

    modelBuilder.Entity<RssBlog>()
        .Property(b => b.Url)
        .HasColumnName("Url");
}
```

备注

使用强制转换查询共享列时，关系数据库提供程序（例如 SQL Server）不会自动使用鉴别器谓词。查询 `Url = (blog as RssBlog).Url` 
将返回同级 `Blog` 行的 `Url` 值。若要将查询限制为 `RssBlog` 实体，需要在鉴别器上手动添加筛选器，例如
 `Url = blog is RssBlog ? (blog as RssBlog).Url : null`。

### 每个类型一张表配置

在 TPT 映射模式中，所有类型都分别映射到各自的表。基类属性存储在一个表中，映射到派生类的表仅存储基类中不存在的其他属性，并存储外键来联接派生表与基表。

- 每个类的单独表：继承层次结构中的每个实体都存储在其自己的数据库表中。
- 外键关系：派生类表有一个主键，该主键也是引用基表主键的外键，形成一对一关系。
- 无 Null 列：每个表仅包含该类的属性，规范化架构，无 null 列。
- 性能通常低于 TPH 模式。

实现 TPT 继承：
1. 创建基类和派生类：定义共享属性的基类和特定属性的派生类。
2. 使用 Fluent API 配置 TPT：使用 Fluent API 指定层次结构中的每个实体都使用 `ToTable()` 方法映射到其自己的表，或者对每个根实体类型调用 `modelBuilder.Entity<Blog>().UseTptMappingStrategy()`，表名自动生成。

```csharp
modelBuilder.Entity<Blog>().ToTable("Blogs");
modelBuilder.Entity<RssBlog>().ToTable("RssBlogs");
```

EF 将为上述模型创建以下数据库架构。

```sql
CREATE TABLE [Blogs] (
    [BlogId] int NOT NULL IDENTITY,
    [Url] nvarchar(max) NULL,
    CONSTRAINT [PK_Blogs] PRIMARY KEY ([BlogId])
);

CREATE TABLE [RssBlogs] (
    [BlogId] int NOT NULL,
    [RssUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_RssBlogs] PRIMARY KEY ([BlogId]),
    CONSTRAINT [FK_RssBlogs_Blogs_BlogId] FOREIGN KEY ([BlogId]) REFERENCES [Blogs] ([BlogId]) ON DELETE NO ACTION
);
```

如果重命名主键约束，新名称将应用于映射到层次结构的所有表。

如果使用批量配置，可以通过调用 [GetColumnName(IProperty, StoreObjectIdentifier)](https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.relationalpropertyextensions.getcolumnname#microsoft-entityframeworkcore-relationalpropertyextensions-getcolumnname(microsoft-entityframeworkcore-metadata-iproperty-microsoft-entityframeworkcore-metadata-storeobjectidentifier@)) 来检索特定表的列名。

```csharp
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    var tableIdentifier = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);

    Console.WriteLine($"{entityType.DisplayName()}\t\t{tableIdentifier}");
    Console.WriteLine(" Property\tColumn");

    foreach (var property in entityType.GetProperties())
    {
        var columnName = property.GetColumnName(tableIdentifier.Value);
        Console.WriteLine($" {property.Name,-10}\t{columnName}");
    }

    Console.WriteLine();
}
```

### 每个具体类型一张表配置

EF Core 7.0 中引入了每个具体类型一张表 (TPC) 功能。

TPC 策略类似于 TPT 策略，为层次结构中每个具体类型创建不同的表，但不为抽象类型创建表。与 TPT 一样，表本身指示已保存对象的类型。
但是，与 TPT 映射不同，每个表都包含具体类型及其基类型中每个属性的列。TPC 数据库架构是非规范化的。

- 每个具体类的单独表：层次结构中的每个具体类都映射到其自己的表，抽象基类无表。
- 完整的实体表示：每个表都完全表示一个具体实体，包括从基类继承的属性。
- 无需联接：针对派生类型的查询不需要联接，这可能会提高性能。
- 无外键关系：基类和派生类之间没有关系或外键。
- 无判别器列：不需要鉴别器列来标识实体的类型。
- 无 Null 列：TPC 通过确保每个表仅包含与其实体相关的列来避免 null 列。

实现 TPC 继承：
1. 创建基类和派生类：定义共享属性的基类和特定属性的派生类。
2. 使用 `UseTpcMappingStrategy()` 方法：在 ModelBuilder 上调用此方法以在基本实体上启用 TPC 映射。
3. 使用 Fluent API 配置 TPC：使用 OnModelCreating 方法中的 Fluent API 指定每个具体类应使用 `ToTable()` 方法映射到其自己的表。

```csharp
modelBuilder.Entity<Blog>().UseTpcMappingStrategy()
    .ToTable("Blogs");
modelBuilder.Entity<RssBlog>()
    .ToTable("RssBlogs");
```

EF 将为上述模型创建以下数据库架构。

```sql
CREATE TABLE [Blogs] (
    [BlogId] int NOT NULL DEFAULT (NEXT VALUE FOR [BlogSequence]),
    [Url] nvarchar(max) NULL,
    CONSTRAINT [PK_Blogs] PRIMARY KEY ([BlogId])
);

CREATE TABLE [RssBlogs] (
    [BlogId] int NOT NULL DEFAULT (NEXT VALUE FOR [BlogSequence]),
    [Url] nvarchar(max) NULL,
    [RssUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_RssBlogs] PRIMARY KEY ([BlogId])
);
```

对每个根实体类型调用 `modelBuilder.Entity<Blog>().UseTpcMappingStrategy()` 可按照约定生成表名。

要为每个表中的主键列配置不同的列名，请参阅[特定于表的方面配置](https://learn.microsoft.com/zh-cn/ef/core/modeling/table-splitting#table-specific-facet-configuration)。

### 键生成

选择的继承映射策略对如何生成和管理主键值产生了影响。

TPH 中的键很简单，因为每个实体实例都由单个表中的单个行表示。可以使用任何类型的键值生成，无需其他约束。

对于 TPT 策略，表中始终有一行映射到层次结构的基类型。此行可以使用任何类型的键生成，其他表的键使用外键约束链接到此表。

对于 TPC 来说，事情会变得更加复杂。首先，必须了解 EF Core 要求层次结构中的所有实体都必须具有唯一键值，即使实体具有不同的类型也是如此。
其次，与 TPT 不同，没有一个通用表可以作为存放和生成键值的单一位置。这意味着无法使用简单的 `Identity` 列。

对于支持序列的数据库，可以使用每个表的默认约束中引用的单个序列来生成键值。例如：

```sql
[Id] int NOT NULL DEFAULT (NEXT VALUE FOR [AnimalSequence])
```

`AnimalSequence` 是由 EF Core 创建的数据库序列。使用适用于 SQL Server 的 EF Core 数据库提供程序时，此策略默认用于 TPC 层次结构。
支持序列的其他数据库的数据库提供程序应具有类似的默认值。使用序列的其他键生成策略（如 Hi-Lo 模式）也可用于 TPC。

虽然标准标识列不适用于 TPC，但如果每个表都配置了适当的种子和增量，则可以使用标识列，以便为每个表生成的值永远不会冲突。例如：

```csharp
modelBuilder.Entity<Cat>().ToTable("Cats", tb => tb.Property(e => e.Id).UseIdentityColumn(1, 4));
modelBuilder.Entity<Dog>().ToTable("Dogs", tb => tb.Property(e => e.Id).UseIdentityColumn(2, 4));
modelBuilder.Entity<FarmAnimal>().ToTable("FarmAnimals", tb => tb.Property(e => e.Id).UseIdentityColumn(3, 4));
modelBuilder.Entity<Human>().ToTable("Humans", tb => tb.Property(e => e.Id).UseIdentityColumn(4, 4));
```

使用此策略会使以后添加派生类型变得更加困难，因为它需要事先知道层次结构中的类型总数。

### 外键约束

TPC 映射策略会创建非规范化 SQL 架构。使用 TPC 时，任何给定类型的主键存储在该类型的具体子类型对应的表中。例如，`Cats.Id`、`Dogs.Id` 主键等。这意味着
无法为此关系创建 FK 约束。实际上，只要应用程序使用导航来关联实体，则可以保证 FK 列将随时包含有效的 PK 值。

### 总结和指南

TPH 通常适用于大多数应用程序，是一个很好的默认值，因此，除非需要，不要使用 TPC 来增加复杂性。

当代码主要查询单个叶子类型的实体并且其基准测试与 TPH 相比有所改进时，TPC 也可使用。

仅当受外部因素约束时，才使用 TPT。