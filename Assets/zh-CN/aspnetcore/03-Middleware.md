[ASP.NET Core 中间件](https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/middleware)
# ASP.NET Core 中间件

## ASP.NET Core 中间件概述

ASP.NET Core 中间件是请求处理管道的基本构建块。它们决定了HTTP请求和响应将如何被处理。中间件用于实现各种功能，如认证、授权、错误处理、路由和日志记录等。

中间件按照它们添加到管道的顺序执行，ASP.NET Core 应用程序中的每个中间件都执行以下任务：
- **传递请求**：选择是否将HTTP请求传递给请求处理管道中注册的下一个中间件。
- **执行任务**：可以在调用管道中的下一个组件之前或之后执行某些任务。

下图显示了 ASP.NET Core MVC 和 Razor Pages 应用的完整请求处理管道。图中描述了在典型的应用程序中，现有中间件的排序，以及自定义中间件的添加位置。

![middleware-pipeline](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/middleware-pipeline.svg)

ASP.NET Core提供了许多内置的中间件，可以根据业务需求创建自定义中间件。每个中间件应遵循单一职责原则。

## 中间件的工作原理

在ASP.NET Core Web应用程序中，中间件可以访问传入的HTTP请求和传出的HTTP响应。因此，它可以：
* 通过生成 HTTP 响应来处理传入的 HTTP 请求。
* 修改传入的 HTTP 请求，然后传递给请求处理管道中配置的下一个中间件。
* 修改传出的 HTTP 响应，然后传递给前面的中间件组件或 Web 服务器。

### 中间件执行顺序
对于传入请求，中间件按照它们被添加到请求处理管道的顺序执行，而对传出响应则是逆序执行。如果以错误的顺序添加中间件，则可能会出现意外行为。

### 请求处理管道
ASP.NET Core 请求管道包含一系列请求委托，依次调用。 下图演示了这一概念。 沿黑色箭头执行。

![request-delegate-pipeline](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/index/_static/request-delegate-pipeline.png)

- 请求处理管道由一系列中间件组件组成，这些组件将按特定顺序调用。
- 每个中间件都可以在调用下一个中间件之前和之后执行一些操作，从实现原理上来说，每个委托均可在下一个委托前后执行操作。中间件组件还可以决定不调用下一个中间件组件，这称为短路。

## 配置中间件

我们需要在`Program.cs`文件内的`Main()`方法中配置中间件。默认情况下，一个新的空ASP.NET Core Web应用程序会包含一个已经设置了两个中间件（`MapGet`和`Run`）的`Main`方法。

### 配置中间件的方式：
- **Map扩展方法**：根据特定的请求路径对中间件管道进行分支，主要用于最小API。
- **Run**：添加一个末尾中间件，末尾中间件管道，通常用于中间件管道的末尾。
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
`Run`方法用于添加一个末尾中间件：

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
可以使用Use extension 方法创建末尾中间件，不调用 `next()` 委托。在这种情况下，需要指定 **context** 对象并显式请求委托：
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//末尾中间件
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

## 中间件的使用

向 Program.cs 文件中添加中间件组件的顺序定义了针对请求调用这些组件的顺序，以及响应的相反顺序。 此顺序对于应用程序的安全性、性能和功能至关重要。

### 典型顺序

以下 Program.cs 代码将为常见应用场景添加中间件组件：

1. 异常/错误处理
   - 当应用在开发环境中运行时：
     - 开发人员异常页中间件 (UseDeveloperExceptionPage) 报告应用运行时错误。
     - 数据库错误页中间件 (UseDatabaseErrorPage) 报告数据库运行时错误。
   - 当应用在生产环境中运行时：
     - 异常处理程序中间件 (UseExceptionHandler) 捕获以下中间件中引发的异常。
     - HTTP 严格传输安全协议 (HSTS) 中间件 (UseHsts) 添加 Strict-Transport-Security 标头。
2. HTTPS 重定向中间件 (UseHttpsRedirection) 将 HTTP 请求重定向到 HTTPS。
3. 静态文件中间件 (UseStaticFiles) 返回静态文件，并简化进一步请求处理。
4. Cookie 策略中间件 (UseCookiePolicy) 使应用符合欧盟一般数据保护条例 (GDPR) 规定。
5. 用于路由请求的路由中间件 (UseRouting)。
6. 身份验证中间件 (UseAuthentication) 尝试对用户进行身份验证，然后才会允许用户访问安全资源。
7. 用于授权用户访问安全资源的授权中间件 (UseAuthorization)。
8. 会话中间件 (UseSession) 建立和维护会话状态。 如果应用使用会话状态，请在 Cookie 策略中间件之后和 MVC 中间件之前调用会话中间件。
9. 用于将 Razor Pages 终结点添加到请求管道的终结点路由中间件（带有 MapRazorPages 的 UseEndpoints）。

```csharp
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseDatabaseErrorPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
// app.UseRateLimiter();
// app.UseRequestLocalization();
// app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
// app.UseResponseCompression();
// app.UseResponseCaching();
app.MapRazorPages();
```
并非所有中间件都完全按照此顺序出现，但许多中间件都会遵循此顺序。 例如：
- `UseCors`、`UseAuthentication` 和 `UseAuthorization` 必须按显示的顺序出现。
- `UseCors` 当前必须在 `UseResponseCaching` 之前出现。
- `UseRequestLocalization` 必须在可能检查请求区域性的任何中间件（例如 `app.UseStaticFiles()`）之前出现。
- 在使用速率限制终结点特定的 API 时，必须在 `UseRouting` 之后调用 `UseRateLimiter`。 当仅调用全局限制器时，可以在 `UseRouting` 之前调用 `UseRateLimiter`。