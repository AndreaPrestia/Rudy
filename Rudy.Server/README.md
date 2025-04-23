# 🚀 Rudy (.NET)

A minimal Redis-like distributed key-value store built in .NET 8 with support for:

- ✅ Pub/Sub
- ✅ Persistence
- ✅ Replication & Clustering
- ✅ Async I/O
- ✅ Docker & Docker Compose support
- ✅ CLI Tool with REPL, autocomplete, command history
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
- 🛠️ Custom CLI client (`rudy-cli`)

---

## 🧰 Technologies

- [.NET 8](https://dotnet.microsoft.com/)
- TCP sockets w/ async I/O
- Docker & Docker Compose
- System.CommandLine (CLI)
- xUnit (testing)

---

## 🧑‍💻 Getting Started

### 🚀 Run with Docker Compose

```bash
docker-compose up --build

Master on localhost:6379

Replicas on 6380, 6381
```

## 💬 CLI Tool: rudy-cli

```bash
dotnet build Rudy.Cli

dotnet run --project Rudy.Cli -- exec "SET mykey hello"
dotnet run --project Rudy.Cli -- exec "GET mykey" --host 127.0.0.1 --port 6380
```

## 💻 Launch REPL
```bash
dotnet run --project Rudy.Cli -- repl
```

## 🧪 Testing
```bash
dotnet test
```

## 📈 Benchmarking
```bash
dotnet run --project Rudy.Core.Benchmark -c Release
```
Outputs full benchmark table:

```bash
| Method | Mean      | Error     | Gen 0 | Allocated |
|------- |----------:|----------:|------:|----------:|
| Set    | 1.234 us  | 0.045 us  | 0.0076 |   56 B    |
| Get    | 1.123 us  | 0.038 us  | 0.0051 |   48 B    |
```

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

### ✅ CLI Dockerfile
```dockerfile
# Dockerfile.cli
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish RedisCloneCli -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./redisclone-cli"]
```

### 🧰 Makefile
```makefile
build:
	dotnet build Rudy.sln

cli:
	dotnet run --project Rudy.Cli -- exec "PING"

cli-repl:
	dotnet run --project Rudy.Cli -- repl

cli-docker:
	docker build -t rudy-cli -f Dockerfile .
	docker run -it rudy-cli repl

publish:
	dotnet publish Rudy.Cli -c Release -o bin/cli

docker-compose:
	docker-compose up --build
```

### 🧠 Advanced CLI (via System.CommandLine)
✅ Features:
- --host and --port flags
- Autocomplete + help
- repl mode with live command history
- Automatic retry on TCP failure

```bash
# Run command
rudy-cli exec "GET foo" --host 127.0.0.1 --port 6380

# REPL mode
rudy-cli repl
```

## 🧑‍💻 Contributing
Pull requests welcome! Ideas:

- RESP protocol support

- TLS / Auth support

- Redis CLI compatibility

- Web dashboard for cluster stats

- LRU/TTL key eviction

- Snapshotting & AOF replay

## 📜 License
MIT © 2025 AndreaPrestia