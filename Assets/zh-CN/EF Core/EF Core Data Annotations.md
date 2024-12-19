
## Entity Framework Core 数据注释属性概览

下面列出了常用的Entity Framework Core (EF Core) 数据注释属性，并以表格形式展示了它们的简要信息。

| 属性名称 | 使用示例 | 应用于实体/属性 | 影响数据库架构 | 说明 |
| --- | --- | --- | --- | --- |
| Key | 属性：`[Key]`<br>实体：`[PrimaryKey(nameof(Name))，nameof(Age))]` | 属性/实体 | 是 | 标记属性作为实体的主键 |
| Table | `[Table("UserInfo", Schema = "dbo")]` | 实体 | 是 | 指定实体映射到的数据库表名、组织架构 |
| ForeignKey | `[ForeignKey("Author")]`<br>`[ForeignKey("UserId,BookId")]` | 属性 | 是 | 指定属性作为外键、复合外键<br>可用于标量属性(scaler property)和导航属性(reference navigation property) |
| InverseProperty | `[InverseProperty("Posts")]` | 属性 | 否 | 指定导航属性的反向属性，帮助建立双向关系 |
| Column | `[Column("dbo", Order = 2, TypeName = "DateTime2")]` | 属性 | 是 | 指定数据库中列的名称、顺序和类型 |
| Index | `[Index("Id","Name", AllDescending = true, IsUnique = true)]` | 属性 | 是 | 创建单/多列索引，设置排序，提高性能 |
| Required | `[Required]` | 属性 | 是 | 指定属性为必填项，数据库创建 `NOT NULL` 列 |
| NotMapped | `[NotMapped]` | 属性/实体 | 否 | 标记属性或实体不映射到数据库中的列或表<br>没有`setter`或`getter`的属性自动不映射 |
| MaxLength | `[MaxLength(100)]` | 属性 | 是 | 限制字符串或数组的最大长度，并设置数据库中相应列的最大大小 |
| MinLength | `[MinLength(10)]` | 属性 | 否 | 指定属性中数组或字符串的最小长度，模型验证 |
| DatabaseGenerated | `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` | 属性 | 是 | 指定数据库如何生成值<br>`None`：从不<br>`Identity`：Insert时生成<br>`Computed`：Insert和Update时生成 |
| Timestamp | `[Timestamp]` | 属性 | 是 | 标记属性为并发令牌，通常用于乐观并发控制 |
| ConcurrencyCheck | `[ConcurrencyCheck]` | 属性 | 是 | 标记属性参与乐观并发检查 |