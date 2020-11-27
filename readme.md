# Dotnet Core with cleanArchitecture

Using EntityFramework **code first** approach. I found out that using **package versions** that don't match with **thes installed runtime**. Migration generation will fail. The package version to install should be the same as the installed runtimes fro your project.

To check on the installed runtimes you could use the command.

```bash
dotnet --info
```

## Running the project

### Migration. You will need to install dotnet-ef

```bash
➜ dotnet tool install --global dotnet-ef
```

To confirm that **dotnet ef tool**  is installed run the command **dotnet ef** alone. The command should output something to confirm its presence.

### Set up path to dotnet tools

This is done by adding this to your **shell** start up shell scripts.

### Running the Project

We need to tell our project where our startup project is. **-p** stated that it is a **project**.

```bash
dotnet run -p Api
```

### Generating  Migrations

Since this is a **clean architecture** project. In order to generate migrations in the code you will need to run the command. Every time you make a change to your Persistence project you will be required to creat a new migations.

```bash
➜ dotnet ef migrations add InitialCreate -p Persistence/ -s Api
```

In order to generate migrations you will need to add a package **Microsoft.EntityFrameworkCore.Design** to your project. Just refer this package from nuget.

### Creating A database From Migrations

The **dotnet ef** code provides a way to **create or update** a database from the command line.  I don't preffer this approach.

The approach i will use is to **check if the migrations** exists from the **program.cs** file and then **create a database with the migrations**. Later i will add **seeding data** for the database.

### Added SeedData

**DataContext** class is used by entity framework to create database from our models. EntityFramework provides a way to manipulate object that are created from the **datacontext** class. By ovverriding the **OnModelCreating** method. You get access to a **model builder** which can be use to seed data, in this case.

```c#
protected override void OnModelCreating(ModelBuilder builder){
  builder.Entity<Values>().HasData(
    new Values { Id = 1, Name = "value 101" },
    new Values { Id = 2, Name = "value 102" },
    new Values { Id = 3, Name = "value 103" }
);
}
```

Once you override the method. Rember to **generate migration for the seeddata**.

```bash
dotnet ef migrations add SeedData -p Persistence/ -s Api/
```

Restarting our application will ensure the **migrations are** added to our database.

### When doing Database queries

