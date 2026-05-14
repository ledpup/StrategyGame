namespace GameModel;

using System.Collections.Generic;

public class MilitaryUnitFactory(UnitTemplateFactory templateFactory)
{
    private readonly UnitTemplateFactory templateFactory = templateFactory;
    private readonly Dictionary<(UnitTemplateName, int ownerId), int> sequenceCounters = [];

    public MilitaryUnit Create(UnitTemplateName templateName, int id, Player owner, string unitName, Tile location = null, int turnBuilt = 0)
    {
        var template = templateFactory.Get(templateName);
        return new MilitaryUnit(template, id, owner, location: location, name: unitName, turnBuilt: turnBuilt);
    }

    public MilitaryUnit CreateNext(UnitTemplateName templateName, int id, Player owner, Tile location = null, int turnBuilt = 0)
    {
        if (!sequenceCounters.TryGetValue((templateName, owner.Id), out var count))
        {
            count = 0;
        }

        count++;
        sequenceCounters[(templateName, owner.Id)] = count;

        var ordinal = ToOrdinal(count);
        var unitName = $"{ordinal} {templateName.ToDisplayName()}";
        return Create(templateName, id, owner, unitName, location, turnBuilt);
    }

    private static string ToOrdinal(int number)
    {
        var suffix = (number % 100) switch
        {
            11 or 12 or 13 => "th",
            _ => (number % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th",
            },
        };
        return $"{number}{suffix}";
    }
}
