# ovodotnet
The OVO .Net Client.

The OVO .Net Client library can connect a cluster of OVO nodes and offers the same API calls as the Go client library. 

## Download the OVO client library for .Net
The library can be downloaded from *Nuget.org* at https://www.nuget.org/packages/OVOdotNetClient/ or using the Nuget Package Manager.
```
PM> Install-Package OVOdotNetClient
```

## Client usage

### Client initialization
```c#
	OvoDotNetClient.Client client = new OvoDotNetClient.Client(); // initialize the client using the default configuration file 
    client.SetLog(System.Console.Out); // enable the console logs 
```
```c#	
	OvoDotNetClient.Model.Configuration config = new OvoDotNetClient.Model.Configuration(); // create a client's configuration object
	config.ClusterNodes.Add(new OvoDotNetClient.Model.Node()
	{
		Host = "localhost",
		Port = 5050
	});
	OvoDotNetClient.Client client = new OvoDotNetClient.Client(config);  initialize the client using the configuration object
```

### Put and Get an object
```c#
 	var p1 = new Product 
	{
		Id = 101,
		Name = "bread",
		Description = "food made of flour, water, and yeast, mixed together and baked",
		CreationDate = DateTime.Now(),
	};
    client.Put("myproduct", p1, 0);
	Product p2 = client.Get<Product>("myproduct");
```

### Delete an object from the storage
```c#
    client.Delete("myproduct");
	Product p2 = client.Get<Product>("myproduct"); // p2 is null
```

### Working with counters
```c#

	long result1 = client.Increment("mycounter", 7, 0);  // the first call to Increment creates a new counter and set it to the value -> 7
	long result2 = client.Increment("mycounter", 10, 0); // icrements the counter -> 17
	long result3 = client.SetCounter("mycounter", 5, 0); // reset the value of the counter -> 5
	long result4 = client.Increment("mycounter", -1, 0); // decrements the value of the counter -> 4
	long result5 = client.GetCounter("mycounter"); // read the counter without changing the value -> 4
	
	long result6 = client.Increment("mycounter2", 1, 60); // create a counter that reset (expires) after 60 secs

```

## Test the client
A complete list of tests of all the client methods is implemented in the test project TestOvoDotNetClient.