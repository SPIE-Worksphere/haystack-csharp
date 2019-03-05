using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectHaystack.Client;

namespace ProjectHaystack
{
  public static class Program
  {
    public static void Main(string[] args)
    {
      if (args.Length != 3)
      {
        Console.WriteLine("usage: HClient <uri> <user> <pass>");
        Environment.Exit(0);
      }

      RunAsync(args[0], args[1], args[2]).Wait();
    }

    public static async Task RunAsync(string uri, string user, string pass)
    {
      var client = new HAsyncClient(new Uri(uri), user, pass);
      await client.OpenAsync();
      Console.WriteLine(await client.GetStringAsync("about", new Dictionary<string, string>(), "text/zinc"));
      Console.ReadKey();
    }
  }
}