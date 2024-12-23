## 在 Entity Framework Core 中使用 Fluent API 进行配置

下面列出了在Entity Framework Core 中常用的 Fluent API 配置。

| 属性配置范围 | 使用示例<br>`var entity = modelBuilder.Entity<User>();`<br>`var property = entity.Property(u=> u.Name);` | 说明 |
| --- | --- | --- |
| 数据库架构 | `modelBuilder.HasDefaultSchema("sales")` | 指定用于所有表、存储过程等的数据库架构，除非有特定实体显式配置 |
| `Entity` | `entity.ToTable("tblUser", schema: "Admin")` | 配置表名、架构 |
| `Entity` | `entity.HasKey(u=> u.UserId)` | 配置主键 |
| `Entity` | `entity.HasNoKey()` | 配置无主键表；<br>`Entity`不应使用其他约定配置主键 |
| `Entity` | `entity.HasKey(fp => new { fp.FormId, fp.PostId })` | 配置复合主键 |
| `Entity` | `modelBuilder.Ignore<Log>()` | 排除实体，不映射到数据库表 |
| `Entity` | `entity.OwnsOne(u=> u.Address)` | 从属实体：由另一个实体拥有，其属性映射到所有者表中的列，没有自己的表 |
| `Property` | `property.HasColumnName("First_Name")` | 配置属性映射的列名 |
| `Property` | `property.HasColumnType("decimal(10, 2)")` | 配置列数据类型 |
| `Property` | `property.HasDefaultValueSql("GETUTCDATE()")` | 配置默认值 |
| `Property` | `property.IsRequired()`<br>`property.IsRequired(false)` | 配置必需（可选）的属性 | 
| `Relationship` | `modelBuilder.Entity<Order>().HasOne(typeof(Customer)).OnDelete(DeleteBehavior.Cascade);` | 设置关系的删除行为 |
| `Property` | `property.IsUnicode(false)` | 将字符串属性默认映射到数据库的 `nvarchar` 配置为非Unicode的 `varchar`，可节省存储空间 |
| `Property` | `property.HasMaxLength(200)` | 配置最大长度，优化存储 |
| `Property` | `entity.HasIndex(u=> u.Email).IsUnique()`<br>复合索引：<br> `entity.HasIndex(u=> new { u.LastName, u.FirstName })`<br>筛选索引：<br>`entity.HasIndex(u=> u.Email)`<br>`.HasFilter("[Email] IS NOT NULL")` | 配置索引、设置索引类型 |
| `Property` | `modelBuilder.Entity(entityType.ClrType)`<br>`.Property(e => e.UpdatedAt)`<br>`.HasDefaultValueSql("GETUTCDATE()")`<br>`.ValueGeneratedOnAddOrUpdate()` | 利用 SQL Server 自动管理所有时间戳属性 |
| `Property` | `entity.Property(o => o.RowVersion).IsRowVersion()` | 配置并发令牌 |
| `Property` | `entity.Property(o => o.LastModified).IsConcurrencyToken()` | 配置替代并发令牌 |