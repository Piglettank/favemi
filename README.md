# favemi

A Windows image viewer. Open a folder, step through its images, and copy the ones you like into a `Favorites` folder inside it.

An image is favorited when a file with the same name already exists in `Favorites`. Favoriting never overwrites or removes that copy.

## Requirements
- Windows 10 version 2004 or later, or Windows 11 (x64 or ARM64)
- Visual Studio 2022 with the **.NET desktop development** workload, or the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build
**Visual Studio:** open `Favemi.sln`, pick **x64** (or **ARM64** on an ARM PC), and press F5.

**Command line:**
```powershell
dotnet build Favemi.sln -c Debug -p:Platform=x64
.\Favemi\bin\x64\Debug\net8.0-windows10.0.19041.0\Favemi.exe
```

## Using it
- **Open folder** or **Ctrl+O**, or drop a folder on the window. Only image files directly in that folder are shown (not subfolders).
- **Left** / **Right** or Previous / Next move through the queue. The ends do not wrap.
- **Favorite** copies the current file into `<opened folder>\Favorites` under the same name.
