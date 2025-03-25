## EF Core 事务

| 事务方法 | 使用场景 | 优点 | 缺点 |
|----------|----------|------|------|
| 隐式事务 | 单个 `DbContext` 内的简单操作和单个 `SaveChanges()` 调用 | 易于实现，代码简洁 | 对事务边界控制有限 |
| 手动事务 | 在单个 `DbContext` 内分组多个操作或多个 `SaveChanges()` 调用 | 对事务流程有更大的控制 | 更冗长，需要仔细管理 |
| TransactionScope | 涉及多个 `DbContext` 实例或多个数据库/资源的操作 | 支持分布式事务 | 开销大，需要提供程序支持 |
| UseTransaction | 在多个 `DbContext` 实例间共享现有事务 | 启用跨上下文的事务共享 | 事务生命周期管理复杂 |
| 异步事务 | 高并发或 I/O 绑定操作，如 Web 应用中的操作 | 非阻塞，提高响应性 | 需要异步编程模式 |

### 默认事务行为
默认情况下，如果数据库提供程序支持事务，则会在事务中应用对 `SaveChanges` 的单一调用中的所有更改。如果其中有任何更改失败，则会回滚事务且所有更改都不会应用到数据库。
这意味着，`SaveChanges` 可保证完全成功，或在出现错误时不修改数据库。

对于大多数应用程序，此默认行为已足够 如果应用程序要求被视为有必要，则应该仅手动控制事务。

### 控制事务
可以使用 `DbContext.Database` API 开始、提交和回滚事务。示例：
```csharp
using var context = new BloggingContext();
using var transaction = context.Database.BeginTransaction();

try
{
    context.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/dotnet" });
    context.SaveChanges();

    context.Blogs.Add(new Blog { Url = "http://blogs.msdn.com/visualstudio" });
    context.SaveChanges();

    var blogs = context.Blogs
        .OrderBy(b => b.Url)
        .ToList();

    // Commit transaction if all commands succeed, transaction will auto-rollback
    // when disposed if either commands fails
    transaction.Commit();
}
catch (Exception)
{
    // TODO: Handle failure
}
```

### 使用 TransactionScope 的分布式事务

当操作跨多个 DbContext 实例或不同的数据库时，TransactionScope 可以管理分布式事务。分布式事务涉及更多的开销，应仅在必要时使用。
```csharp
// Enable implicit distributed transactions in case operations span multiple databases
TransactionManager.ImplicitDistributedTransactions = true;

// Start a TransactionScope to encompass operations across different DbContexts
using (var scope = new TransactionScope(
    TransactionScopeOption.Required, // Requires a new transaction or joins an existing one
    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted })) // Enables async operations within the transaction
{
    try
    {
        int generatedOrderId; // for logging purposes

        using (var orderContext = new ECommerceDbContext())
        {
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                TotalAmount = 300.00m,
            };

            orderContext.Orders.Add(order);
            orderContext.SaveChanges();
            generatedOrderId = order.OrderId;
        }

        using (var logContext = new LoggingDbContext())
        {
            var log = new OrderLog
            {
                OrderId = generatedOrderId,
                LogDate = DateTime.UtcNow,
                Message = "Order placed successfully."
            };

            logContext.OrderLogs.Add(log);
            logContext.SaveChanges();
        }

        scope.Complete();
    }
    catch (DbUpdateException ex)
    {
        Console.WriteLine($"Database error occurred: {ex.InnerException?.Message ?? ex.Message}");
        // The transaction will be rolled back automatically upon disposal if Complete() is not called.
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        // The transaction will be rolled back automatically upon disposal if Complete() is not called.
    }
    // The transaction will be rolled back automatically upon disposal if Complete() is not called.
}
```