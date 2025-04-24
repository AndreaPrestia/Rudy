# 🚀 Rudy (.NET)

A minimal Redis-like distributed key-value store built in .NET 8 with support for:

- ✅ Pub/Sub
- ✅ Persistence
- ✅ Replication & Clustering
- ✅ Async I/O
- ✅ Docker & Docker Compose support
- ✅ Performance benchmarking
- ✅ Full integration tests

---

## 📦 Features

- ⚡ In-memory key-value store
- 📬 Publish/Subscribe channels
- 🧠 Replication between master & replicas
- 🧾 Persistent storage (append-only file)
- 🧪 Full integration tests with TCP clients
- 📊 Benchmarking tools for throughput/latency

---

## 🧰 Technologies

- [.NET 8](https://dotnet.microsoft.com/)
- TCP sockets w/ async I/O
- Docker & Docker Compose
- xUnit (testing)

---

## 🧑‍💻 Getting Started

### 🚀 Run with Docker Compose

```bash
docker-compose up --build

Master on localhost:6379

Replicas on 6380, 6381
```

## 🧪 Testing
```bash
dotnet test
```

### 📈 Benchmarking
This part is part of the test suite. It is in the **IntegrationTest.cs** class.

## 🐳 Docker Support

### ✅ Server Dockerfile
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Rudy.Core -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Rudy.Core.dll"]
```

## 🧑‍💻 Contributing
Pull requests welcome! Ideas:

- TLS / Auth support

- Web dashboard for cluster stats

- LRU/TTL key eviction

- Snapshotting & AOF replay

## 📜 License
MIT © 2025 AndreaPrestia

# ![Alt text](Rudy.png)