namespace OldenEraHome.Wiki.WebApi.Models;

public sealed record Creature(
    string Name,
    uint Level,
    string Type,
    string Fraction,
    uint Health,
    uint Attack,
    uint Defence,
    uint MinDamage,
    uint MaxDamage,
    uint Initiative,
    uint Speed,
    int Morale,
    int Luck,
    Ability[] Abilities
);
