## 断开连接的实体和实体图

在 Entity Framework Core (EF Core) 中，**断开连接的实体（Disconnected Entities）**指的是那些不在
当前 `DbContext` 实例上下文中的实体。这通常发生在Web应用程序中，客户端获取实体、修改它们后将它们发送回服务器保存的情况下。

要处理断开连接的实体和实体图（包括多个相关的实体），需要将实体重新附加到具有正确实体状态的 DbContext，以允许 EF Core 跟踪更改并
执行必要的数据库操作。这需要使用 `context.Entry(entity).State = EntityState.[State]` 显式设置实体的状态，其中 [State] 可以为 
**Added**、**Modified** 或 **Deleted**。然后，调用 `SaveChanges()`以将更改持久化到数据库。

我们可以使用 Attach、Add 和 Entry 方法将断开连接的实体、实体图附加到上下文对象，还可以更改断开连接的实体图中每个实体的 EntityState 。

- `Attach(entry)` 将整个实体图附加到上下文，实体状态默认为 Unchanged，具有空键值的子实体为 Added 。
- `Entry(entry).State = EntityState.Added` 将根实体附加到上下文，可设置根实体状态，忽略所有子实体。
- `Add(entry)` 方法将实体图附加到上下文，设置所有实体状态为 Added 。
- `ChangeTracker.TrackGraph` 可以遍历整个实体图并根据需要设置每个实体的状态。

### 处理断开连接实体示例

使用 Add、Entry 插入或借助已跟踪实体（图）进行更新：
```csharp
public static void InsertOrUpdate(BloggingContext context, Blog blog)
{
    var existingBlog = context.Blogs.Find(blog.BlogId);
    if (existingBlog == null)
    {
        context.Add(blog);
    }
    else
    {
        context.Entry(existingBlog).CurrentValues.SetValues(blog);
    }

    context.SaveChanges();
}
```

使用 Attach 将实体（图）附加到上下文再进行插入，如果 `blog` 是实体图，则 `blog` 中含键值的子实体默认不参与更新：
```csharp
public static void Insert(Blog blog)
{
    using var context = new EFCoreDbContext();
    context.Attach(blog).State = EntityState.Added;
    // if blog has `Address` Child Entity，it will have Unchanged state. 
    context.SaveChanges();
}
```

为每个实体单独设置追踪：
```csharp
public static void Update(StudentsContext context)
{
    var student = new Student()
    {
        // ...
        Standard = new Standard()   //Child Entity with key value
        {
            StandardId = 1,
            // ...
        },
        StudentAddress = new StudentAddress() //Child Entity with Empty Key
        {
            // ...
        }
    };

    context.ChangeTracker.TrackGraph(student, nodeEntry =>
    {
        //Setting the Root Entity
        if (nodeEntry.Entry.Entity is Student std)
        {
            if (std.StudentId > 0)
            {
                nodeEntry.Entry.State = EntityState.Modified;
            }
            else
            {
                nodeEntry.Entry.State = EntityState.Added;
            }
        }
        //Setting the Child Entity
        else
        {
            if (nodeEntry.Entry.IsKeySet)
            {
                nodeEntry.Entry.State = EntityState.Unchanged;
            }
            else
            {
                nodeEntry.Entry.State = EntityState.Added;
            }
        }
    });

    context.SaveChanges();
}
```