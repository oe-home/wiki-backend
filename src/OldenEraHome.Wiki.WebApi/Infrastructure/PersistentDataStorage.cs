using System.Diagnostics.CodeAnalysis;
using OldenEraHome.Wiki.WebApi.Abstractions;
using OldenEraHome.Wiki.WebApi.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using LangDict = System.Collections.Generic.Dictionary<string, string>.AlternateLookup<System.ReadOnlySpan<char>>;

namespace OldenEraHome.Wiki.WebApi.Infrastructure;

public sealed class PersistentDataStorage() : IPersistentDataStorage
{
    private Dictionary<string, WikiData>? Data { get; set; }

    public async Task<Creature[]> GetCreaturesAsync(string locale)
    {
        await InitAsync();
        return Data[locale].Creatures;
    }

    #region Initialization

    [MemberNotNull(nameof(Data))]
    private async Task InitAsync()
    {
        if (Data is not null)
        {
            return;
        }

        var yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        var localeFolder = Path.Combine(AppContext.BaseDirectory, "data", "locale");
        var availableLocales = Directory
            .EnumerateFiles(localeFolder)
            .Select(fileName => fileName.Split('.').First())
            .ToArray();

        Data = new Dictionary<string, WikiData>(availableLocales.Length);

        var abilitiesDb = await LoadAbilitiesDbAsync(yamlDeserializer);
        var creaturesDb = await LoadCreaturesDbAsync(yamlDeserializer);

        foreach (var locale in availableLocales)
        {
            var localeDict = await LoadLocaleFileAsync(yamlDeserializer, localeFolder, locale);
            var creatures = creaturesDb.Select(c => c.ToDomain(localeDict, abilitiesDb)).ToArray();
            Data.Add(locale, new WikiData(creatures));
        }
    }

    private static async Task<LangDict> LoadLocaleFileAsync(IDeserializer deserializer, string localeFolder, string locale)
    {
        var fileName = Path.Combine(localeFolder, $"{locale}.yml");
        var localeDict = await LoadFromYamlFileAsync<Dictionary<string, string>>(deserializer, fileName);
        return localeDict.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    private static Task<Dictionary<string, AbilityDb>> LoadAbilitiesDbAsync(IDeserializer deserializer)
    {
        var fileName = Path.Combine(AppContext.BaseDirectory, "data", "abilities.yml");
        return LoadFromYamlFileAsync<Dictionary<string, AbilityDb>>(deserializer, fileName);
    }

    private static Task<CreatureDb[]> LoadCreaturesDbAsync(IDeserializer deserializer)
    {
        var fileName = Path.Combine(AppContext.BaseDirectory, "data", "creatures.yml");
        return LoadFromYamlFileAsync<CreatureDb[]>(deserializer, fileName);
    }

    private static async Task<T> LoadFromYamlFileAsync<T>(IDeserializer deserializer, string fileName)
    {
        try
        {
            using var file = File.OpenRead(fileName);
            using var streamReader = new StreamReader(file);
            var fileData = await streamReader.ReadToEndAsync();
            var result = deserializer.Deserialize<T>(fileData);
            return result;
        }
        catch (Exception e)
        {
            throw new InvalidCastException($"Can't deserialize file: {fileName}", e);
        }
    }

    #endregion

    #region Helpers 
    private static string LocalizeString(string value, LangDict langDict)
    {
        if (!value.StartsWith('$'))
        {
            return value;
        }
        if (langDict.TryGetValue(value.AsSpan()[1..], out var localizedValue))
        {
            return localizedValue;
        }

        throw new InvalidCastException($"Can't be resolve localization for value: {value}");
    }
    #endregion

    #region Models
    private sealed record WikiData(
        Creature[] Creatures
    );

    public sealed record CreatureDb(
        string Name,
        uint Level,
        string Type,
        string Fraction,
        uint Health,
        uint Atack,
        uint Defence,
        uint MinDamage,
        uint MaxDamage,
        uint Initiative,
        uint Speed,
        int Morale,
        int Luck,
        string[] Abilities
    )
    {
        public Creature ToDomain(
            LangDict langDict,
            Dictionary<string, AbilityDb> abilitiesDb)
        {
            var abilities = abilitiesDb
                .ToDictionary(k => k.Key, a => a.Value.ToDomain(langDict));

            return new Creature(
                Name: LocalizeString(Name, langDict),
                Level,
                Type,
                Fraction: LocalizeString(Fraction, langDict),
                Health,
                Atack,
                Defence,
                MinDamage,
                MaxDamage,
                Initiative,
                Speed,
                Morale,
                Luck,
                Abilities: [.. Abilities.Select(a => abilities[a])]
            );
        }
    };

    public sealed record AbilityDb(
        string Name,
        string Description
    )
    {
        public Ability ToDomain(LangDict langDict)
        {
            return new Ability(
                Name: LocalizeString(Name, langDict),
                Description: LocalizeString(Description, langDict)
            );
        }
    }
    #endregion
}
