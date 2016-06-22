# Clearing-Public
Sample code for accessing the Nordpool Clearing APIs.

There are two different versions of the C# sample application. Both versions are simple console applications, with a Visual Studio 2015 solution, targeting the .NET 4.5 framework.

The solution in *CSharp-simple* folder is the most simple version of making an API request against the Clearing Trade Capture API. You can use this as the first proto implementation, to see hands-on how to make a simple request.

The solution in *CSharp* folder contains a more evolved version of the sample application. When run, you can choose from three different query modes:
* A single query, printing the results in JSON format
* Repeated queries, querying trades by the Delivery time
* Repeated queries, querying trades by the trading time, making use of the delta search functionality

The last two options issues that would be necessary in a production-grade implementation, like:
* Refreshing an expired access token
* Synchronizing requests so that a new request is not started until a new one is finished
