#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Net.Http;
using System.Threading.Tasks;
using GTranslate.Translators;
#endregion

public class GoogleTranslateLogic : BaseNetLogic
{
    [ExportMethod]
    public void TranslateMissingKeys()
    {
        // Insert code to be executed by the method
        translateDictionaryTask?.Dispose();
        translateDictionaryTask = new LongRunningTask(TranslateMissingKeysMethod, LogicObject);
        translateDictionaryTask.Start();
    }

    private void TranslateMissingKeysMethod()
    {
        // Get the dictionary to be translated
        var localizationDictionary = GetLocalizationDictionary() ?? throw new ArgumentException("Could not load LocalizationDictionary");

        // Load the content of the dictionary
        var actualDictionaryValues = LoadDictionaryContent(localizationDictionary) ?? throw new ArgumentException("Could not load dictionary content");

        // Translate the content of the dictionary using non-empty keys
        var newDictionaryValues = TranslateDictionaryContent(actualDictionaryValues);

        // Update the dictionary with the translated content
        if (UpdateDictionaryContent(localizationDictionary, newDictionaryValues))
            Log.Info(LogicObject.BrowseName, "Dictionary updated successfully");
        else
            Log.Error(LogicObject.BrowseName, "Error updating dictionary");
    }

    private IUAVariable GetLocalizationDictionary()
    {
        // Get the dictionary to be translated
        IUAVariable dictionaryPointer = LogicObject.GetVariable("LocalizationDictionary");
        if (dictionaryPointer == null)
        {
            Log.Error(LogicObject.BrowseName, "LocalizationDictionary variable not found");
            return null;
        }

        NodeId targetDictionary = dictionaryPointer.Value;
        if (targetDictionary == null)
        {
            Log.Error(LogicObject.BrowseName, "LocalizationDictionary variable is not initialized");
            return null;
        }

        IUAVariable dictionary = InformationModel.GetVariable(targetDictionary);
        if (dictionary == null)
        {
            Log.Error(LogicObject.BrowseName, "LocalizationDictionary variable not found in the Information Model");
            return null;
        }

        return dictionary;
    }

    private string[,] LoadDictionaryContent(IUAVariable localizationDictionary)
    {
        string[,] actualDictionaryValues = null;
        try
        {
            actualDictionaryValues = localizationDictionary.Value.Value as string[,];
        }
        catch (Exception exception)
        {
            Log.Error(LogicObject.BrowseName, $"Error loading dictionary content: {exception.Message}");
        }
        return actualDictionaryValues;
    }

    private string[,] TranslateDictionaryContent(string[,] dictionaryContent)
    {
        // Counter to store the elements that have been translated
        int translatedElements = 0;

        // First row of the dictionary contains the languages
        string[] languagesList = Enumerable.Range(0, dictionaryContent.GetLength(1)).Select(x => dictionaryContent[0, x]).ToArray();
        Log.Info(LogicObject.BrowseName, $"Found {languagesList.Length} languages in the dictionary");

        // Translate each empty key in the dictionary (skip first line which is header)
        for (int lineIndex = 1; lineIndex < dictionaryContent.GetLength(0); lineIndex++)
        {
            // For each line try to get any cell which is not empty to be used as source
            var textToBeTranslated = "";
            var sourceLanguage = "";
            for (int columnIndex = 1; columnIndex < dictionaryContent.GetLength(1); columnIndex++)
            {
                if (!string.IsNullOrEmpty(dictionaryContent[lineIndex, columnIndex]))
                {
                    textToBeTranslated = dictionaryContent[lineIndex, columnIndex];
                    sourceLanguage = languagesList[columnIndex];
                    break;
                }
            }
            if (string.IsNullOrEmpty(textToBeTranslated))
            {
                Log.Warning(LogicObject.BrowseName, $"Could not find any text to be translated for key \"{dictionaryContent[lineIndex, 0]}\" at line {lineIndex}");
                continue;
            }
            // Skip the first column which is the localization key
            for (int columnIndex = 1; columnIndex < dictionaryContent.GetLength(1); columnIndex++)
            {
                if (string.IsNullOrEmpty(dictionaryContent[lineIndex, columnIndex]))
                {
                    Log.Debug(LogicObject.BrowseName, $"Translating key \"{dictionaryContent[lineIndex, 0]}\" from {languagesList[0]} to {languagesList[columnIndex]}");
                    var translatedText = TranslateText(textToBeTranslated, languagesList[columnIndex], sourceLanguage);
                    if (string.IsNullOrEmpty(translatedText))
                    {
                        Log.Warning(LogicObject.BrowseName, $"Could not translate key \"{dictionaryContent[lineIndex, 0]}\" from {languagesList[0]} to {languagesList[columnIndex]}");
                    }
                    else
                    {
                        Log.Debug(LogicObject.BrowseName, $"Translated key \"{dictionaryContent[lineIndex, 0]}\" from {languagesList[0]} to {languagesList[columnIndex]}: {translatedText}");
                        dictionaryContent[lineIndex, columnIndex] = translatedText;
                        translatedElements++;
                    }
                }
            }
        }

        Log.Info(LogicObject.BrowseName, $"Translated {translatedElements} elements in the dictionary");
        return dictionaryContent;
    }

    private static string TranslateText(string textToTranslate, string targetLanguage, string sourceLanguage)
    {
        var translator = new GoogleTranslator();
        // Get source language
        var from = "";
        if (!string.IsNullOrEmpty(sourceLanguage))
            from = sourceLanguage.Split('-')[0];

        // Get target language
        var to = "";
        if (string.IsNullOrEmpty(targetLanguage))
            throw new ArgumentException("Target language is required.");
        else
            to = targetLanguage.Split('-')[0];

        // Force the translation to be synchronous
        var result = translator.TranslateAsync(textToTranslate, to, from).GetAwaiter().GetResult();

        return result.Translation;
    }

    private bool UpdateDictionaryContent(IUAVariable dictionaryToUpdate, string[,] newDictionaryContent)
    {
        try
        {
            dictionaryToUpdate.Value = new UAValue(newDictionaryContent);
            return true;
        }
        catch (Exception exception)
        {
            Log.Error(LogicObject.BrowseName, $"Error updating dictionary content: {exception.Message}");
            return false;
        }
    }

    private LongRunningTask translateDictionaryTask;
}
