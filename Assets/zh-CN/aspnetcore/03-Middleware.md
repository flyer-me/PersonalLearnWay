[ASP.NET Core 中间件](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware)
# ASP.NET Core 中间件

## ASP.NET Core 中间件

ASP.NET Core 中间件是请求处理管道的基本构建块。它们决定了HTTP请求和响应将如何被处理。中间件用于实现各种功能，如认证、授权、错误处理、路由和日志记录等。

中间件按照它们添加到管道的顺序执行，ASP.NET Core 应用程序中的每个中间件都执行以下任务：
- **传递请求**：选择是否将HTTP请求传递给请求处理管道中注册的下一个中间件。
- **执行任务**：可以在调用管道中的下一个组件之前或之后执行某些任务。

ASP.NET Core提供了许多内置的中间件，可以根据业务需求创建自定义中间件。每个中间件应遵循单一职责原则。

## 中间件的工作原理

在ASP.NET Core Web应用程序中，中间件可以访问传入的HTTP请求和传出的HTTP响应。因此，它可以：
* 通过生成 HTTP 响应来处理传入的 HTTP 请求。
* 修改传入的 HTTP 请求，然后传递给请求处理管道中配置的下一个中间件。
* 修改传出的 HTTP 响应，然后传递给前面的中间件组件或 Web 服务器。

### 中间件执行顺序
对于传入请求，中间件按照它们被添加到请求处理管道的顺序执行，而对传出响应则是逆序执行。如果以错误的顺序添加中间件，则可能会出现意外行为。

## 配置中间件

我们需要在`Program.cs`文件内的`Main()`方法中配置中间件。默认情况下，一个新的空ASP.NET Core Web应用程序会包含一个已经设置了两个中间件（`MapGet`和`Run`）的`Main`方法。

### 配置中间件的方式：
- **Map扩展方法**：根据特定的请求路径对中间件管道进行分支，主要用于最小API。
- **Run**：添加一个终止中间件，终止中间件管道，通常用于中间件管道的末尾。
- **Use**：添加一个新的中间件，选择性地调用下一个中间件，使用最灵活。

### 使用 Map/MapGet 扩展方法配置中间件组件
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
```
MapGet 和 Map 扩展方法的区别
- **MapGet**：专门设计用于处理HTTP GET请求，适用于仅需响应GET请求的端点。
- **Map**：处理所有类型的HTTP请求的通用方法，请求委托通常包含区分这些方法的逻辑。

### 使用Run扩展方法配置中间件
`Run`方法用于添加一个终止中间件：

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//Configuring New Inline Middleware Component using Run Extension Method
app.Run(async (context) =>
{
    await context.Response.WriteAsync("Response from First Middleware");
});

// Starts the web application which begins listening for incoming requests
app.Run();
```

### 使用 Use Extension 方法配置中间件组件
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//Use Extension Method
app.Use(async (context, next) =>
{
    await context.Response.WriteAsync("Getting Response from First Middleware");
    await next();
});

//Run Extension Method
app.Run(async (context) =>
{
    await context.Response.WriteAsync("\nGetting Response from Second Middleware");
});

app.Run();
```
在第一个 Use Extension 方法中，**context** 和 **next** 参数传递给匿名方法。然后，调用 **next** delegate，它将调用下一个中间件。
可以使用Use extension 方法创建终止中间件，不调用 `next()` 委托。在这种情况下，需要指定 **context** 对象并显式请求委托：
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//终止中间件
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    await context.Response.WriteAsync("Getting Response from First Middleware");
    // 不调用 'next'，终止处理管道
});

// 响应不会追加本方法的文本
app.Run(async (context) =>
{
    await context.Response.WriteAsync("\nGetting Response from Second Middleware");
});

app.Run();
```

## 中间件的使用位置

### 横切关注点处理

中间件组件在 ASP.NET Core 应用程序中用于处理横切关注点，例如身份验证、授权、日志记录、错误处理、路由和其他应用程序级服务。中间件组件在 HTTP 请求处理管道中配置，它们决定如何处理请求和返回响应。以下是ASP.NET Core应用程序中使用的常见内置中间件组件：

**UseRouting**

配置路由中间件，负责将请求映射到相应的端点处理程序。

**UseAuthentication**

- 验证请求中的凭据，设置用户身份。
- 通常在`UseRouting`之后、`UseAuthorization`之前添加。

**UseAuthorization**

- 检查用户是否具有访问资源的权限。
- 确保安全策略的实施。

**UseHttpsRedirection**

- 自动将HTTP请求重定向到HTTPS，确保安全。

**UseDeveloperExceptionPage**

- 在开发过程中提供详细的错误页面，帮助诊断问题。

**UseExceptionHandler**

- 捕获并处理异常，管理生产环境中的错误。

**UseStaticFiles**

- 直接从服务器提供静态文件，提高性能。