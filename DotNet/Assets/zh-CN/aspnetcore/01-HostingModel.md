# ASP.NET Core 托管模型

在 ASP.NET Core 中，托管模型确定如何托管 ASP.NET Core 应用程序、使用哪些 Web 服务器以及如何处理请求。ASP.NET Core 中有两种主要的托管模型：

- InProcess

  - 应用直接在 IIS 工作进程中运行。
  - Windows / IIS / IIS Express 环境默认托管模型。
  - 性能更佳，避免与外部进程通信开销。

- OutOfProcess
  - 应用在独立进程中运行（通常使用 Kestrel Web Server）。
  - 服务器 IIS (或 Apache, Nginx) 作为反向代理，将 HTTP 请求转发给内部 Web 服务器（如 Kestrel）。
  - 通过反向代理服务器处理请求转发。

## InProcess 托管模型工作流程

请求/响应 ↔ **(IIS** ↔ **ANCM** ↔ **IIS HTTP Server** ↔ **应用代码)** 

1. **用户请求**  
   - 用户通过浏览器或其他客户端发送 HTTP 请求到 IIS 。

2. **IIS (Internet Information Services)**  
   - 接收请求，检查应用状态。  
   - 若未启动，转发至 ASP.NET Core Module；否则直接转至 IIS HTTP Server。

3. **ANCM(ASP.NET Core Module)**  
   - 启动 ASP.NET Core 应用（如未运行）。  
   - 加载.NET Core 运行时并在 IIS 工作进程中初始化应用。  
   - 转发请求至 IIS HTTP Server（仅在初始或应用池启动时）。

4. **IIS HTTP Server**  
   - 处理 HTTP 协议相关细节。  
   - 将处理后的请求发送给应用程序代码。

5. **应用代码**  
   - 业务逻辑和页面生成。  
   - 请求经过中间件管道处理（认证、路由等）。  
   - 控制器处理请求，准备响应并返回给 IIS。

6. **IIS HTTP Server**  
   - 响应经 ASP.NET Core Module 直接转到 IIS。

7. **IIS**  
   - 发送响应回客户端。


## OutOfProcess 托管模型工作流程

请求/响应 ↔ **反向代理** ↔ **ANCM** ↔ **Kestrel** ↔ **应用代码**

## 流程说明

1. **用户请求**  
   用户通过浏览器等客户端发送HTTP请求到服务器。

2. **IIS**  
   作为反向代理接收请求，通过ANCM转发给Kestrel。

3. **ANCM**  
   - 检查并启动Kestrel（如果需要）  
   - 转发请求给Kestrel

4. **Kestrel**  
   - 处理来自ANCM的请求  
   - 将请求传递给应用代码

5. **应用代码**  
   - 执行业务逻辑、身份验证、路由等  
   - 生成响应内容

6. **Kestrel**  
   - 接收应用代码的响应  
   - 发送回ANCM

7. **ANCM**  
   直接转发响应给IIS

8. **IIS**  
   准备并将响应发送回客户端

9. **客户端浏览器**  
   显示从IIS接收到的HTTP响应

## Kestrel Web Server

Kestrel Web Server 是在 Windows、Linux 和 MacOS 上托管 ASP.NET Core Web 应用程序的默认跨平台 Web 服务器，使用 HTTP/HTTPS 配置启动程序将使用 Kestrel 服务器，程序代码在 Kestrel 服务器工作进程中执行。  
当 Kestrel 独立使用时，它直接处理请求，无需反向代理服务器，此时 InProcess/OutOfProcess 术语不适用。
