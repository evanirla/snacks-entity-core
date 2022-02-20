# Snacks.Entity.Core
Snacks.Entity.Core is a .NET Core framework that aims to simplify the relationship between the database and the public-facing REST API.

## Installation
Use the NuGet package manager to install the latest version of Snacks.Entity.Core

```bash
Install-Package Snacks.Entity.Core
```

## Usage
### Create a DbContext
```csharp
public class GlobalDbContext : DbContext
{
    public DbSet<Student> Students { get; set; }

    public GlobalDbContext(DbContextOptions<GlobalDbContext> options)
        : base(options)
    {
        
    }
}
```

### Register provider
In your `Program.cs` file, add DbContext factory and entity provider.
```csharp
builder.Services.AddDbContextFactory<GlobalDbContext>(options => 
{
    options.UseInMemoryDatabase("Global");
});
builder.Services.AddEntityProvider<GlobalDbContext>();
builder.Services.AddControllers();
```

### Test
Your application should now allow you to query data RESTfully like `api/students?grade[gte]=5&orderby[desc]=age&offset=5&limit=20`

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](https://choosealicense.com/licenses/mit/) © Irla Software Solutions
