# Rudy (.NET) 

A minimal Redis-like distributed key-value store built in .NET 9 with support for:

- ✅ Pub/Sub
- ✅ Persistence
- ✅ Replication & Clustering
- ✅ Async I/O
- ✅ Performance benchmarking
- ✅ Full integration tests

---

## 📦 Features

- ⚡ In-memory key-value store
- 📬 Publish/Subscribe channels
- 🧠 Replication between master & replicas
- 🧾 Persistent storage (append-only file)
- 🧪 Full integration tests with TCP clients
- 📊 Benchmarking on csv for throughput/latency

---

## 🧰 Technologies

- [.NET 9](https://dotnet.microsoft.com/)
- TCP sockets w/ async I/O
- xUnit (testing)

---

## 🧑‍💻 Getting Started

### 🚀 Configure and run

```
var server = RudyServerBuilder.Initialize($"{masterPort}.log")
            .WithIpAddress("127.0.0.1")
            .WithPort(6390)
            .Build();
     
server.Start();
     
server.Stop();
```

The example above shows how to configure a **RudyServer**, starting and stopping.

#### Replica connection

```
await server.ConnectAsReplicaAsync("127.0.0.1", 6391);
```

The example above shows how to configure a **RudyServer** as a replica for a specific master.

## 🧪 Testing
```bash
dotnet test
```

This command will launch **MemoryStoreTests** and **IntegrationTests**.

### 📈 Benchmarking
This part is part of the test suite. It is in the **IntegrationTests.cs** class and will write the result in the console with **ITestOutputHelper**.

## 🛢️ Commands allowed

- PING
- SET
- GET
- DEL
- SUB
- PUB
- CLONE
- HEALTH


## 🧑‍💻 Contributing
Pull requests welcome! Ideas:

- TLS / Auth support
- Web dashboard for cluster stats
- LRU/TTL key eviction
- Snapshotting & AOF replay
- CLI tool
- Docker Support
- Better concurrency management

## 📜 License
MIT © 2025 AndreaPrestia

# ![Alt text](Rudy.png)
