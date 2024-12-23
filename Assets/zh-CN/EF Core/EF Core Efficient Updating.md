# EF Core 高效操作

EF Core 目前支持 `SaveChanges` 更新批处理和 ExecuteUpdate、ExecuteDelete 操作，也可以通过
 Z.EntityFramework.Extensions.EFCore 、EFCore.BulkExtensions 插件支持批量操作。

## 批处理

在多数情况下，EF Core 通过在一次往返中自动将所有更新批处理在一起，帮助最大限度地减少数据库往返次数。操作上下文时的CRUD操作，EF Core 不会逐个发送，而是在内部跟踪这些更改，并在调用 `SaveChanges()` 时在单个往返中执行这些更改。EF 在一次往返中批处理的语句数量取决于所使用的数据库提供程序，默认在单个批处理中最多执行 42 条语句。

## ExecuteUpdate 和 ExecuteDelete （EF Core 7.0 引入）
使用 `SaveChanges()` 和更改跟踪保存更改具备很多优势，但可能涉及大量实体操作。ExecuteUpdate 和 ExecuteDelete 是无需使用 EF 的传统更改跟踪和 `SaveChanges()` 将数据保存到数据库的方法。

ExecuteUpdate 和 ExecuteDelete 具有以下限制：
- 在调用时立即生效，多个调用不支持批处理。
- 不会隐式启动调用它们的事务。
- 无法自动应用并发控制，但可利用受操作影响的行数进行控制。
- 仅适用于关系数据库提供程序。
- 由于数据库限制，通常只允许使用 UPDATE 或 DELETE 修改单个表。

### ExecuteDelete 示例
假设需要删除评分低于特定阈值的所有博客。
```csharp
// SaveChanges
foreach (var blog in context.Blogs.Where(b => b.Rating < 3))
{
    context.Blogs.Remove(blog);
}

context.SaveChanges();

// ExecuteDelete API
context.Blogs.Where(b => b.Rating < 3).ExecuteDelete();
```
使用传统方式执行此任务的步骤：在数据库中查询与筛选器匹配的所有博客，然后查询、具体化和跟踪所有实例。告知 EF 的更改跟踪器，删除每个博客，调用 SaveChanges() 应用更改，浙江为每个博客生成 DELETE 语句。
通过 ExecuteDelete API 执行任务将生成SQL DELETE命令：
```sql
DELETE FROM [b]
FROM [Blogs] AS [b]
WHERE [b].[Rating] < 3
```
此操作在数据库中高效执行，无需从数据库加载数据，也不涉及更改跟踪器。

### ExecuteUpdate 示例
更新多个属性。
```csharp
context.Blogs
    .Where(b => b.Rating < 3)
    .ExecuteUpdate(setters => setters
        .SetProperty(b => b.IsVisible, false)
        .SetProperty(b => b.Rating, b => b.Rating + 1));
```
执行的SQL
```sql
UPDATE [b]
SET [b].[IsVisible] = CAST(0 AS bit),
    [b].[Rating] = [b].[Rating] + 1
FROM [Blogs] AS [b]
WHERE [b].[Rating] < 3
```
