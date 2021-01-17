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

To confirm that **dotnet ef tool** is installed run the command **dotnet ef** alone. The command should output something to confirm its presence.

### Set up path to dotnet tools

This is done by adding this to your **shell** start up shell scripts.

### Running the Project

We need to tell our project where our startup project is. **-p** stated that it is a **project**.

```bash
dotnet run -p Api
```

### Generating Migrations

Since this is a **clean architecture** project. In order to generate migrations in the code you will need to run the command. Every time you make a change to your Persistence project you will be required to creat a new migations.

```bash
➜ dotnet ef migrations add InitialCreate -p Persistence/ -s Api
```

In order to generate migrations you will need to add a package **Microsoft.EntityFrameworkCore.Design** to your project. Just refer this package from nuget.

### Creating A database From Migrations

The **dotnet ef** code provides a way to **create or update** a database from the command line. I don't preffer this approach.

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

> _Command-query Separation_

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
  MediatR -> \*\*Object in > Handler > Object out
  - **Object In** the activity object.
  - **Object out**
    - create new Activity.
    - Save new Activity.
    - Return "Unit"
- **Query Handler** -Get Activity

- **object in**

  ```json
  {
    "id": 3
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

T vbhere is no need to process a request if it does not meet the validation logic.

## Exception and error handling.

To handle errors we will use, **custom middleware** to achive this. If you try to send an empy object. Our servers will most likely accept any data but with all fields null. There are very many ways of doing data validations.

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

Now place a break point on the api we create in order to debugg. Since on our api we have **[apicontroller]** annotation c# knows to use inbuild data validations for the request. We do not even hit the break point at this stage.

Lets comment it out the data annotation **[apicontroller]** and see whats happens. The breakpoint get hit. Initially in order to do validation we used to check if the **modelstate** is valid before processing the request.

The request before the \*\*[apicontroller] could be. 👇

```csharp
[HttpPost]
public async Task<ActionResult<Unit>> Create(Create.Command command) {
    if(!ModelState.IsValid) return BadRequest(ModelState);
    return await _mediator.Send(command);
}
```

☝ There is no good reason to not write the **[apicontroller]**. The other thing the **[apicontroller]** does is **binding source parameter inference**. When doing clean achitecture, We will not do validation in this way. We will look at **fluent validation**.

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

### Error Handling

Right Now our program is either sending **500** or a **200**. We need a way to send the right **error responses** for our api. There are a few way to return status code.

1. Using **Application Handlers**. Our application project do not have access to the **application handlers**.
2. Throw an **exception** that returns a https status code. This will done by creating our own middware. Our middleware have access to our **http context**.

#### Normally

When using **repository pattern** the approch we do to send back status code is \*\*

```csharp
  // Get api/values
  [HttpGet]
  public async Task<ActionResult<IEnumerable<Values>>> Get()
  {
      var values = await _context.Values.ToListAsync();
      if(values == null) return NotFound();
      return Ok(values);
  }
```

☝ this makes our api **smart** but the aim of **CQRS and Mediator** is to make our apis thin apis.

### AspNet core Identity

Current anybody can accessing anything with our routes. We need to have a scenario where all our user authenticate and in return gets a **jwt token** which is then sent up with every subsequent requests.

**Asp.Net core Identity** is a membership system that supports login stored in Identity. Asp.net core provides external providers such as google , facebook login. **Asp.Net core identity** will come with **default** user stores. It also gives use access to a **UserManagers** that able to do the **user managements.** and a **Signin managers**. Entity Framework also does the **password hashing** and **salting**.

Add the **identity package to our Domain** class.

```sharp
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 3.1.8
```

Add a **appuser** class and Iherite from **identity** as shown below.

```csharp
  public class AppUser: IdentityUser
  {
  public string DisplayName { get; set; }
  }
```

We also need to change the **Datacontext** class to inherit from a **IdentityDbContext** of type of the class we created. We don't need to add our appuser as a dbset in the **datacontext** class. We also need to pass the **model builder** to the base class.

```csharp
  protected override void OnModelCreating(ModelBuilder builder){
    base.OnModelCreating(builder);
}
```

The line we added ensures that our **appuser** table has a **primary key** of string.

The next step is to **generate migrations**.

```csharp
➜ dotnet ef migrations add "Added Identity" -p Persistence -s Api
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

In dotnet framework 3.0 in order to use thes **signinmanager** you will need to install the package below 👇.

```csharp
dotnet add package Microsoft.AspNetCore.Identity.UI --version 3.1.8
```

Run the application to apply migrations.

### JwT token Authentication

Jwt token is made up three parts. This token are used to **identify** users to an api. Using Jwt token, the server does not need to make a query to the database to determine you are who u say u are.

1. Header

- describes the **hashing** algorithm used to generate the key.

2. Payload.

- Usually described as the claims of of the jwt.
  contains
  1. iat - created at date
  2. exp - expiring date
  3. payload - id

3. Verfy Signature

Always note that the token, will be decoded by anyone. Also make sure that the payload is small since it will be sent along your requests.

The last part is **Verify Signature**. This part is used to tell if the token have been tempered with, or modified. This is a signature that is attached to the token and hashed using a **secret\*** and the hashing algorithm defined.

### workflow

- The user is **client/User** creates a request to the server sending a **username** and **a password**.
- The server **queries** the datbase to see if the specified password and username are correct.
- Once **identity** is verified the server issues a **jwt** token that is then use for other requests.
- The Jwt token could have a payload with **the user email or id** which is used to idetify whose request is whose.
  - The jwt token is set in the **Header** Authorization header as a **Bearer <jwt>**
- If the server **verify the** api token resources are given
- If the **jwt** token have been modified, expired . The server returns a **not authorized** response.

To configure this in our clean architecture , we will add a new project **infrastructure project**

### Adding an Infrastructure Projects

This project will be resposible for **JWT token Generations.**. All this project will no is how to generate a token.

```bash
➜ dotnet new classlib -n Infrastructure
```

