### folder structure
 
  src/
  sample/
  test/
  .gitignore
  ./github > githubaction
  LICENSE.txt  > serch a good one
  readme.md

#### src/

### **csproj** > Dependecies used : Hubspot client

Flurl is a modern, fluent, asynchronous, testable, portable, buzzword-laden URL builder and HTTP client library for .NET.

url https://flurl.dev/docs/fluent-url/


### Microsoft.CSharp

Provide code complilation and code generation


### RapidCore

RapidCore is a collection of dotnet libraries to help you accelerate backend and api development

1. A class to handle exceptions.
2. MainStuff done
   Dto
   inteface
   httpclient > to do i

 abstract class Baseclient
  properties.
     readonly Httpclient
     readony  ILogger
     readonly _serializer
     string  HUbSpotBaseUrl
     string __apikey/ token;

3. Specifc
   Dto
    contains all menhod definitions < generic methods
      creat, createBatch, Delete, deleteBatch
   interface
   specific client > implement the abstract class.


### test
  functional> ensure it works as it is expected.
     functionalTestBase<T>
  Mocks/ >  dumpl data. to test functinality
         
  integration > testing against the real api.
    IntegrationTestBase<T>

  unit
     UnittestBase<T>


### GitLabclient

