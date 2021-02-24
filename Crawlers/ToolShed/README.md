# ToolShed Offline Crawler

## Build

There are multiple options to build/compile the crawler. 
The simplest option is to compile a host-dependant version, 
i.e., you are required to have `.NET 5.0` installed on 
your operating system before running the crawler. 
To complie in this fashion, you may run the following 
command: 

```shell
$ cd .\TVQ\Crawlers\ToolShed
$ dotnet publish --output publish
```

However, if you want to compile the crawler to 
host-independent executables (i.e., you do not need
to have `.NET 5.0` installed on the operating system
to run the crawler), you can run the following commands 
depending on your operating system.

```shell
$ cd .\TVQ\Crawlers\ToolShed
$ dotnet publish .\ToolShedCrawler.csproj --output publish --runtime win-x64

# Or: 
$ dotnet publish .\ToolShedCrawler.csproj --output publish --runtime win-x64 -p:PublishReadyToRun=true -p:PublishTrimmed=true 
```

Common values for the `--runtime` argument are `win-x64`, `linux-x64`, `osx-x64`.
For a complete list of supported runtime identifiers,
see [this page](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).
For a complete list of the `publish` arguments, 
[see this page](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish).

## Run

```shell
$ cd publish
# Cross-platform:
$ dotnet .\ToolShedCrawler.dll  -c categories.json -t tools.json -p publications.json

# Or on Windows-only:
$ .\ToolShedCrawler.exe  -c categories.json -t tools.json -p publications.json
```
