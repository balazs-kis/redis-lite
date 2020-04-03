![RedisLite](/Logo/logo-title.png)

Redis Lite is a small, simple redis client for .NET (Standard). It implements the often used redic commands, and then some.

[![Build Status](https://travis-ci.org/balazs-kis/redis-lite.svg?branch=master)](https://travis-ci.org/balazs-kis/redis-lite)
[![Coverage Status](https://coveralls.io/repos/github/balazs-kis/redis-lite/badge.svg?branch=master)](https://coveralls.io/github/balazs-kis/redis-lite?branch=master)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Usage

Create a `RedisClient` and connect it to the redis server of your choosing:
```csharp
var client = new RedisClient();
var connectionSettings = new ConnectionSettings(address: "127.0.0.1", port: 6379);
client.Connect(connectionSettings);
```
Then you can start *commanding*:
```csharp
client.Set("MyKey", "MyValue");
var result = client.Get("MyKey");
client.Del("MyKey");
```

## Included redis commands
The list of included redis commands can be seen here: [RedisCommands.cs](/RedisLite.Client/CommandBuilders/RedisCommands.cs)
