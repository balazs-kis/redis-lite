![RedisLite](https://raw.githubusercontent.com/balazs-kis/redis-lite/master/Logo/logo-title.png)

Redis Lite is a small, simple redis client for .NET (Standard). It implements the often used redis commands, and then some.

[![Build Status](https://travis-ci.org/balazs-kis/redis-lite.svg?branch=master)](https://travis-ci.org/balazs-kis/redis-lite)
[![Coverage Status](https://coveralls.io/repos/github/balazs-kis/redis-lite/badge.svg?branch=master)](https://coveralls.io/github/balazs-kis/redis-lite?branch=master)
[![Nuget](https://img.shields.io/nuget/v/RedisLite)](https://www.nuget.org/packages/RedisLite)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![pull requests: welcome](https://img.shields.io/badge/pull%20requests-welcome-brightgreen)](https://github.com/balazs-kis/redis-lite/fork)

## Usage

### RedisClient
Create a `RedisClient` and connect it to the redis server of your choosing:
```csharp
using(var client = new RedisClient())
{
    var connectionSettings = new ConnectionSettings(address: "127.0.0.1", port: 6379);
    client.Connect(connectionSettings);
```
Then you can start *commanding*:
```csharp
    client.Set("MyKey", "MyValue");
    var result = client.Get("MyKey");
    client.Del("MyKey");
    ...
}
```

### RedisSubscriptionClient
Subscribing to redis channels is done through a special client, the `RedisSubscriptionClient`:
```csharp
// Create a client and connect to a server:
var subscriber = new RedisSubscriptionClient();
var connectionSettings = new ConnectionSettings(address: "127.0.0.1", port: 6379);
subscriber.Connect(connectionSettings);

// Register callcback for subscription:
dutSubscriber.OnMessageReceived += (channel, message) =>
{
    // Do something when a message arrives.
};

// Subscribe to the channel you wish to recieve messages from:
dutSubscriber.Subscribe("MyChannel");
```

Sending messages to a channel is done through the regular `RedisClient`:
```csharp
// Create a client and connect to a server:
var publisher = new RedisClient();
var connectionSettings = new ConnectionSettings(address: "127.0.0.1", port: 6379);
publisher.Connect(connectionSettings);

// Publish a message to a given channel:
publisher.Publish("MyChannel", "My interesting message");
```

## Included redis commands
The list of included redis commands can be seen here: [RedisCommands.cs](https://raw.githubusercontent.com/balazs-kis/redis-lite/master/RedisLite.Client/CommandBuilders/RedisCommands.cs)
