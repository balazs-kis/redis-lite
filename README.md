![RedisLite](https://raw.githubusercontent.com/balazs-kis/redis-lite/master/Logo/logo-title.png)

Redis Lite is a small, simple redis client for .NET (Standard). It implements the often used redis commands, and then some.

[![Build Status](https://travis-ci.org/balazs-kis/redis-lite.svg?branch=master)](https://travis-ci.org/balazs-kis/redis-lite)
[![Coverage Status](https://coveralls.io/repos/github/balazs-kis/redis-lite/badge.svg?branch=master)](https://coveralls.io/github/balazs-kis/redis-lite?branch=master)
[![Nuget](https://img.shields.io/nuget/v/RedisLite)](https://www.nuget.org/packages/RedisLite)
[![License: MIT](https://img.shields.io/badge/license-MIT-blueviolet)](https://opensource.org/licenses/MIT)
[![pull requests: welcome](https://img.shields.io/badge/pull%20requests-welcome-brightgreen)](https://github.com/balazs-kis/redis-lite/fork)

## Usage

### AsyncRedisClient
Create an `AsyncRedisClient` and connect it to the redis server of your choosing:
```csharp
using(var client = new AsyncRedisClient())
{
    var connectionSettings = new ConnectionSettings(address: "127.0.0.1", port: 6379);
    await client.Connect(connectionSettings);
```
Then you can start *commanding*:
```csharp
    await client.Set("MyKey", "MyValue");
    var result = await client.Get("MyKey");
    await client.Del("MyKey");
    ...
}
```

### AsyncRedisSubscriptionClient
Subscribing to redis channels is done through a special client, the `AsyncRedisSubscriptionClient`:
```csharp
// Create a client and connect to a server:
var subscriber = new AsyncRedisSubscriptionClient();
var connectionSettings = new ConnectionSettings(address: "127.0.0.1", port: 6379);
await subscriber.Connect(connectionSettings);

// Register callcback for subscription:
subscriber.OnMessageReceived += (channel, message) =>
{
    // Do something when a message arrives.
};

// Subscribe to the channel you wish to recieve messages from:
subscriber.Subscribe("MyChannel");
```

Sending messages to a channel is done through the regular `AsyncRedisClient`:
```csharp
// Create a client and connect to a server:
var publisher = new AsyncRedisClient();
var connectionSettings = new ConnectionSettings(address: "127.0.0.1", port: 6379);
await publisher.Connect(connectionSettings);

// Publish a message to a given channel:
await publisher.Publish("MyChannel", "My interesting message");
```

## Included redis commands
The list of included redis commands can be seen here: [RedisCommands.cs](https://raw.githubusercontent.com/balazs-kis/redis-lite/master/RedisLite.Client/CommandBuilders/RedisCommands.cs)
