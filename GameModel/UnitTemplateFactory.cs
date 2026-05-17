namespace GameModel;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum UnitTemplateName
{
    DwarvenInfantry,
    DwarvenDragoons,
    DwarvenCrossbowmen,
    MountainFolk,
    MarshFolk,
    DesertFolk,
    AirborneScouts,
    RiverBarge,
    ReefSkimmer,
}

public static class UnitTemplateNameExtensions
{
    public static string ToDisplayName(this UnitTemplateName name)
    {
        return Regex.Replace(name.ToString(), "(\\B[A-Z])", " $1");
    }
}

public class UnitTemplateFactory
{
    private readonly Dictionary<UnitTemplateName, UnitTemplate> templates = [];

    public UnitTemplateFactory()
    {
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.DwarvenInfantry, UnitType = UnitType.Melee, Personnel = 250, RoadMovementBonus = 1, TransportableBy = [OperationalDomain.Waterbound, OperationalDomain.Airborne] });
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.DwarvenDragoons, UnitType = UnitType.Melee, Personnel = 100, MovementPoints = 3, RoadMovementBonus = 2, TransportableBy = [OperationalDomain.Waterbound] });
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.DwarvenCrossbowmen, UnitType = UnitType.Ranged, Personnel = 150, RoadMovementBonus = 0, TransportableBy = [OperationalDomain.Waterbound, OperationalDomain.Airborne] });
        Register(new UnitTemplate
        {
            UnitTemplateName = UnitTemplateName.MountainFolk,
            UnitType = UnitType.Melee,
            MovementPoints = 3,
            RoadMovementBonus = 1,
            AdditionalCanStopOn = TerrainType.Mountain,
            TerrainMovementCostOverrides = new Dictionary<TerrainType, int>
            {
                [TerrainType.Hill] = 1,
                [TerrainType.Mountain] = 1,
            },
            EdgeMovementCostOverrides = new Dictionary<EdgeType, int>
            {
                [EdgeType.Mountain] = 1,
            },
            TransportableBy = [OperationalDomain.Waterbound, OperationalDomain.Airborne],
        });
        Register(new UnitTemplate
        {
            UnitTemplateName = UnitTemplateName.MarshFolk,
            UnitType = UnitType.Melee,
            MovementPoints = 3,
            RoadMovementBonus = 1,
            TerrainMovementCostOverrides = new Dictionary<TerrainType, int>
            {
                [TerrainType.Swamp] = 1,
            },
            EdgeMovementCostOverrides = new Dictionary<EdgeType, int>
            {
                [EdgeType.River] = 1,
            },
            TransportableBy = [OperationalDomain.Waterbound, OperationalDomain.Airborne],
        });
        Register(new UnitTemplate
        {
            UnitTemplateName = UnitTemplateName.DesertFolk,
            UnitType = UnitType.Ranged,
            MovementPoints = 4,
            TerrainMovementCostOverrides = new Dictionary<TerrainType, int>
            {
                [TerrainType.Desert] = 1,
            },
            TransportableBy = [OperationalDomain.Waterbound, OperationalDomain.Airborne],
        });
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.AirborneScouts, OperationalDomain = OperationalDomain.Airborne, UnitType = UnitType.Ranged, MovementPoints = 4, Personnel = 80 });
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.RiverBarge, OperationalDomain = OperationalDomain.Waterbound, MovementPoints = 3, IsTransporter = true, Personnel = 50 });
        Register(new UnitTemplate
        {
            UnitTemplateName = UnitTemplateName.ReefSkimmer,
            OperationalDomain = OperationalDomain.Waterbound,
            MovementPoints = 4,
            Personnel = 40,
            TerrainMovementCostOverrides = new Dictionary<TerrainType, int>
            {
                [TerrainType.Reef] = 1,
            },
            EdgeMovementCostOverrides = new Dictionary<EdgeType, int>
            {
                [EdgeType.Reef] = 0,
            },
        });
    }

    public UnitTemplate Register(UnitTemplate template)
    {
        var key = Enum.Parse<UnitTemplateName>(template.Name.Replace(" ", string.Empty));
        templates[key] = template;
        return template;
    }

    public UnitTemplate Get(UnitTemplateName name) => templates[name];

    public bool TryGet(UnitTemplateName name, out UnitTemplate template) => templates.TryGetValue(name, out template);
}
