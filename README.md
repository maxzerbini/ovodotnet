# ovodotnet
The OVO .Net Client.

The OVO .Net Client can connect a cluster of OVO nodes and offers the same API calls as the Go client library. 

## Download the OVO client library for .Net
The library can be downloaded from *Nuget.org* at https://www.nuget.org/packages/OVOdotNetClient/ or using the Nuget Package Manager.
```
PM> Install-Package OVOdotNetClient
```

## Client usage

### Client initialization
```c#
	OvoDotNetClient.Client client = new OvoDotNetClient.Client();
    client.SetLog(System.Console.Out); // enable the console logs 
	
```

### Put and Get
```c#
 	var p1 = new Product 
	{
		Id = 101,
		Name = "bread",
		Description = "food made of flour, water, and yeast or another leavening agent, mixed together and baked",
		CreationDate = DateTime.Now(),
	};
    client.Put("myproduct", p1, 0);
	Product p2 = client.Get<Product>("myproduct");
```
