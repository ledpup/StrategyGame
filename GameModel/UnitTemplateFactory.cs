using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameModel;

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
        Register(new UnitTemplate { Name = UnitTemplateName.DwarvenInfantry.ToDisplayName(), UnitType = UnitType.Melee, Personnel = 250 });
        Register(new UnitTemplate { Name = UnitTemplateName.DwarvenDragoons.ToDisplayName(), UnitType = UnitType.Melee, Personnel = 100, MovementPoints = 3 });
        Register(new UnitTemplate { Name = UnitTemplateName.DwarvenCrossbowmen.ToDisplayName(), UnitType = UnitType.Ranged,  Personnel = 150 });
    }

    public UnitTemplate Register(UnitTemplate template)
    {
        var key = Enum.Parse<UnitTemplateName>(template.Name.Replace(" ", ""));
        templates[key] = template;
        return template;
    }

    public UnitTemplate Get(UnitTemplateName name) => templates[name];

    public bool TryGet(UnitTemplateName name, out UnitTemplate template) => templates.TryGetValue(name, out template);
}
