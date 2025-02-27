
1. 操作数据库
```sql
CREATE DATABASE TestDB;
Alter DATABASE TestDB Modify Name = NewDB;
DROP DATABASE NewDB;
```

2. 操作数据表
```sql
CREATE TABLE students
( 
    No INT, 
    Name  VARCHAR(max)
);
-- 修改
ALTER TABLE students ALTER COLUMN No INT NULL;
ALTER TABLE students ALTER COLUMN Name NVARCHAR(100);
ALTER TABLE students ADD Branch VARCHAR(20) ;
ALTER TABLE students DROP COLUMN Branch;
SP_RENAME 'students.Name', 'studentsName'; -- 使用自带的存储过程

-- 删除表记录
TRUNCATE TABLE students;
-- 删除表
DROP TABLE students;
```

3. 约束
```sql
CREATE TABLE employees (
    ID int PRIMARY KEY,                     -- 主键约束（唯一、非Null）
    Name varchar(255) NOT NULL,             -- 非Null约束
    City varchar(255) DEFAULT 'Mumbai',     -- 默认值约束
    Branchcode INT,
    DateOfBirth date DEFAULT GETDATE()      -- 默认值约束

    CONSTRAINT Branchcode_fk FOREIGN KEY (Branchcode) REFERENCES BranchDetails(Bcode)   -- 外键约束
    CONSTRAINT eid_unique UNIQUE(ID)                    -- 唯一约束
    CONSTRAINT city_bc_unique UNIQUE(City, Branchcode)  -- 复合约束
    Entered_date DATETIME NOT NULL CHECK(Entered_date <= CURRENT_TIMESTAMP),            -- 检查约束
);

CREATE TABLE BranchDetails
( 
    City  VARCHAR(40), 
    Bcode INT, 
    PRIMARY KEY(City, Bcode)    -- 复合主键
);

  -- 修改约束
ALTER TABLE employees ALTER COLUMN ID INT NOT NULL;
ALTER TABLE employees ADD CONSTRAINT id_prime PRIMARY KEY (ID);
ALTER TABLE employees DROP CONSTRAINT id_prime;
```

4.操作数据
```sql
INSERT INTO employees (ID, Name, Branchcode, Entered_date) VALUES (2, 'name1', 102, GETDATE());
```