It is usually advisable to run **asyncronous** queries for our database requests, since the queries could be **long running** and you don't want the code to be blocking. Making it asyncronous, you now are able to run the requests on multiple threads. The database request is passed to a different **thread** and then we await the **task** to be completed.

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<Values>>> Get()
{
    var values = await _context.Values.ToListAsync();
    return Ok(values);
}
```

#### Testing Api with Postman

When it comes to testing your api. The browser in usually not the best tool to test your **rest api**. You could just took to using **postman utility** to test your api.

### Why use sqlite

Sqlite allow portablity when developing. Since we don't need to install additonal software when developing our application. When using **entityframework** the database technology you choose to use is **irrelevant**. Entityframework abstracts that layer away and you dont even need to think about it. Databases can be swapped when need without much work.

Of course when publishing our application we will not publish with **sqlite** we will use a different database **say mysql, sql server**.

### Why separate out our Projects

We get a genuine **separation of concern** when building our project in this way. Although it might appear as overcomplication our code. What we get at the end it a projects with layers that are responsible of a few things.

### What we will Do

- **Create, Read, Update and Delete**
- **Thin Api Controllers** Instead of having the logic in our **api** we will move out the code away from that.
- **Seeding more data**
- **Adding more Migrations**
- **CQRS and Mediator Pattern**

### Seeding Related Data.

Seeding data with the **modelbuilder** is a simple way for simple data. But when you have relational data. seeding it with the method becomes a little bit tricky. This calls for a new approch to seed our data in the code.

The new way am usiing is simple. Since i create a **SeedData** class and a static method that takens in the **dbcontext** class. Using dbcontext we can add a range of activities. But first i check if there are any Activity in existance. IF they are , nothing is added. 

The static method is called at startup at the same place i called the **database migrations**.

```csharp
context.Database.Migrate();
SeedData.SeedActivities(context);
```

### CRUD. Using CQRS and MEDIATOR patterns

> *Command-query Separation*

**Commands** Does something, Modifies State, should not return a value. Examples includes **Create, Update, Delete**

**Query** Answers a question. Does not modify state. should return a value. **Get**

#### CQRS

with CQRs you could have **multiple databases** lets say two datbase. One optimized for **reading data** and the **other optimized for writing data**.

- commands use write DB.
- Queries use read Db.
- CQRS ensures Eventual consistency.
- can be faster.

CQRS is responsible on database flow. CQRS - Single Database

#### Another use case of CQRS

CQRS using an **event store**. Every Event that happens is tracked and Stored in the event store database.

#### Pros

1. Scalability.
2. Flexibility.
3. Event Sourcing.

#### Cons

- More complex than other patterns.
- Does not modifly state.
- Event sourcing costs.

https://youtu.be/JHGkaShoyNs

#### uncle Bob clean architecture

- The dependecy points inwards.  
- **command Handler - Create Activity**
MediatR -> **Object in > Handler > Object out
  - **Object In** the activity object.
  - **Object out** 
    - create new Activity.
    - Save new Activity.
    - Return "Unit"
- **Query Handler** -Get Activity

- **object in**

   ```json
   {
     id: 3
   }
   ```

- **Object Out**
  - Get activity from **Database** with Id of 3
  - If activity does not exist retunr not found.
  - If the activity is found, project into **ActivityDTO**
  - Return ActivityDTO

#### Adding the Mediator Package

Mediator

### Api Validations

There are a few way of doing data validation in dotnet core. This is essential since we dont want to receive bad data from the server.

1. Api Validation using Data attributes.
2. Api Validation Using Fluent validation.

#### Data Annotations

Data attributes put on top of the propertties. This is done using **microsoft data annotations** package.

#### Fluent Validator

This is done using Fluent validation package. We will be able to set rules on our commands.

command > validate command > handler logic

T   vbhere is no need to process a request if it does not meet the validation logic.

## Exception and error handling.

To handle errors we will use, **custom middleware** to achive this. If you try to send an empy object. Our servers will most likely accept any data but with all fields null. There are very many ways of doing  data validations.

The simplest is to use **data annnotation**.
  
```bash
[Required]
public string Title { get; set; }
```

Now that we added the **data annotation** we want to try to Send an empty object from postman. we know will have validation for the **title** properties.

```json
traceId": "|afae5dba-47d7327a74651f22.",
    "errors": {
        "Title": [
            "The Title field is required."
        ]
    }
```

Now place a break point  on the api we create in order to debugg. Since on our api we have **[apicontroller]** annotation c# knows to use inbuild data validations for the request. We do not even hit the break point at this stage.

Lets comment it out  the data annotation **[apicontroller]** and see whats happens. The breakpoint get hit. Initially in order to do validation we used to check if the **modelstate** is valid before processing the request.

The request before the **[apicontroller] could be. 👇

```csharp
[HttpPost]
public async Task<ActionResult<Unit>> Create(Create.Command command) {
    if(!ModelState.IsValid) return BadRequest(ModelState);
    return await _mediator.Send(command);
}
```

☝ There is no good reason to  not write the **[apicontroller]**. The other thing the **[apicontroller]** does is **binding source parameter inference**. When doing clean achitecture, We will not do validation in this way. We will look at **fluent validation**.

The fluent validation is addded **between** the **command** and the **handler**.

### Add package for fluent Validation

```bash
dotnet add package FluentValidation.AspNetCore --version 8.6.3
```

☝ add the package to the **application** project. The latest version was not **compatible** with .netframwork **2.1**. I changed the project up to target **netcore3.0**. To configure fluent validation we use create a class between the **command** and the **handler** for the sake of create.

```csharp

public class CommandValidator : AbstractValidator<Command>
{
    public CommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
    }
}
```

In order to use **fluentvalidation** we must subscribe to it in the startup classes as.

```csharp
  services.AddControllers().AddFluentValidation(
      cfg => {
          cfg.RegisterValidatorsFromAssemblyContaining<Create>();
      }
  );
```

Ensure you remove the **dataannotation** we used but keep the **[apicontroller]** Restart the application. Trying to create an empty object. We get the same validation errors.


