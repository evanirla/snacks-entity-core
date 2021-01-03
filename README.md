# Snacks.Entity.Core
Snacks.Entity.Core is a .NET Core framework that aims to simplify the relationship between the database and the public-facing REST API.

## Installation
Use the NuGet package manager to install the latest version of Snacks.Entity.Core

```bash
Install-Package Snacks.Entity.Core -Version 1.2.0
```

## Usage
Create a model
```csharp
using Snacks.Entity.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class StudentModel : BaseEntityModel<int>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
}
```
Create a model service
```csharp
using System;
using Snacks.Entity.Core;
using Snacks.Entity.Sqlite;

public class StudentService : BaseEntityService<StudentModel, SqliteService>
{
    public StudentService(
        IServiceProvider serviceProvider) : base(serviceProvider) { }
}
```
Create a model controller
```csharp
using System;
using Snacks.Entity.Core;

public class StudentController : BaseEntityController<StudentModel, int>
{
    public StudentController(
        IServiceProvider serviceProvider) : base(serviceProvider) { }
}
```
In your `Startup.cs` file, register the services.
```csharp
using Snacks.Entity.Core;
using Snacks.Entity.Core.Sqlite

public void ConfigureServices(IServiceCollection services)
{
    services.AddSqliteService(options => { });

    services.AddEntityServices();

}
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/) © Evan Irla
