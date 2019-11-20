# WordExtractor

## Install
1. Install [.NET Core](https://dotnet.microsoft.com/download/dotnet-core)
2. To try out the tool run `dotnet tool install -g wordextractor`

## Use as a library
Run `dotnet add package WordExtractor --version 1.1.0`


## Example
```csharp
var dictionary = new Dictionary<string, int> {
    {"test", 3},
    {"hello", 1},
    {"item", 0},
};

WordInferer inferer = new WordInferer(dictionary);

Console.WriteLine(inferer.Infer(i));
```

I have a list of words, that you can use. If I have not replaced this text with instructions, feel free to open an issue.