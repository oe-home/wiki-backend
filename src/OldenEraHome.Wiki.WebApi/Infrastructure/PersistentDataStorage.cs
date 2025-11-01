using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using OldenEraHome.Wiki.WebApi.Abstractions;
using OldenEraHome.Wiki.WebApi.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using LangDict = System.Collections.Generic.Dictionary<string, string>.AlternateLookup<System.ReadOnlySpan<char>>;

namespace OldenEraHome.Wiki.WebApi.Infrastructure;

public sealed class PersistentDataStorage(IFileSystem fileSystem) : IPersistentDataStorage
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
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithEnforceRequiredMembers()
            .Build();

        var localeFolder = fileSystem.Path.Combine(AppContext.BaseDirectory, "data", "locale");
        var availableLocales = fileSystem.Directory
            .EnumerateFiles(localeFolder)
            .Select(fileSystem.Path.GetFileNameWithoutExtension)
            .ToArray();

        Data = new Dictionary<string, WikiData>(availableLocales.Length);

        var abilitiesDb = await LoadAbilitiesDbAsync(yamlDeserializer);
        var creaturesDb = await LoadCreaturesDbAsync(yamlDeserializer);

        foreach (var locale in availableLocales)
        {
            if (locale is null)
            {
                continue;
            }
            var localeDict = await LoadLocaleFileAsync(yamlDeserializer, localeFolder, locale);
            var creatures = creaturesDb.Select(c => c.ToDomain(localeDict, abilitiesDb)).ToArray();
            Data.Add(locale, new WikiData(creatures));
        }
    }

    private async Task<LangDict> LoadLocaleFileAsync(IDeserializer deserializer, string localeFolder, string locale)
    {
        var fileName = fileSystem.Path.Combine(localeFolder, $"{locale}.yml");
        var localeDict = await LoadFromYamlFileAsync<Dictionary<string, string>>(deserializer, fileName);
        return localeDict.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    private Task<Dictionary<string, AbilityDb>> LoadAbilitiesDbAsync(IDeserializer deserializer)
    {
        var fileName = fileSystem.Path.Combine(AppContext.BaseDirectory, "data", "abilities.yml");
        return LoadFromYamlFileAsync<Dictionary<string, AbilityDb>>(deserializer, fileName);
    }

    private Task<CreatureDb[]> LoadCreaturesDbAsync(IDeserializer deserializer)
    {
        var fileName = fileSystem.Path.Combine(AppContext.BaseDirectory, "data", "creatures.yml");
        return LoadFromYamlFileAsync<CreatureDb[]>(deserializer, fileName);
    }

    private async Task<T> LoadFromYamlFileAsync<T>(IDeserializer deserializer, string fileName)
    {
        try
        {
            var fileData = await fileSystem.File.ReadAllTextAsync(fileName, CancellationToken.None);
            var result = deserializer.Deserialize<T>(fileData);
            return result;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to load or deserialize file: {fileName}. See inner exception for details.", e);
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

    public sealed class CreatureDb
    {
        public required string Name { get; set; }
        public required uint Level { get; set; }
        public required string Type { get; set; }
        public required string Fraction { get; set; }
        public required uint Health { get; set; }
        public required uint Attack { get; set; }
        public required uint Defence { get; set; }
        public required uint MinDamage { get; set; }
        public required uint MaxDamage { get; set; }
        public required uint Initiative { get; set; }
        public required uint Speed { get; set; }
        public required int Morale { get; set; }
        public required int Luck { get; set; }
        public required string[] Abilities { get; set; }

        public Creature ToDomain(
            LangDict langDict,
            Dictionary<string, AbilityDb> abilitiesDb)
        {
            var abilities = abilitiesDb
                .ToDictionary(k => k.Key, a => a.Value.ToDomain(langDict));

            return new Creature(
                Name: LocalizeString(Name, langDict),
                Level,
                Type: LocalizeString(Type, langDict),
                Fraction: LocalizeString(Fraction, langDict),
                Health,
                Attack,
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

    public sealed class AbilityDb
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
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
