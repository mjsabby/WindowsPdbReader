# WindowsPdbReader
Cross platform Windows PDB Reader for .NET Core to read windows pdb

# Purpose of this repo

In priority order.

1. To fix https://github.com/dotnet/corefx/issues/29375
2. Cross-platform support for https://github.com/Microsoft/BPerf
3. Support debugging Windows PDBs on VSCode (lines & locals - nothing else)

# What works right now

You can pass an method token, il offset and get back the line number. It's useful for the aforementioned issue but also tools like profilers can use this.

```csharp
        GetSourceLineInfo(Stream fs, uint token, uint iloffset, out int age, out Guid guid, out string sourceFile, out int sourceLine, out int sourceColumn)
```