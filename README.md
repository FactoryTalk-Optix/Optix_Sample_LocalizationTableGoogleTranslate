# GoogleTranslateLogic

## Overview

`GoogleTranslateLogic` is a NetLogic designed to translate missing keys in a localization dictionary of FactoryTalk Optix using the Google Translate API. This class leverages the `GTranslate` NuGet package to perform translations.

## Prerequisites

- FactoryTalk Optix
- .NET 8 SDK
- Visual Studio
- NuGet package: `GTranslate` ([link](https://github.com/d4n3436/GTranslate))

## Installation

1. Open the NetSolution in Visual Studio.
2. Install the `GTranslate` NuGet package using the `NuGet Package Manager` or by running the following command in the Package Manager Console:
   
```sh
Install-Package GTranslate
```


## How It Works

### Main Methods

1. **TranslateMissingKeys**: This method initiates the translation process by starting a long-running task.
   
```csharp
public void TranslateMissingKeys()
```

2. **TranslateMissingKeysMethod**: This method performs the actual translation process. It retrieves the localization dictionary, loads its content, translates the missing keys, and updates the dictionary.
   
```csharp
private void TranslateMissingKeysMethod()
```


3. **GetLocalizationDictionary**: This method retrieves the localization dictionary from the Information Model.

```csharp
private IUAVariable GetLocalizationDictionary()
```


4. **LoadDictionaryContent**: This method loads the content of the localization dictionary.
   
```csharp
private string[,] LoadDictionaryContent(IUAVariable localizationDictionary)
```


5. **TranslateDictionaryContent**: This method translates the content of the dictionary using non-empty keys.

```csharp
private string[,] TranslateDictionaryContent(string[,] dictionaryContent)
```


6. **TranslateText**: This method uses the `GoogleTranslator` class from the `GTranslate.Translators` package to translate text.

```csharp
private static string TranslateText(string textToTranslate, string targetLanguage, string sourceLanguage)
```

7. **UpdateDictionaryContent**: This method updates the localization dictionary with the translated content.

```csharp
private bool UpdateDictionaryContent(IUAVariable dictionaryToUpdate, string[,] newDictionaryContent)
```

### Usage

1. Ensure that the `LocalizationDictionary` variable is properly set up in the NetLogic object.
2. Call the `TranslateMissingKeys` method to start the translation process.

## Logging

The class uses the `Log` object to log information, warnings, and errors throughout the translation process. This helps in monitoring the progress and identifying any issues that may arise.

## Notes

- The translation process is synchronous to ensure that all translations are completed before updating the dictionary.
- The first row of the dictionary is assumed to contain the language headers.
- The first column of the dictionary is assumed to contain the localization keys.

## License

This project is licensed under the MIT License. See the LICENSE file for details.
