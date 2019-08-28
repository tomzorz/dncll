# dncll

### *DotNetCoreLicenseLister* - The easy way to list all licenses used by your .net core / standard dependencies.

This command line tool allows you to list all the .net dependencies and their licenses, just point it at an `.sln`.

![platform-any](https://img.shields.io/badge/platform-any-green.svg?longCache=true&style=flat-square) ![nuget-yes](https://img.shields.io/badge/nuget-yes-green.svg?longCache=true&style=flat-square) ![license-MIT](https://img.shields.io/badge/license-MIT-blue.svg?longCache=true&style=flat-square)

You must have [.NET Core 2.1 SDK](https://www.microsoft.com/net/download/windows) or higher installed.

⚠ 👉 Yes, the **SDK**, not just the runtime, as dncll relies on the `dotnet ...` commands.

## Use the pre-built `dncll`

You can quickly install and try [toitnups from nuget.org](https://www.nuget.org/packages/dncll/) using the following commands:

```console
dotnet tool install -g dncll
dncll YourSolution.sln licenses.txt
```

> Note: You may need to open a new command/terminal window the first time you install the tool.

## Special thanks

Thanks to Jerrie Pelser for the excellent work on AnalyzeDotNetProject, building on which greatly made implementing dncll easier.

See blog post: https://www.jerriepelser.com/blog/analyze-dotnet-project-dependencies-part-2/

See github project: https://github.com/jerriepelser-blog/AnalyzeDotNetProject