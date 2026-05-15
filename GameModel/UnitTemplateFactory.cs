namespace GameModel;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum UnitTemplateName
{
    DwarvenInfantry,
    DwarvenDragoons,
    DwarvenCrossbowmen,
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
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.DwarvenInfantry, UnitType = UnitType.Melee, Personnel = 250, TransportableBy = [OperationalDomain.Waterbound, OperationalDomain.Airborne] });
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.DwarvenDragoons, UnitType = UnitType.Melee, Personnel = 100, MovementPoints = 3, TransportableBy = [OperationalDomain.Waterbound] });
        Register(new UnitTemplate { UnitTemplateName = UnitTemplateName.DwarvenCrossbowmen, UnitType = UnitType.Ranged, Personnel = 150, TransportableBy = [OperationalDomain.Waterbound, OperationalDomain.Airborne] });
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