we want our JwtGenerator to be available to our api thoroug **dependency Injection**. We create an interface on the Application projecte that has a method that returns a token as a string, and pass in a Appuser to the Jwt token.

In the Infastracture project in a **security** folder we create a class that implements the **intefaces** we created in the application folder. Finaly register the two with Dependecy injection in the startapp class as follows.

```csharp
services.AddScoped<IJwtGenerator, JwtGenerator>();
```

We need to add another package **System.IdentityMode.Token.Jwt\*** which provides access to Jwt claim names.

```bash
dotnet add package System.IdentityModel.Tokens.Jwt --version 6.8.0
```

After that is fully set up what remains is **adding authentication to our api**

### Adding Authentication

Let add a package **Microsoft.AspNetCore.Authentication.JwtBearer**. Always add package **same version** as your instSkip to section

### Securing ASP.NET Core apps with authentication

- .NET Core is is a general purpose programming platform that use can use to build almost anything, from desktop to web, clod, mobile, and even gaming. You do it all with the help of the .Net Core, but all these apps need to have a stable authentication system and with the .Net Core, this is easier than you may think. ASP .Net Core identity system is a membership system that adds all the necessary features to your ASP .Net Core apps. It also supports external login providers including Facebook, Twitter, LinkedIn and more. And last but not least, you can create your very own authentication system on top of .Net Core. During this course, we will learn how to set up the .Net Core identity system, how to use external login providers, and ever how to implement a cookie-based authentication system to authenticate our users. Hi, I'm Ervis Trupja, and I'm a web developer with a great passion for technology. I invite you to join me on this course to learn about authentication in .Net Core here on LinkedIn learning.

### What you should know

- [Instructor] Before we start this course, let us talk about a couple of things that you need to know. To build our web application, we are going to use Visual Studio 2017 Community edition. But you can also use any version of 2017 or later. Also, since we are going to build two apps, make sure that you have installed both .NET Core 2.0 and 2.2. We are going to build two ASP.NET Core MVC applications so having previous experience with the MVC framework is strongly recommended. Throughout this course, if you would like to follow along while I'm coding, you can download the Exercise Files from the course page.

### ASP.NET Core Security features

- [Narrator] When creating an app on top of the .NET Framework, or any other framework, it is important to have a robust security system because we are going to manage data and most of the times these data are critical. For example you might be storing user preferences, user emails, passwords, and much more. With ASP.NET Core it is possible to easily configure and manage security for our apps. ASP.NET Core contains features for managing authentication, which means that we can basically use the built in features of the .NET Framework to protect our apps from attackers. Another key feature that comes with .NET Core is authorization. So we get to use a built-in feature of .NET Core to check if a user is authorized or not. We are going to talk about the differences between authentication and authorization in the upcoming parts. Data protection, HTTPS enforcement, App secrets, anti-forgery protection and CORS management are some of the features that ASP.NET Core contains. ASP.NET Core provides many tools and libraries to secure our apps. And to do so, in .NET Core we can use both built-in identity providers and it also gives us the ability to use the third-party identity services such as social media. We are going to talk about this concept in details in the upcoming chapters.

### Authentication vs. authorization

- [Instructor] Authentication and authorization are two concepts that most people tend to confuse. Both terms are often used in conjunction with each other when it comes to security and gaining access to the system or the application. However, both these terms are quite different with completely different concepts. Authentication means confirming your own identity, but on the other end, authorization means being allowed to access the system. In even simpler terms, authentication is the process of verifying one's self, while authorization is the process of verifying that you have access to. Imagine a school as an application. In a school we have different people, usually the same like in an application that we have different users. For example, in a school we have students, and teachers, and in an application we have administrators, we have basic users, and we have managers. A school has different components, or different rooms, used for different purposes. So for example we have Principal's Office, we have teacher workrooms, and Classrooms. The same way in an application, we have different components, or features. So for example in an application, we might have an admin dashboard, we might have a user dashboard or profile, and chatrooms for example. Only the students and teachers of this school can enter this institution, because they exist in the records of the institution as teachers or students. In other words, they are authenticated to enter the building, so the school in this case, but they do not have access to all the rooms, which means that they are not authorized to have access everywhere they want. For example in a classroom, both students and teachers have access, but in a teacher workroom, only teachers have access, and to the Principal's Office, none of them has access. So this is the key difference between authentication and authorization.

### ASP.NET Core Identity

- In this chapter we are going to discuss the ASP.NET core identity framework in brief. As the name already indicates, identity is about identifying if the users are part of our replication or not. On the other hand the ASP.NET core identity is a membership system which adds the login functionality or the feature, to our apps and in this case the .NET core apps. ASP.NET core identity's really flexible when it comes to the way we identify, or we allow users to login to our .NET core apps. We can either use the default identity system or we can use external providers like, Facebook, Google, GitHub et cetera. And on the fourth chapter of this course we are going to learn in more details how to use Facebook and GitHub to authenticate our users. Identity can be configured using an SQL server database to store usernames, passwords and profile data. But of course you can use another persistent store like, for example **Azure Table storage.** On this chapter, we will learn how to use identity to register, login and logout a user.


#### Creating a web application with Identity

