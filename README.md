# README #

[![Build Status](https://worksphere.visualstudio.com/Worksphere-OSS/_apis/build/status/Strukton-Worksphere.haystack-csharp?branchName=master)](https://worksphere.visualstudio.com/Worksphere-OSS/_build/latest?definitionId=231&branchName=master)

### What is this repository for? ###

Connecting to Haystack 3.0 via SCRAM mechanism.

Update (Ian Davies) : August 2018 : data model from Java Toolkit has been ported with the following noted exceptions that can be used with the previous work to implement a fuller client with the added HGrid and postString methods.  Noted differences to the Java toolkit are:
	- Timezones reply on TimeZoneConverter (https://github.com/mj1856/TimeZoneConverter) (OSS license saved in Haystack project) - There are some slight differences between the calculations in this 
	    and Java toolkit.
	- equals to avoid confusion with .NET operands is hequals in this toolkit - unit tests have been adapted to reflect this.
 Tested thus far with unit tests and some simple operations without SCRAM with the Java toolkit Server (Gradle appBeforeIntegrationTests deployment).
Update (Chris Breederveld) : March 2019 :
	- Moved to .NET standard for broader use.
	- Added async client to better make use of the underlying asynchronisity of web requests.

### How do I get set up? ###

Below is example code found in Program.cs.
HClient and HAsyncClient are very basic implementations and contain methods to create clients, get data, and post data.
To use this you must pass in the uri, user, and pass as arguments when running the program.
This is an example of the GetStringAsync method to get information about the current client.
    
```
#!c#
var client = new HAsyncClient(new Uri(uri), user, pass);
await client.OpenAsync();
Console.WriteLine(await client.GetStringAsync("about", new Dictionary<string, string>(), "text/zinc"));
```

### Who do I talk to? ###

* Repo owner or admin
* Other community or team contact. 
* Consider posting questions on the [Project Haystack Forum](http://project-haystack.org/forum/topic)
