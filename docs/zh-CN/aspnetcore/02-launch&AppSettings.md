## ASP.NET Core 中的 `LaunchSettings.json`

ASP.NET Core 中的 `LaunchSettings.json` 位于 ASP.NET Core 项目的 Properties 文件夹中，用于配置应用程序在开发过程中的启动方式。

仅当使用 Visual Studio、VS Code 和 .NET CLI 运行应用程序时，`LaunchSettings.json` 文件在本地开发计算机中使用。将 ASP.NET Core 应用程序发布到生产服务器时，不需要此文件。

如果在将应用程序发布和部署到生产服务器时使用某些设置，则需要将此类设置存储在应用程序级别的配置设置，即 `appsettings.json` 文件中。

以下是 `LaunchSettings.json`文件的配置示例：

```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "dotnetRunMessages": true,
      "applicationUrl": "http://localhost:5093"
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "WSL": {
      "commandName": "WSL2",
      "launchBrowser": true,
      "launchUrl": "http://localhost:5093",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_URLS": "http://localhost:5093"
      },
      "distributionName": ""
    }
  },
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:45531",
      "sslPort": 0
    }
  }
}
```

## ASP.NET Core 中的 `appsettings.json`

### 默认配置值

在 ASP.NET Core 应用程序中，`appsettings.json`文件作为默认配置源提供了应用程序启动时所需的初始设置，不限于以下内容：

- **日志记录级别**：定义应用程序的日志详细程度，默认情况下限制为“警告”。
- **连接字符串**：用于数据库或其他外部服务的默认连接信息。
- **应用行为设置**：分页大小、缓存过期时间等参数。

### 配置优先级顺序

ASP.NET Core 应用程序从多个来源读取配置数据，并根据一定的优先级顺序来决定最终使用的配置值。如果多个配置源包含相同的键，则较晚加载的配置源会覆盖先前的值。
配置优先级顺序（从低到高）：

1. **`appsettings.json`**
2. **`appsettings.{Environment}.json`**
3. **用户秘密 (User Secrets)**
4. **环境变量**
5. **命令行参数**

以下是 `appsettings.json`文件的部分配置示例：

#### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=address;Database=dataBase;User=username;Password=password;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ExternalService": {
    "BaseUrl": "https://api.dev.example.com/",
    "ApiKey": "Dev-Api-Key"
  },
  "ApplicationSettings": {
    "PageSize": 10,
    "SupportEmail": "support.dev@example.com"
  },
  "Authentication": {
    "Jwt": {
      "Key": "Secret-Key",
      "Issuer": "Issuer",
      "Audience": "Audience"
    }
  }
}
```
