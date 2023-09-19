# WinUI3 -Task List App.Core

## Release Notes

### v1.0.0.1 - August 2023

The **Core** project contains code that can be [reused across multiple application projects](https://docs.microsoft.com/dotnet/standard/net-standard#net-5-and-net-standard).

* The bulk of the file routines employ `System.IO.File` and `System.IO.Directory`.

* If you are using a packaged project then you can get the current folder by calling `Windows.Storage.ApplicationData.Current.LocalFolder.Path`.

* If you are using an unpackaged project then you can get the current folder by calling `System.IO.Directory.GetCurrentDirectory()` or `System.AppContext.BaseDirectory`.

