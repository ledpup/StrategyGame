using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests;

[TestClass]
public class MilitaryUnitFactoryTests
{
    MilitaryUnitFactory unitFactory;

    [TestInitialize]
    public void Setup()
    {
        unitFactory = new MilitaryUnitFactory(new UnitTemplateFactory());
    }

    [TestMethod]
    public void CreateNext_FirstUnit_Uses1stOrdinal()
    {
        var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        Assert.AreEqual("1st Dwarven Infantry", unit.Name);
    }

    [TestMethod]
    public void CreateNext_SecondUnit_Uses2ndOrdinal()
    {
        unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        Assert.AreEqual("2nd Dwarven Infantry", unit.Name);
    }

    [TestMethod]
    public void CreateNext_ThirdUnit_Uses3rdOrdinal()
    {
        unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        Assert.AreEqual("3rd Dwarven Infantry", unit.Name);
    }

    [TestMethod]
    public void CreateNext_FourthUnit_Uses4thOrdinal()
    {
        for (var i = 0; i < 3; i++)
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        Assert.AreEqual("4th Dwarven Infantry", unit.Name);
    }

    [TestMethod]
    public void CreateNext_EleventhUnit_Uses11thOrdinal()
    {
        for (var i = 0; i < 10; i++)
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        Assert.AreEqual("11th Dwarven Infantry", unit.Name);
    }

    [TestMethod]
    public void CreateNext_TwelfthUnit_Uses12thOrdinal()
    {
        for (var i = 0; i < 11; i++)
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        Assert.AreEqual("12th Dwarven Infantry", unit.Name);
    }

    [TestMethod]
    public void CreateNext_TwentyFirstUnit_Uses21stOrdinal()
    {
        for (var i = 0; i < 20; i++)
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        Assert.AreEqual("21st Dwarven Infantry", unit.Name);
    }

    [TestMethod]
    public void CreateNext_DifferentOwners_RestartsSequencePerOwner()
    {
        var owner0Unit1 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 0);
        var owner1Unit1 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 1);
        var owner0Unit2 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 0);

        Assert.AreEqual("1st Dwarven Infantry", owner0Unit1.Name);
        Assert.AreEqual("1st Dwarven Infantry", owner1Unit1.Name);
        Assert.AreEqual("2nd Dwarven Infantry", owner0Unit2.Name);
    }

    [TestMethod]
    public void CreateNext_DifferentTemplates_UsesIndependentSequences()
    {
        var infantry = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
        var crossbow = unitFactory.CreateNext(UnitTemplateName.DwarvenCrossbowmen);
        var infantry2 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);

        Assert.AreEqual("1st Dwarven Infantry", infantry.Name);
        Assert.AreEqual("1st Dwarven Crossbowmen", crossbow.Name);
        Assert.AreEqual("2nd Dwarven Infantry", infantry2.Name);
    }

    [TestMethod]
    public void Create_ExplicitName_UsesProvidedName()
    {
        var unit = unitFactory.Create(UnitTemplateName.DwarvenInfantry, "King's Guard");
        Assert.AreEqual("King's Guard", unit.Name);
    }

    [TestMethod]
    public void ToDisplayName_TemplateNames_ReturnDisplayNames()
    {
        Assert.AreEqual("Dwarven Infantry", UnitTemplateName.DwarvenInfantry.ToDisplayName());
        Assert.AreEqual("Dwarven Dragoons", UnitTemplateName.DwarvenDragoons.ToDisplayName());
        Assert.AreEqual("Dwarven Crossbowmen", UnitTemplateName.DwarvenCrossbowmen.ToDisplayName());
    }
}