- [Instructor] On this part, we are going to learn how to create a .NET core application, and we will see how the SQL server database is used to store user related data. To create a .NET core application, we are going to use Visual Studio. I'm going to use Visual Studio 2017 Community Edition, but any version of Visual Studio 2017 or later is going to work just fine. In Visual Studio, to create a new project, go to file, then new, project. Let us provide a solution name, so I'm going to name this solution Identity, and let us provide a project name. We are going to create two .NET core applications. We are going to create one with the .NET 2.0 versions, so for that I'm going to name this project Auth20, which stands for 2.0, and we are going to create another one, of version 2.2, because the identity related files in these two versions are different from each other. Then next from the menu on the left make sure that you have chosen the web, and then in the middle column choose the ASP .NET Core Web Applications. Click the OK button to go to the next screen. And here from the dropdown on the top, make sure that you have selected the 2.0 version. And as the project template, choose the Web Application Model View Controller. Then change the authentication type to individual user accounts, and leave the dropdown value to the default one. Next, click the OK button. To create the project, click the OK button one more time. If you want to see all the files that were created by default, go to the solution explorer tab on the right. If you don't see this option here, go to the view, and then solution explorer. Here in the solution explorer we are going to see the controllers folder, and inside this folder we have the account controller.cs file. And this is the controller, which is just to handle all the identity related actions. Now, to run the application, click the play button on the top, and once the application has run successfully go to the register option on the top right. Here provide an email address, provide passwords, and click the register button to create a new account. So we see that we get an error which says that the password and the confirmation password do not match, so make sure that the password field has the same value as the confirm password field. So I'll just rewrite my password one more time. And click the register button again. Since we have not created our database yet, we are getting an option in here to apply migrations. And we use migrations to either create a database schema, or to update an existing database schema. And in our case, we don't have a database yet, so when we click the apply migrations button, a new database schema is going to be created. Now that the migrations were applied we can press the F5 button to see the result. So here we are going to see the hello, and the email that we used to create our account. If you are wondering where our data was stored, go to Visual Studio, then go to the solution explorer, next go to the appsettings.json file, and in here you are going to see that the server was set to be the localdb, \\mssqllocaldb. Copy this value, and then go to the server explorer option on the left. If you don't see this option, go to view, and then server explorer. Here, right click on the data connections, and then choose the add connection option, paste the value in the server name input box. Here remove one backslash, and then from the dropdown in here, choose the database that we just created. So, click the test connection button to make sure that the connection works, and then click the OK button. The database name that you see in here comes from the database value that we have in here. Let us go back to the server explorer, expand the database, inside here you are going to see a tables folder, and inside the tables folder, we have eight tables. The first table is the migrations history table, which is used to store all the migrations that we create while we develop our application. If you want to see the values, just right click, and then go to the show table data. And here you are going to see our first migration. Let us go back to server explorer, and to see the user that we just created, right click on the ASP net users table, and then show table data. Here, we are going to see the email address that we used to create our user. And now let us create a .NET core 2.2 application. For that, let us right click on this solution, then add, new project, change the name to Auth22, which stands for .NET core 2.2, here make sure that you have selected the ASP .NET Core Web Application, and then click the OK button. From the dropdown, change the value to 2.2, and then change authentication to individual user accounts. Click the OK button to create this project slow. Now if you go to solution export, inside the Auth22 project, we are going to see that inside the controllers folder, we don't have the account controller. And that is because in the .NET core 2.2, the entire identity UI is as a prebuilt package. So if you go inside the dependencies, and then inside the NuGet packages, inside here, let us expand the AspNetCore.app, here we are going to see the Microsoft.AspNetCore.identity.UI. And this package contains the entire identity UI. If you want to run this application, make sure that from the dropdown in here you have selected the Auth22 project, and then click the play button. Now, the same way let us go to the register, here provide some data, and click the register button to create this account. The same way, click the apply migrations button, and refresh the browser by pressing the F5 button. Let us now go back to Visual Studio, and go to server explorer, right click on the data connections to add a new connection, paste the server name in here, make sure that you remove one backslash, and then choose the database that was just created which is the Auth22 one. So, this one. Then click the OK button. Inside this database go inside the tables folder, and inside this folder right click on the ASP NET users table, and then choose the show table data option. So in here, we see the email of the user that we just created.

#### User Secrets

User secrets are usually available in **Development** mode an in production environment we do not have access to the user secrets. Set **dotnet secrets** in your startup applications.

To generate user secrets. Use the command.

```bash
dotnet user-secrets init -p api/
```

Add the token use the command.

```bash
dotnet user-secrets set "Tokenkey" "super secret key"
Successfully saved Tokenkey = super secret key to the secret store.
```

This key is specific to a machine. If you change your machine you will need to recreate the token again. using the above command.

To list out available keys use the command.

```bash
➜ dotnet user-secrets list -p Api
Tokenkey = super secret key
```

### Deployments

- Frameworks

**.net Framework.**

1.  Windows only.
2.  Older libraries or legacy code

-**.Net core**.

1. Cross-platform
2. fast and optimized

- Server.

.net core ships with 2 servers **HTTP.sys** and **kestrel**.

| Http.sys                       | kestrel          |
| ------------------------------ | ---------------- |
| Windows only                   | Cross platform   |
| Support Windows Authentication | Highly Optimized |
|                                | Recomended.      |

- How to expose the server to internet

1. kestrel can handl internet directly as an **edge server**.
2. kestrel can also be behind a reverse proxy or load balancer.

## Edge Server

### Behind Proxy

### Deployment Strategies

- IIS on windows - copy binaries.
- kestrl on linux - copy binaries.
- Azure App Service - copy binaries or CI
- kestrel on Docker Container.
  - This is the powerful way.

Whenever possible you should often avoid manually binaries.

In the \*Startapp.cs** the **IHostingEnvironment** objects lets you check at runtime the environment your are running on. Aspnet core use environment variable **ASPNETCORE_ENVIRONMENT\*\* to control the environment you running the code.

The **.csproj** file can be used to include and exlude files from deployments. There is a **CopyToPublishDirectory** attribute for **ItemGroup** elements that determines whether to copy the file to the publish directory and can have one of the following value.

- Always
- PreserveNewest
- Never

```<ItemGroup>

    <ResolvedFileToPublish Include="readme.md">
      <RelativePath>
      wwwroot\readme.md
      </RelativePath>
    </ResolvedFileToPublish>

  <None Include="notes.txt" CopyToOutputDirectory="Always" />
  <!-- CopyToOutputDirectory = { Always, PreserveNewest, Never } -->

  <Content Include="files\**\*" CopyToPublishDirectory="PreserveNewest" />
  <None Include="publishnotes.txt" CopyToPublishDirectory="Always" />
  <!-- CopyToPublishDirectory = { Always, PreserveNewest, Never } -->
</ItemGroup>xml
```

