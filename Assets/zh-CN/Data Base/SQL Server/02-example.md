
1. 操作数据库
```sql
CREATE DATABASE TestDB;
Alter DATABASE TestDB Modify name = NewDB;
DROP DATABASE NewDB;
```

2. 操作数据表
```sql
CREATE TABLE students
( 
    no INT PRIMARY KEY idENTITY, 
    name  VARCHAR(max)
);
-- 修改
ALTER TABLE students ALTER COLUMN no INT NULL;
ALTER TABLE students ALTER COLUMN name NVARCHAR(100);
ALTER TABLE students ADD branch VARCHAR(20) ;
ALTER TABLE students DROP COLUMN branch;
SP_RENAME 'students.name', 'studentsname'; -- 使用自带的存储过程

-- 删除表记录
TRUNCATE TABLE students;
-- 删除表
DROP TABLE students;
```

3. 约束
```sql
CREATE TABLE employees (
    id INT PRIMARY KEY,                     -- 主键约束（唯一、非Null）
    name varchar(255) NOT NULL,             -- 非Null约束
    city varchar(255) DEFAULT 'Mumbai',     -- 默认值约束
    branchcode INT,
    dateofbirth date DEFAULT GETDATE()      -- 默认值约束

    CONSTRAINT branchcode_fk FOREIGN KEY (branchcode) REFERENCES branchDetails(branchcode)   -- 外键约束
    CONSTRAINT eid_unique UNIQUE(id)                    -- 唯一约束
    CONSTRAINT city_bc_unique UNIQUE(city, branchcode)  -- 复合约束
    Entered_date DATETIME NOT NULL CHECK(Entered_date <= CURRENT_TIMESTAMP),            -- 检查约束
);

CREATE TABLE branchDetails
( 
    city  VARCHAR(40), 
    branchcode INT, 
    PRIMARY KEY(city, branchcode)    -- 复合主键
);

  -- 修改约束
ALTER TABLE employees ALTER COLUMN id INT NOT NULL;
ALTER TABLE employees ADD CONSTRAINT id_prime PRIMARY KEY (id);
ALTER TABLE employees DROP CONSTRAINT id_prime;
```

4.操作数据
```sql
INSERT INTO employees (id, name, branchcode, Entered_date) VALUES (2, 'name1', 102, GETDATE());
```

5.读取数据
```sql
SELECT [ ALL | DISTINCT ]
[TOP (top_value) [ PERCENT ] [ WITH TIES ] ]
expressions
FROM tables
[WHERE conditions]
[GROUP BY expressions]
[HAVING condition]
[ORDER BY expression [ ASC | DESC ]];
/*
ALL ：返回所有匹配的行。

DISTINCT ：从结果集中删除重复的行。

TOP ：返回结果集中的前 top_value 行。

PERCENT：返回总结果集的百分比。

WITH TIES：返回在有限结果集中的最后一个位置绑定的行。

expressions ：在结果集中检索的列或计算。使用 * 选择所有列。

tables ：要从中检索记录的表。

WHERE ：结果集满足的条件。

GROUP BY ：按一列或多列对记录进行分组。

ORDER BY ：按升序或降序排序。
*/
```