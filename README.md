# README #

### What is this repository for? ###

Connecting to Haystack 3.0 via SCRAM mechanism. Currently, this library provides a haystack client with a very low-level API for invoking ops and getting the result as a raw string. In the future we hope to add support for the Haystack data model as well.

Update (Ian Davies) : August 2018 : data model from Java Toolkit has been ported with the following noted exceptions that can be used with the previous work to implement a fuller client with the added HGrid and postString methods.  Noted differences to the Java toolkit are:
	- Timezones reply on TimeZoneConverter (https://github.com/mj1856/TimeZoneConverter) (OSS license saved in Haystack project) - There are some slight differences between the calculations in this 
	    and Java toolkit.
	- equals to avoid confusion with .NET operands is hequals in this toolkit - unit tests have been adapted to reflect this.
 Tested thus far with unit tests and some simple operations without SCRAM with the Java toolkit Server (Gradle appBeforeIntegrationTests deployment).

### How do I get set up? ###

Below is the main method found in HClient.cs. HClient is a very basic implementation and contains methods to create clients, get strings, and post strings. To use this you must pass in the uri, user, and pass as arguments when running the program. In the main method you can also see an example of the GetString method to get the information about the current client.
    
```
#!c#
public static void Main(string[] args)
{
  try
  {
    if (args.Length != 3)
    {
      Console.WriteLine("usage: HClient <uri> <user> <pass>");
      Environment.Exit(0);
    }

    HClient client = MakeClient(args[0], args[1], args[2]);
    Console.WriteLine(client.GetString("about", new Dictionary<string, string>(), "text/zinc"));
    Console.ReadKey();
  }
  catch (Exception e)
  {
    throw e;
  }
}
```

### Who do I talk to? ###

* Repo owner or admin
* Other community or team contact. 
* Consider posting questions on the [Project Haystack Forum](http://project-haystack.org/forum/topic)