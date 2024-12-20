## 在 Entity Framework Core 中使用 Fluent API 进行配置

下面列出了在Entity Framework Core 中常用的 Fluent API 配置。

| 属性配置范围 | 使用示例  | 说明 |
| --- | --- | --- |
| 数据库架构 | `modelBuilder.HasDefaultSchema("sales")` | 指定用于所有表、存储过程等的数据库架构，除非有特定实体显式配置 |
| `Property` | `fKey.DeleteBehavior = DeleteBehavior.Restrict` | 设置关系的删除行为 |
| `Property` | `property.SetIsUnicode(false)` | 将字符串属性默认映射到数据库的 `nvarchar` 配置为非Unicode的 `varchar`，可节省存储空间 |
| `Property` | `property.SetMaxLength(200)` | 属性设置默认最大长度，提高数据库性能 |
| `Property` | `modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique()`<br>复合索引：<br> `.HasIndex(c => new { c.LastName, c.FirstName })`<br>筛选索引：<br>`modelBuilder.Entity<Customer>()`<br>`.HasIndex(c => c.Email)`<br>`.HasFilter("[Email] IS NOT NULL")` | 配置索引、设置索引类型 |
| `Property` | `modelBuilder.Entity(entityType.ClrType)`<br>`.Property(e => e.UpdatedAt)`<br>`.HasDefaultValueSql("GETUTCDATE()")`<br>`.ValueGeneratedOnAddOrUpdate()` | 利用 SQL Server 自动管理时间戳属性 |
| `Entity` | `modelBuilder.Entity<User>().ToTable("tblUser", schema: "Admin")` | 配置表名、架构 |
| `Entity` | `modelBuilder.Entity<Customer>().HasKey(c => c.CustomerId)` | 配置主键 |
| `Entity` | `modelBuilder.Entity<FormPost>().HasKey(fp => new { fp.FormId, fp.PostId })` | 配置复合主键 |
| `Entity` | `modelBuilder.Ignore<Log>()` | 排除实体，不映射到数据库表 |
| `Entity` | `modelBuilder.Entity<Customer>().OwnsOne(c => c.Address);` | 从属实体：由另一个实体拥有，其属性映射到所有者表中的列，没有自己的表 |