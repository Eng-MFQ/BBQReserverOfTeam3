## BBQReserver
Bot for reserving the BBQ zone in Innopolis.
You can find the hosted bot in [@BBQReserver_bot](https://t.me/BBQReserver_bot).

## Prerequisite for running the BBQReserver software
- Microsoft DotNet Core SDK, Version 2.2

## You can get this software here
- Mac: https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.402-macos-x64-installer
- Ubuntu 19.04: https://dotnet.microsoft.com/download/linux-package-manager/ubuntu19-04/sdk-2.2.402
- Other systems: https://dotnet.microsoft.com/download/dotnet-core/2.2

Make sure to have version 2.2 installed

### Install DotNet for Linux
- Ubuntu 19.04
```
$ wget -q https://packages.microsoft.com/config/ubuntu/19.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
$ sudo dpkg -i packages-microsoft-prod.deb
```

```
$ sudo apt-get update
$ sudo apt-get install apt-transport-https
$ sudo apt-get update
$ sudo apt-get install dotnet-sdk-2.2
```

> Another Linux Installation Instruction detaul can be found in here [Installation Instruction](https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-1904).

### Install DotNet for Mac
- Download the [DotNet Version 2.2 installer](https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.402-macos-x64-installer)
- Run the installer

### Install DotNet for Windows
- Download the [DotNet Version 2.2 installer](https://dotnet.microsoft.com/download/thank-you/dotnet-sdk-2.2.402-macos-x64-installer)
- Run the installer

## Run the bot
Navigate to **BBQReserverBot** subfolder and run the following code:
```
$ cd BBQReserverBot
$ dotnet build
```
change to **BBQReserverBot/BBQReserverBot** folder one level deeper
```
$ cd BBQReserverBot
$ dotnet run
```
## Run the tests
Navigate to the first level **BBQReserverBot** folder and run the following code:
```
$ dotnet test
```

## Information
After starting the software, you can reach the bot under the following name/url:
[@BBQTesterBot](https://t.me/BBQTesterBot)
