using System.Linq;

var data = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

// 获取集合中的偶数 排序后取前3个
var res = (from x in data
           where x % 2 == 0
           orderby x
           select x).Take(3);

var res2 = data.Where(x => x % 2 == 0).OrderBy(x => x).Take(3);

Console.WriteLine(string.Join(", ", res));
// Output: 2, 4, 6

// 取交集
var data1 = new List<int> { 1, 2, 3, 4, 10, 10 };
res = data.Intersect(data1);
res2 = from x in data
       where data1.Contains(x)
       select x;
// 1, 2, 3, 4, 10

// 分组求和 排序
var dic = data1.GroupBy(x => x).Select(g => new { g.Key, Count = g.Count() });  //排序 .OrderByDescending(kv => kv.Count);
var dic1 = from x in data1
           group x by x into g
           select new { g.Key, Count = g.Count() } into kv
           orderby kv.Count descending
           select kv;

Console.WriteLine(string.Join(", ", dic1.Select(kv => $"{{ {kv.Key}: {kv.Count} }}")));
// { 10: 2 }, { 1: 1 }, { 2: 1 }, { 3: 1 }, { 4: 1 }

// 常用方法
var FirstEven = data.First(x => x % 2 == 0);    // data.Where(x => x % 2 == 0).First();
var CountEven = data.Count(x => x % 2 == 0);    // data.Where(x => x % 2 == 0).Count();


//延迟执行 直至调用ToList()、Take()、Sum()等访问数据
res = data1.Select(data => data * 2);
Console.WriteLine(string.Join(", ", res));
data1.Add(11);
Console.WriteLine(string.Join(", ", res));
// 1st Output: 2, 4, 6, 8, 20, 20
// 2nd Output: 2, 4, 6, 8, 20, 20, 22

// 多线程
res = data.ToArray()
    .AsParallel()
    .AsOrdered()    // 保持原有顺序
    .Select(x =>
    {
        Thread.Sleep(500);
        return x * x;
    })
    .AsSequential();    // 转换单线程
Console.WriteLine(string.Join(", ", res));

// 锯齿状数组展开
int[][] jaggedArray = new int[][]
{
    new[] { 1, 2, 3 },
    new[] { 4, 5 },
    new[] { 6, 7, 8, 9 }
};

res = jaggedArray.SelectMany(subArray => subArray);
res2 = from subArray in jaggedArray
       from item in subArray
       select item;
// 1, 2, 3, 4, 5, 6, 7, 8, 9

//笛卡尔积
res = from x in new[] { 1, 2, 3 }
      from y in new[] { 4, 5, 6 }
      select x * y;
// 4, 5, 6, 8, 10, 12, 12, 15, 18

// 批量异步
async Task UploadAsync(int x)
{
    await Task.Delay(1000);
    Console.WriteLine($"{x} done");
}
data = new List<Task>();
var tasks = from x in data
            select UploadAsync(x);
// tasks = data.Select(x => UploadAsync(x));
await Task.WhenAll(tasks);
Console.WriteLine("All done");