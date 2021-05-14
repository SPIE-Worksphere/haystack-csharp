# README

[![Build Status](https://worksphere.visualstudio.com/Worksphere-OSS/_apis/build/status/Strukton-Worksphere.haystack-csharp?branchName=master)](https://worksphere.visualstudio.com/Worksphere-OSS/_build/latest?definitionId=231&branchName=master)

## Introduction

The .NET Haystack client library is built to ease communicating with Haystack compatible libraries. The NuGet package [ProjectHaystack.Client](https://www.nuget.org/packages/ProjectHaystack.Client/) can be found on nuget.org.

There are two main functions:

- Athentication
- Conversion between haystack text and managed models

## Authentication

The [Haystack authentication](https://project-haystack.org/doc/Auth) officially supports [https://project-haystack.org/doc/Auth#scram](SCRAM) and unofficially others (like basic authentication) all through a handshake mechanism that tells the client what methods are supported.

See the Clients chapter for more information about the authentication selection.

## Clients

As of version 2.0 older clients have been removed in favor of a single modern interface.
If you have a high dependency on the old clients you should use the 1.x version of the library.

### HaystackClient

The `HaystackClient` is a modular client that uses the `HttpClient` implementation for communication and an `IAuthenticator` for the authentication method.

It can be used to do "raw" requests to the server, but it can also be used to automatically convert between queries and results into the managed classes. For conversion it relies on the `HZincWriter` and `HZincReader` classes.

## Usage

This chapter describes some usage examples of the library.

### Client creation

```C#
var user = "someuser";
var pass = "somepassword";
var uri = new Uri("https://someserver/api/");
var auth = new AutodetectAuthenticator(user, pass);
var client = new HaystackClient(auth, uri);
```

### Opening the connection

```C#
client.OpenAsync();
```

### Simple call

One of the simplest calls is the call of the "about" page which contains some basic information about the server. This call requires no content, so it simply sends an empty grid.

```C#
HaystackDictionary result = await client.AboutAsync();
```

### Raw call

It is possible to do a raw call to the server if you do mainly want to use the authentication mechanism, but not the conversion.
This can be useful if you do conversion elsewhere, or if you want to get specific data types from the server, like json:

```C#
string result = await client.PostStringAsync("about", string.Empty, "text/zinc", "application/json");
```

### Grid query

When sending more complex requests, like an Axon query in SkySpark you can build and send a grid with a specific operation.

```C#
var axon = "someaxon";
var grid = new HaystackGrid()
	.AddColum("expr")
	.AddRow(new HaystackString(axon));
HaystackGrid[] result = await client.EvalAllAsync(grid);
```

### Authenticators

As the AutodetectAuthenticator adds a little bit of overhead and complexity and doesn't cover all cases, it may be useful to use a specific authentication method provided in the library or one that you created.

```C#
var auth = new ScramAuthenticator(user, pass);
var client = new HaystackClient(auth, uri);
```

Other examples of authenticators are `BasicAuthenticator` and `NonHaystackBasicAuthenticator`.

### HttpClient

The `HaystackClient` creates its own static default `HttpClient`, but you can also provide your own for more control over the communication.
The default authentication uses cookies, so if you provide your own client, you need to provide it a cookie container to be able to authenticate properly.

```C#
var handler = new HttpClientHandler() { UseCookies = false, AllowAutoRedirect = false };
var httpClient = new HttpClient(handler);
var client = new HaystackClient(httpClient, auth, uri);
```

## Contact

If you have questions you can ask them on the [Project Haystack Forum](http://project-haystack.org/forum/topic).
If you have issues or suggestions for the library you can create a new issue in the [repository](https://github.com/Strukton-Worksphere/haystack-csharp/issues).
If you want to contribute, just create your fork and create a pull request on the main [repository](https://github.com/Strukton-Worksphere/haystack-csharp).

## History

Originally the library is a .NET Framework port from the Java Toolkit with the exception of a dependency on TimeZoneConverter.

Over time it has been modified to .NET 5.0, dropping support for the old functional origins and providing native .NET features like asynchronous functions and enumerators.
