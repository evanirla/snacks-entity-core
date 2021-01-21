# Snacks.Entity.Core
Snacks.Entity.Core is a .NET Core framework that aims to simplify the relationship between the database and the public-facing REST API.

## Installation
Use the NuGet package manager to install the latest version of Snacks.Entity.Core

```bash
Install-Package Snacks.Entity.Core
```

## Usage
Create an entity service for an EF Core model
```csharp
using System;
using Snacks.Entity.Core;

public class StudentService : BaseEntityService<StudentModel, MyDbContext>
{
    public StudentService(
        IServiceScopeFactory scopeFactory) : base(scopeFactory) { }
}
```
Create an entity controller
```csharp
using System;
using Snacks.Entity.Core;

public class StudentController : EntityControllerBase<StudentModel, int, StudentService>
{
    public StudentController(
        IServiceProvider serviceProvider) : base(serviceProvider) { }
}
```
In your `Startup.cs` file, register the services
```csharp
using Snacks.Entity.Core;

public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<MyDbContext>();
    services.AddEntityServices();
}
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/) © Evan Irla