### Https

Enabled by default. This help to test https on the local machine.

This could be configured.

#### Using kestrel as an Edge server

To configure the server with a **letsencrypts** certificate. This is how you could do it.You must have the certificate on disk and the password for the certificate.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>().UseKestrel(options => {
                options.Listen(IPAddress.Loopback, 8088, listenOptions =>
                {
                    listenOptions.UseHttps("testcertifca.pfx", "password");
                });
            });
        });
```

#### Https For Reverse Proxies

It common to have have **servers** on **Reverse proxy** since it makes scalling to handle more traffic much easier. It also makes it easy to handle **https** certificates instead of configuring the certificate at the api level. We configure the certificates at the **reverse proxy** level. When a new request arrive the reverse proxy handles the https conections and then it turns around and makes and internal **http** request to our endpoint.

If you used the **aspnet core** in this way. Its important to configure **aspnet core** to look for the following Forwarded headers.

```bash
X-Forwarded-For: 203.0.113.195
X-Forwarded-Host: example.com
X-Forwarded-Proto: https
```

### Set up data protection

- ASP.NET Core uses the data protection API or DP API to encrypt and store keys that are used for authentication in your application. When hosting your application with IIS, you'll need to run a small script to create a registry hive for these keys, otherwise the keys will be regenerated when you restart the application or your server, which will invalidate any user sessions or cookies that were encrypted with the previous old keys. To create a data protection registry hive, you'll need a PowerShell script that's provided by the ASP.NET Core team. It's linked to from the ASP.NET Core documentation. It's hosted on GitHub, and to download it, you'll have to click the raw button on GitHub to display the raw text of the script. Then right click and choose save as. I'm gonna try saving it to the desktop just to make it easy. You'll need to put quotes around the file name so that the extension is saved properly and then click Save. Now that I have the script downloaded, I need an administrator level PowerShell window to execute the script in. I can open up PowerShell from the Start menu or from the task bar by typing PowerShell, and then I need to right click on PowerShell and choose Run as administrator. An administrator level PowerShell window starts in a different area of the file system, so I need to back up and navigate to my user folder and then navigate to the desktop. If I type ls, I can see the PowerShell script that I downloaded. Now I need to elevate this PowerShell window one more time to give it the remote signed execution policy, which allows me to execute that script. To do this, I'll type the command PowerShell again and then give it the -ExecutionPolicy parameter with the value RemoteSigned. This starts a new instance of PowerShell which has that additional privilege. Now I can type dot slash and the name of the script, Provision-AutoGenKeys.ps1, and pass it the name of the website that I created earlier. I named my HelloCoreWorld. When I hit Enter, the script will go out and provision the registry hive that I need and set up everything I need for IIS. With this in place, IIS is ready to host your ASP.NET Core application.

### Publish your app with Visual Studio

- [Instructor] Now that both the server and the project are ready, let's try actually deploying this application to IaaS with Visual Studio. Deploying ASP.NET Core applications using Visual Studio looks very similar to deploying applications in the past. You can access the publish wizard by right clicking on your project and selecting publish. If you've never published this particular project before, you'll need to create a publish profile. In this case, we want to deploy to the local file system, so we'll pick folder. Earlier, I created a folder for my published files that's already set up in IaaS. Once you click publish, Visual Studio will start automatically building and publishing your application. You can see the progress in the output panel. After it churns away for a bit, you'll see a successful message. This settings page contains a few more options, but the defaults should be fine. If you prefer to clean the output directory before publishing, check the delete all existing files prior to publish option. I like this option because it ensures the deployment directory starts from a clean slate. All of these settings are saved in a file called a publish profile, you can see these if you expand the properties item and the publish profiles item in the solution explorer. The publish profile supports the same markup we saw earlier in the project file. If you need to, you can manually define additional files to include or exclude and tweak other publishing related settings. The publish profile inherits any settings you've defined in the project file. I've already set up IaaS to monitor the publish directory for files. The website should be live once IaaS spins up. I'll switch over to my browser and try it out, going to localhost, and there we go. That's all that's necessary to successfully publish your application to IaaS using Visual Studio.

publish your app via the command line

- [Instructor] Deploying to IaaS via the command line is a useful alternative to using Visual Studio, especially if you want to script or automate the deployment process. The dotnet publish tool makes it easy to deploy an application using the command line or a script. I'll demonstrate how to use the tool from PowerShell to publish the application to IaaS. I've navigated to the project directory in PowerShell. First I need to run dotnet restore to restore any NuGet packages for the project, and then dotnet build just to make sure that it'll build correctly. Now I'll run dotnet publish, and then with the -c Release parameter, which will tell the application to build and publish in release mode. When the process is complete, the binaries will be placed in the bin directory. I can open up an explorer window to see where those landed. If I open up bin, release, netcoreapp1.1, which is the framework we're building on top of, and then the publish directory, all of the files and libraries needed to run the application are included here in the publish folder. You can copy the contents of this folder to the IaaS website folder manually, or via an automated script. Notice that this directory includes a web.config file. I'll explain why that's important and how it's generated next.

#### Web.config

### Understand Web.config

- You may have noticed that there's a Web.Config file in the directory, along with your published binaries. It plays an important role, but maybe not the one you expect. If you've used ASP.NET Core in the past, you're probably familiar with using Web.Config to store configuration for your application. ASP.NET Core doesn't use WebConfig because it uses a new configuration model based on appsettings.json instead. So why is WebConfig in this folder? It turns out that IIS still needs WebConfig, even if ASP.NET Core isn't using it. Web.Config is used to configure the ASP.NET Core module that IIS uses to act as a reverse proxy to Kestrel. When you run dotnet publish, WebConfig is generated for you automatically. You shouldn't need to touch it or modify it yourself. It is important that WEB.Config exists in the root of your published application. It's required for IIS to properly host your ASP.NET Core application, and it also prevents IIS from accidentally serving the content from the project directory, which could contain sensitive configuration files, so don't remove it. That wraps up our look at one of the deployment strategies. You should feel confident deploying ASP.NET Core applications to IIS on Windows.

### Get started with Azure

- [Instructor] Another option for hosting your ASP.NET Core applications is Microsoft Azure. Azure is a powerful platform for hosting web and mobile apps. It supports ASP.NET Core out of the box and Microsoft provides plugins for Visual Studio that make it really easy to deploy your application. I'll show you how to get started. There's two things you'll need. If you don't already have an Azure account, you can sign up for free at azure.microsoft.com. You will need to enter a credit card, but you'll only be charged for resources you use. It can take a couple of minutes to provision your account once you sign up. You can use that time to install the tooling for Visual Studio. Grab the latest SDK for Visual Studio at the Azure downloads page. Pick the version of Visual Studio that you use, in my case, Visual Studio 2015, and download the installer. The Azure SDK is a pretty big download, so it can take some time. Once the tools are finished installing, you're good to go. Next, I'll show you how to use Visual Studio to deploy an ASP.NET Core application to the Azure App Service.

### Deploy to Azure with Visual Studio

- [Instructor] Azure app service is a managed hosting service for your application, which means you don't have to worry about setting up virtual machines or servers. Visual Studio, along with the Azure SDK makes it really easy to deploy ASP.NET Core applications directly to Azure App Service. For this example, I'm again starting with the default ASP.NET Core mvc application template, you don't need to add any Azure related packages or settings to the project. To get the application deployed, we need to first set up an Azure App Service deployment profile. Right click on the project and select publish, and switch over to the profile tab if it isn't there already. We need to create a new publish profile based on the Azure App Service target, so click on that button, and if you haven't logged in to Azure through Visual Studio before, you'll be prompted to enter your credentials. Once you log in, we need to create a new resource group for this application. A resource group is kind of a high level container for resources you provision for the application in Azure, such as the App Service instance, databases, and so on. Click the new button on the right side of the screen. Here we can configure the web application name, which I'm gonna simplify just to HelloCoreWorld, and we need to type a name for the resource group as well, I'm just going to use HelloCoreWorld_Resources. We also have to create a new App Service plan, so click new, the default location is fine, and the free size is fine too, since we're just doing some testing. When I click create, these resources will be provisioned in Azure, which can take a few seconds. Once it's done provisioning those resources, the fields on the next step of the wizard will be populated for you automatically, this should all be good, but you can click validate connection just to test it and make sure it's all green, and that looks great. So when we click publish, Visual Studio is gonna compile the application and then upload it directly into Azure into that new App Service instance we just created. This can take a couple of seconds. At the end, we should see a success message at the bottom of the output window. When the publish process is done, you can click the URL to go open a browser and test out your website, or it may just open for you automatically. The URL that's provisioned is in the form of appname.azurewebsites.net, and once it spins up, you'll see your application running on Azure. And that's it, we've successfully deployed an ASP.NET Core application to the Azure App Service.



### Continuous deployment with Azure

- [Instructor] Earlier I demonstrated how to manually publish an ASP.NET Core application to the Azure app service using a publish profile in Visual Studio. Now let's take it a step further and set up continuous deployment from a source control repository. To set up continuous deployment, we'll need to log in to the Azure portal, and when you're logged in, find the app service instance that we created earlier. Under the app deployment and deployment options setting, click choose source to pick the source control that you want to connect this app service instance to, in my case I have my code out on GitHub, so I'll click on GitHub. If you haven't connected to a particular deployment source before, you'll need to authorize and login, which I've already done, then click on choose project and you'll see all of the available projects. In this case, all of my public GitHub repositories. The one I want is called HelloCoreWorld, and I can also choose the branch I wanna monitor for changes, in my case I want to deploy any time I check in code to the master branch, and that's it, I can click OK to start this deployment monitoring. Now in order to test this, let's make a small change to the application that we can see. I'll switch back over to Visual Studio, and we can make a small change to the index view. I'll add a little bit of text here so it'll be obvious that we made a change, and then I'll need to commit it to the GitHub repo. Once it's committed, I have to sync it to push it up to GitHub using the team explorer. To see this change, we can flip back over to the live application and refresh the page. It may take about 30 seconds to show up, but that change will be displayed for us right here, and that's it. Continuous deployment is a great way to automate the process of shipping code from your local machine to production, I highly recommend it over a manual deployment process.

### Install .NET Core on Linux

- [Instructor] Unlike previous versions of ASP.NET, you can run ASP.NET Core applications directly on Linux. This is a great option for applications that need to run on Linux-heavy infrastructure like Amazon Web Services or inside of Docker containers. To get started, you first need to install the .NET Core runtime packages on your Linux machine. The official .NET download site at microsoft.com/net/download includes instructions for Linux. What you need is the runtime. The instructions will vary by distribution. For this example, I'll use Ubuntu 16.04. We need to run a few commands to add the .NET sources to the package manager. Then we'll run sudo apt-get install apt-transport-https and sudo apt-get update. And finally, sudo apt-get install aspnetcore-runtime-2.1, which is the latest version at the time of this course. You can verify that everything was installed correctly by running dotnet --info. If you see this version output, then everything's working properly. Your Linux machine is now ready to host an ASP.NET Core application.

### Self-hosting with Kestrel

- [Instructor] Now that .NET Core is set up on our Linux machine, let's run an ASP.NET Core application. First, you'll need to get your application's published artifacts onto this machine. You can do this by running dotnet publish on another machine and copying the files over the network or by building and publishing from source directly on this machine. If you do wanna compile from source, you'll need to install the .NET Core SDK using apt-get in a similar way to how you installed the Runtime. Then you can do dotnet publish - c Release. No matter how you get the published files onto the server, once they're there, you'll want to go to the bin directory, cd bin, and the Release directory. And in this version, the directory underneath there is called netcoreapp2.1. And finally, the publish directory. This directory contains all of the files that are needed to run this application on this machine. What I wanna do is copy these into another place, maybe /var/websites, to hold all the files for my application. So I'll do sudo, make a directory over on /var/websites and another one for my application HelloCoreWorld. Let me back up a directory here. Then I'll do copy -r, publish everything under the publish folder over to /var/websites/HelloCoreWorld. Now I'll move over to that new directory. In this directory, we have a file called HelloCoreWorld.dll. That's the main entry point for my application. So if I run dotnet HelloCoreWorld.dll, the application'll spin up on port 5000 by default. At this point, I could take another machine, point it at the address of this machine with a browser, and see ASP.NET Core running and hosting the application right from this machine. That's all that's required to get Kestrel running on Linux. If you wanna run Kestrel as an edge server, you could change the port to 80 here and then just start serving real web traffic. For future scaling though, it's common to put a proxy like nginx in front of Kestrel. I'll show you how to do that next.

### Use Kestrel with NGINX

- [Instructor] It's common for Linux servers to use Nginx as a lightweight reverse proxy. I'll show you how to setup Nginx and configure it to forward requests to Kestrel. One way to do this easily on Ubuntu is with the Nginx PPA source. If I do sudo add-apt-repository ppa:nginx/stable, and then sudo apt-get update, and finally sudo apt-get install nginx, I can verify that Nginx installed successfully by doing sudo service nginx start. Nginx runs in the background, so if I go over to another machine and use the IP address of this machine in a web browser and try to browse it, I should see the default Nginx splash page. Nginx serves this page when everything is configured correctly, but there's nothing for it to point to. Now we need to configure Nginx. In order to edit the Nginx configuration, I need to move to /etc/nginx/sites-available/. There's a file called default here, which is the default configuration, I'm gonna make a backup of this real quick, nv copy it to default.backup, then I'll use nano to edit. I need to make a server block here, so I'll say server, we're going to listen on 80, we'll have a location called /, and then we'll set a bunch of parameters here. Passing through to local host 5000, we are proxying http version 1.1, and we need to set some http headers. We'll also set the connection header to keep alive, we'll set the host header to the current host value, we'll set cache bypass, and finally, we'll set the x forwarded headers. We'll say x-forwarded-for and we'll also set x-forwarded-Proto. So let's save this, and we can use sudo nginx -t to test it. On a production server, it's important to set the server_name property as well to the domain name that your site will be hosted on. I'm leaving this off for testing, but it's an important security practice on a real server. This configuration includes the x-forwarded-for, and x-forwarded-Proto headers, so if Nginx is handling https connections, it can let your ASP.NET Core app know that the connection to the browser is encrypted. If you install an https certificate on your Nginx server, go watch the configure forwarded headers middleware video to understand how to set up your ASP.NET Core application to look for these headers. You can get more information about how to configure Nginx and write this configuration file at the Nginx website. Now that the configuration has been written and has tested out okay, we'll do sudo nginx -s reload, to restart it. Now I'm gonna move back to my websites folder, var/websites/HelloCoreWorld, and I'm gonna do dotnet HelloCoreWorld to start up the server once again. This is still hosting on port 5000, but now we have Kestrel listening on 80, forwarding to 5000. If we switch to another machine and open up a browser, we should see the ASP.NET Core application. And there we go, ASP.NET Core running on a Linux server behind Nginx, which is proxying port 80 to port 5000. If you get a connection error message in your browser instead, ASP.NET core might be trying to redirect port 80 back to port 5001. In this case, you could temporarily remove or comment out the use https redirection line in configure in startup.cs until you install a proper https certificate on your Linux machine.

### Start the application automatically

- [Instructor] The Nginx service starts up automatically when the machine boots, but Kestrel and the application have to be started manually. You can use the systemd tool to make sure that Kestrel starts hosting your application whenever the machine restarts. The first thing to do is to create a service definition file. We'll do that with sudo nano /etc/systemd/system/ and we'll call this service hellocoreworld.service. We need to add a couple of things to this configuration file. First a unit section, which just contains a description. We'll say Example .NET web app running on Ubuntu. And we have a service section. The working directory for the service is /var/websites/HelloCoreWorld. Start command is /usr/bin/dotnet and then the path to the DLL file. Let's say we want this to always restart. Basically keep it alive. Provide an identifier for the logs. Just say hellocoreworld. The user it'll run under is me for now, but you could use a service user or a websites user. And specify environment variables. ASPNETCORE_ENVIRONMENT, which determines whether the application is running in development or production. We want production. And we can also set DOTNET_PRINT_TELEMETRY_MESSAGE to false just to clean up the logs. In the install section, we'll say wanted by multi-user.target. And that should do it. Let's go ahead and save this file. And try enabling it. We'll do systemctl enable hellocoreworld.service. It'll ask for a password. Okay. Well, now let's try systemctl... Nope, that was my password. Let me try that again. And my password. There we go. Now let's try systemctl start hellocoreworld.service. Okay, we can check the status logs to see if it started correctly. We'll do systemctl status hellocoreworld.service. And we can see here that all the ASP.NET logs have outputted to the systemctl log. We're running in production, listening on 5000 just like before, but now the application will start up automatically whenever the machine restarts.

### Docker overview

- [Instructor] Containerization is a new technology that many ASP.NET developers may not yet have a lot of experience with. Docker is the most popular containerization tool today. I'll explain how Docker works at a high level and why it's a good choice for deploying ASP.NET Core applications. When you set up a server or virtual machine to host your application, think of all the stuff that you have to do. Install dependencies like .NET and third-party libraries, configure things like IIS and NGINX, add environment variables and so on. If you have multiple servers, you have to do this setup manually on each one. Because of this, adding and maintaining servers becomes a complex task. With Docker, you instead create an image from your application that includes all of the required dependencies, files, and setup steps. The Docker image contains everything needed to take a machine from a blank slate all the way up to running your application. You can then use this image to create one or more Docker containers. To use a programming metaphor, think of images as classes. A container is a process that runs on the Docker host and is isolated from other running processes on the machine. The container is a live version of the image, so to continue the programming metaphor, think of containers as instances of the classes. The Docker host can run many containers at once, all isolated from each other. This approach has a few benefits. Instead of managing servers or virtual machines that have been carefully set up to run your application, you only need a server that can run Docker. The dependencies and setup steps required to host and run your application are explicitly defined in the Docker image. That means that Docker images become the fundamental unit of deployment. When you build a new version of your application, you create an updated Docker image and push that out to your running Docker containers. Images can be versions tagged and swapped in and out of containers easily, so adding more servers just means spinning up more containers from the same image. All of this means that using Docker makes deploying and managing your application servers much easier. The .NET Core team at Microsoft has created a set of base images that make it straightforward to deploy your ASP.NET Core applications using Docker.

### Create a Docker image

- [Instructor] The first step to creating Docker images is installing Docker on your development machine. The official Docker website has instructions for Windows, Mac, and Linux. Since I'm on Windows 10, I installed the version for Windows. When you finish installing Docker and restart your machine, you can open up PowerShell and run Docker --version to make sure Docker is installed and running. The next thing that you need is a Docker file. The Docker file is like a recipe that tells Docker how to build an image for your application. You can create a Docker file from inside Visual Studio by right clicking on your project and choosing add, Docker support. You can also write this file by hand if you aren't using Visual Studio. I'll demonstrate creating this file with Notepad so you can understand what each command does. It's important that this file be saved in the root of the project, in the same directory as the csproj or program.cs files. If you're on Windows, use quotes to save the file with no extension. The docker file will start with FROM microsoft/dotnet:2.1, which is the latest version at the time of this course, - sdk AS build. This tells Docker that we're starting from the Microsoft .NET Core sdk base image. Then we'll say WORKDIR/src to move into a virtual directory inside of the Docker image. We wanna copy all of our source files into the Docker image temporarily so we can build the application. We'll start with just the csproj file first. Copy anything .csproj into the Docker image, and then we're gonna RUN dotnet restore to pull down any packages that we need to build the application. After that restore step, we'll COPY the rest of the rest of the source files, and then RUN dotnet publish -c Release, and say that output should go into another virtual directory called /app. Splitting up the restore and publish steps in this way allows Docker to optimize how the packages get pulled down when we're doing the NuGet restore. Now that we've built the application, we'll say FROM microsoft/dotnet:2.1-aspnetcore-runtime, since we don't need the sdk any longer, AS runtime. Switch into that /app directory, COPY things from the build step into /app, we need to set an environment variable, so we'll say ENV, the environment variable is called ASPNETCORE_URLS, this tells ASP.NET Core what ports and URLS it should bind to. We'll stick with the default configuration of binding to port 5000 by saying http://*:5000. You can also bind to https if you have an https certificate configured in your Docker container by saying, https://*5001, for example, we'll stick with http to keep it simple for now. Then we finally need to say ENTRYPOINT and give the command to start the application, which is "dotnet", "HelloCoreWorld.dll". Alright, I'll save this, and then I'll switch to PowerShell. From the application directory, I need to say Docker build, give it a tag, we'll say -t hellocoreworld just as a name for the image, and then a period to say that we wanna build this image from the current directory. Once the image has been built, it can be run locally on this machine, or on any Docker host.

### Test the Docker image locally

- [Instructor] Now that we've built an image with Docker build, we can test it on our local machine. First use Docker images to list the images that are available, hellocoreworld is the name of the image I created before. I can run this image using Docker run. I'll use the -it flag to tell Docker to take all of the output from the container and pipe it to this console window. I'll also use the -p flag to map port 5000 from inside of the container, which was exposed with the expose command in the Docker file, to port 5000 on this machine, on my local machine. And finally I'll specify the name of the image that I want to spin up. When I hit enter, Docker will take this image, create a container from it, and then run the container and show me the output. And it works really fast, so in just that amount of time I have the container spun up, Kestrel started up, and it's listening for requests. If I switch over to a browser and browse to localhost5000, I should start interacting with that container and getting a response back. As you can see, testing Docker images and containers is really straightforward. Next I'll explore running and monitoring this container as a background process.

### Run and monitor a container

- [Instructor] On a real server, you'll wanna run the container, or multiple containers as background processes. If we use Docker run with the -d flag, it'll start the container in the background. We still need -p to map the ports from inside the container to our local machine, and we'll specify the name of the image to start up. We can use Docker ps to monitor the status of this running container. The output of Docker ps is usually too wide for a single window, so I like to use the --format command to make it a little bit easier to read. I prefer using table .names .image .status, and. ports, this just customizes what output Docker ps will send to the screen. So that's a little bit more readable. This tells us that this image, HelloCoreWorld was spun up into the container called hungry_kare, it's been up for about 40 seconds, and it has internal port 5000 mapped to external port 5000. We can use Docker ps to monitor the status of this container, or we can use Docker stop, followed by the generated container name, hungry_kare in this case, to shut the container down. Next we'll explore using NGINX to expose our application to the internet.

### Docker Compose overview

- [Instructor] We now have Kestrel running in a Docker container but it's not accepting traffic on port 80. As I mentioned earlier, the best practice is to put a reverse proxy in front of Kestrel. We can use Docker to create another container running NGINX that will proxy requests to our Kestrel container. And you might be wondering, why would we use a Linux server like NGINX if I'm developing this on Windows? The reason is that under the hood Docker runs these containers on top of a small Linux Kernel, even on Windows. When I spin up a container for my Kestrel image, which is based on the dot net core base image, it's actually starting up a small Debian Linux environment. Docker includes a tool called Docker Compose that helps you create multi container applications. We'll use it to create a simple container that runs NGINX, pair that with our existing Kestrel container, and then configure NGINX to proxy requests to Kestrel.

### Kestrel and NGINX with Compose

- [Instructor] First we need to create a Docker file for the new Nginx container we need. To keep things organized, I'll create a folder here in my project folder called nginx. I'll use Notepad to create the new Docker file and save it in the nginx folder as Dockerfile with no extension. There's already a public base image for Nginx, so we can use FROM nginx to get started really quickly. The only thing we need to do is customize the Nginx configuration, so we'll copy our own nginx.conf file into the image. We need to copy it to /etc/nginx/nginx.conf. And that takes care of the docker file. Now we need to create nginx.conf. Save this also in the nginx folder as nginx.conf with quotes to preserve the extension. This Nginx configuration will be very similar to the configuration we used earlier when we configured Nginx on a Linux machine. This time it includes a few more elements. We need an events group that specifies the maximum number of concurrent worker connections. This means that up to 1024 active connections can be handled by Nginx at once. And we need an http group which will contain a server group. We want, of course, to listen on port 80. We're gonna send those requests to a location that we'll define here. We'll proxy those requests to Kestrel port 5000. We'll come back to this in a moment. I also need to set some boilerplate things, the HTTP version to 1.1, set the upgrade header, set the connection header, set the host header, and set cache bypass to follow the upgrade setting. That takes care of nginx.conf. Next we'll need a set of instructions for Docker Compose that tells Compose how we wanna structure our multi-container application. Compose looks for a YAML file called docker-compose.yml. I'll create that and save it in the root of my project. Save this as docker-compose.yml with quotes. This file specifies the containers that we wanna spin up when we run Docker Compose, and in our case we need two. We need one container for Nginx. We'll build this from the Docker file in the current directory slash nginx subdirectory, and we need to link this container to another container that we'll define in a moment called kestrel. This is where that host name comes from. We want to expose port 80 internally from the container and externally to our physical machine. And then for our other container, we'll build it from the Docker file in the root directory. So that's our Kestrel image. And we need to expose port 5000 internally. So in this case, it means that port 5000 will be exposed from our Kestrel container, but only inside of Docker. When we spin up both of these containers together, port 80 will be exposed on our local machine. Port 5000 will not be. It will only be exposed inside of the Docker environment. And that's necessary so that the Nginx container can proxy requests to the Kestrel container. I'll save this file, and now let's switch over to PowerShell. From my project root directory, I can run docker-compose up, which will read that YAML file, build the images, and then spin up the containers that we need. At the end of the build process, we should have Nginx listening on port 80 proxying those requests over to Kestrel. We can test this in a browser. If I switch over to Chrome and navigate to localhost, notice there's no port there so I'm hitting port 80, and there's my ASP.NET Core application. As we've seen, docker-compose makes it possible to build complex, multi-container applications with Docker.

### Save an image to a file

- [Instructor] We've build a number of images using Docker build, but these are all stored locally on our machine. What if we need to transfer this image to another machine to push it to a production server, for example. That's where the Docker save command comes in. If we run Docker images, we can see all the images that I've stored locally on this machine. To export or save one of these, we can run Docker save -o to specify an output filename, in this case I'll call it hellocore.tar, and specify the image I wanna save out. This command will save a tar ball of the image as a single file on the file system that you can then easily transport to another computer. This command can take a few minutes, especially if it's a large image, in my case it's almost 600 megabytes. When it's done we can see the file on the file system, 600 megabytes of image there. The opposite of the save command is the load command, I can use Docker load -i and the filename to load that image back into Docker and get it ready for deployment. You can also use Docker hub to share images between machines. I'll show you how to do that next.

### Publish an image to Docker Hub

- [Instructor] Docker Hub is an online repository for sharing Docker images. You can push images that you've built up to Docker Hub, and then pull them down at a later time, or on a different machine. Storing public images on Docker Hub is free, so it's a useful way to share and maintain images that you use. To use Docker Hub, sign up for an account at hub.docker.com, then in the terminal, run Docker login and enter your credentials. I've already logged in on this machine. Images you push to Docker Hub must follow a naming scheme, where the name of your Docker Hub repository comes first, followed by a slash, and then the name of the image. Your personal repository name is your username, so I need to rebuild my image to include my username. I'll rebuild my project image by running Docker build -t, and for the name, I'll use my username, nbarbettini/ and then the image name, hellocoreworld, and a period to build it from the current denstuuirectory. Once the image is built, I can push it up to Docker Hub by using Docker push, and then the image name. When the process is done, my image will be visible online at hub.docker.com, and to pull that image back down, I can just run Docker pull and the image name. As we've seen, Docker and Docker Hub provide a number of powerful tools to help you deploy applications in containers. Since ASP.NET Core runs natively on the Linux environments Docker uses, Docker's a great choice for deploying ASP.NET Core applications.

### Publish an image to Docker Hub

- [Instructor] Docker Hub is an online repository for sharing Docker images. You can push images that you've built up to Docker Hub, and then pull them down at a later time, or on a different machine. Storing public images on Docker Hub is free, so it's a useful way to share and maintain images that you use. To use Docker Hub, sign up for an account at hub.docker.com, then in the terminal, run Docker login and enter your credentials. I've already logged in on this machine. Images you push to Docker Hub must follow a naming scheme, where the name of your Docker Hub repository comes first, followed by a slash, and then the name of the image. Your personal repository name is your username, so I need to rebuild my image to include my username. I'll rebuild my project image by running Docker build -t, and for the name, I'll use my username, nbarbettini/ and then the image name, hellocoreworld, and a period to build it from the current directory. Once the image is built, I can push it up to Docker Hub by using Docker push, and then the image name. When the process is done, my image will be visible online at hub.docker.com, and to pull that image back down, I can just run Docker pull and the image name. As we've seen, Docker and Docker Hub provide a number of powerful tools to help you deploy applications in containers. Since ASP.NET Core runs natively on the Linux environments Docker uses, Docker's a great choice for deploying ASP.NET Core applications.

### Photo Upload

#### Photo storage

Cloud service Scalable could be more expensive secured with Api Key.

