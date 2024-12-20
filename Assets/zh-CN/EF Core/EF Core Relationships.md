
## Entity Framework Core 实体关系和对应实现

下面列出了Entity Framework Core 实体关系和不同方式的实现。

### 1. 一对一关系
#### 使用默认命名约定：
```csharp
// Principal (parent)
public class Blog
{
    public int Id { get; set; }
    public BlogHeader? Header { get; set; } // Navigation property
}
// Dependent (child)
public class BlogHeader
{
    public int Id { get; set; }
    public int BlogId { get; set; } // Required foreign key property
    public Blog Blog { get; set; } = null!; // Required reference navigation
}
```
#### 使用Fluent API：
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>()
        .HasOne(e => e.Header)
        .WithOne(e => e.Blog)
        .HasForeignKey<BlogHeader>(e => e.BlogId)
        .IsRequired();
}
```
#### 一对一关系中的Shared Primary Key：
当一对一关系中的依赖实体的外键，同时被配置为它本身的主键，该键称为共享主键，这将使得：
- 保证对于每个主体实体，只有一个依赖实体，反之亦然。
- 只需要依赖表中的一列用作主键和外键。
- 简化查询并减少冗余，只有一个键列用于引用关系。
### 2. 一对多关系
#### 使用默认命名约定：
```csharp
public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Post> Posts { get; }  // Navigation property
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; } //FK, Required Relationship
    public Blog Blog { get; set; }  // Navigation property
}
```
#### 使用DataAnnotations：
```csharp
public class Blog
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Post> Posts { get; }  // Navigation property
}

public class Post
{
    [key]
    public int Id { get; set; }
    public string Content { get; set; }
    public int BlogId { get; set; } //FK, Required Relationship
    [ForeignKey("BlogId")]
    public Blog Blog { get; set; }  // Navigation property
}
```
#### 使用Fluent API：
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Blog>()
        .HasMany(e => e.Posts)
        .WithOne(e => e.Blog)
        .HasForeignKey(e => e.BlogId)
        //.HasPrincipalKey(e => e.Id);  //  optional
        .OnDelete(DeleteBehavior.Cascade); // This will delete the child record(s) when parent record is deleted
}
```
DeleteBehavior 枚举值：

**级联删除 (Cascade)**

主实体被删除时，所有相关的依赖实体也将被删除。在EF Core和数据库中强制执行。

**客户端设置为空 (ClientSetNull)**

当主实体被删除时，内存中依赖实体的外键值将被设置为 `Null`，如果数据库对应列不可为 `Null`，调用 `SaveChanges()` 时将抛出异常。

**设置为空 (SetNull)**

当主实体被删除时，依赖实体中的外键值、数据库列将被设置为 `Null`。在EF Core和数据库中强制执行。

**限制 (Restrict)**

在有相关依赖实体存在时，主实体不可删除。在EF Core和数据库中强制执行。

**客户端级联删除 (ClientCascade)**

当主实体被删除时，依赖实体将在内存中被删除，数据库保留。

**无操作 (NoAction)**

主实体删除时不对依赖实体操作。如果外键不可为空，只对主实体进行删除可能会违反数据库外键约束。需要适当处理以避免错误。

**客户端无操作 (ClientNoAction)**

主实体删除时不对内存中依赖实体操作，需要自行处理依赖实体。
### 3. 多对多关系
#### 使用默认命名约定：
```csharp
public class Post
{
    public int Id { get; set; }
    public List<PostTag> PostTags { get; } = [];
}

public class Tag
{
    public int Id { get; set; }
    public List<PostTag> PostTags { get; } = [];
}
```
#### 使用Fluent API：
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Post>()
        .HasMany(e => e.Tags)
        .WithMany(e => e.Posts)
        .UsingEntity(   //Explicitly set
            "PostTag",  //set the join table name
            l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId"),  //set the join table ForeignKey name
            r => r.HasOne(typeof(Post)).WithMany().HasForeignKey("PostsId"),    //set the join table ForeignKey name
            j => j.HasKey("PostsId", "TagsId"));
}
```
#### 具有联接实体的类的多对多
可以创建一个显式联接实体，表示实体之间的多对多关系，并允许将其他属性添加到联接表中。
```csharp
//Model.cs
public class Post
{
    public int Id { get; set; }
    public List<Tag> Tags { get; } = [];
    public List<PostTag> PostTags { get; } = [];    //navigations to join entity
}

public class Tag
{
    public int Id { get; set; }
    public List<Post> Posts { get; } = [];
    public List<PostTag> PostTags { get; } = [];    //navigations to join entity
}

public class PostTag
{
    public int PostId { get; set; }
    public int TagId { get; set; }
    public Post Post { get; set; } = null!; //navigations to entity
    public Tag Tag { get; set; } = null!;   //navigations to entity
}
//AppContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Post>()
        .HasMany(e => e.Tags)
        .WithMany(e => e.Posts)
        .UsingEntity<PostTag>();
        /* Explicitly set
        .UsingEntity<PostTag>(
            l => l.HasOne<Tag>().WithMany(e => e.PostTags),
            r => r.HasOne<Post>().WithMany(e => e.PostTags));
        */
}
```