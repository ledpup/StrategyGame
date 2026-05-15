namespace GameModel;

using System;
using System.Collections.Generic;

public class MilitaryUnitFactory(UnitTemplateFactory templateFactory)
{
    private readonly UnitTemplateFactory templateFactory = templateFactory;
    private readonly Dictionary<(UnitTemplateName, Guid OwnerId), int> sequenceCounters = [];
    private readonly Dictionary<int, Player> playersByOwnerIndex = [];

    public MilitaryUnit Create(UnitTemplateName templateName, Player owner, string unitName, Tile location = null, int turnBuilt = 0)
    {
        var template = templateFactory.Get(templateName);
        return new MilitaryUnit(template, owner, location: location, name: unitName, turnBuilt: turnBuilt);
    }

    public MilitaryUnit Create(UnitTemplateName templateName, string unitName, int ownerIndex = 0, Tile location = null, int turnBuilt = 0)
    {
        return Create(templateName, GetPlayer(ownerIndex), unitName, location, turnBuilt);
    }

    public MilitaryUnit CreateNext(UnitTemplateName templateName, Player owner, Tile location = null, int turnBuilt = 0)
    {
        if (!sequenceCounters.TryGetValue((templateName, owner.Id), out var count))
        {
            count = 0;
        }

        count++;
        sequenceCounters[(templateName, owner.Id)] = count;

        var ordinal = ToOrdinal(count);
        var unitName = $"{ordinal} {templateName.ToDisplayName()}";
        return Create(templateName, owner, unitName, location, turnBuilt);
    }

    public MilitaryUnit CreateNext(UnitTemplateName templateName, int ownerIndex = 0, Tile location = null, int turnBuilt = 0)
    {
        return CreateNext(templateName, GetPlayer(ownerIndex), location, turnBuilt);
    }

    private Player GetPlayer(int ownerIndex)
    {
        if (!playersByOwnerIndex.TryGetValue(ownerIndex, out var player))
        {
            var colour = Enum.IsDefined(typeof(PlayerColour), ownerIndex) ? (PlayerColour)ownerIndex : PlayerColour.Red;
            player = new Player(colour, colour.ToString());
            playersByOwnerIndex[ownerIndex] = player;
        }

        return player;
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
