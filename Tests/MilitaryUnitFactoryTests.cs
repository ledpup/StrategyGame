using GameModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
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
        public void OrdinalNaming_FirstUnit_Is1st()
        {
            var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            Assert.AreEqual("1st Dwarven Infantry", unit.Name);
        }

        [TestMethod]
        public void OrdinalNaming_SecondUnit_Is2nd()
        {
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            Assert.AreEqual("2nd Dwarven Infantry", unit.Name);
        }

        [TestMethod]
        public void OrdinalNaming_ThirdUnit_Is3rd()
        {
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            Assert.AreEqual("3rd Dwarven Infantry", unit.Name);
        }

        [TestMethod]
        public void OrdinalNaming_FourthUnit_Is4th()
        {
            for (var i = 0; i < 3; i++)
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            Assert.AreEqual("4th Dwarven Infantry", unit.Name);
        }

        [TestMethod]
        public void OrdinalNaming_EleventhUnit_Is11th()
        {
            for (var i = 0; i < 10; i++)
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            Assert.AreEqual("11th Dwarven Infantry", unit.Name);
        }

        [TestMethod]
        public void OrdinalNaming_TwelfthUnit_Is12th()
        {
            for (var i = 0; i < 11; i++)
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            Assert.AreEqual("12th Dwarven Infantry", unit.Name);
        }

        [TestMethod]
        public void OrdinalNaming_TwentyFirstUnit_Is21st()
        {
            for (var i = 0; i < 20; i++)
                unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            var unit = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            Assert.AreEqual("21st Dwarven Infantry", unit.Name);
        }

        [TestMethod]
        public void SequenceRestartsPerOwner()
        {
            var owner0Unit1 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 0);
            var owner1Unit1 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 1);
            var owner0Unit2 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry, ownerIndex: 0);

            Assert.AreEqual("1st Dwarven Infantry", owner0Unit1.Name);
            Assert.AreEqual("1st Dwarven Infantry", owner1Unit1.Name);
            Assert.AreEqual("2nd Dwarven Infantry", owner0Unit2.Name);
        }

        [TestMethod]
        public void SequenceIsIndependentPerTemplate()
        {
            var infantry  = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);
            var crossbow  = unitFactory.CreateNext(UnitTemplateName.DwarvenCrossbowmen);
            var infantry2 = unitFactory.CreateNext(UnitTemplateName.DwarvenInfantry);

            Assert.AreEqual("1st Dwarven Infantry", infantry.Name);
            Assert.AreEqual("1st Dwarven Crossbowmen", crossbow.Name);
            Assert.AreEqual("2nd Dwarven Infantry", infantry2.Name);
        }

        [TestMethod]
        public void CreateWithExplicitName()
        {
            var unit = unitFactory.Create(UnitTemplateName.DwarvenInfantry, "King's Guard");
            Assert.AreEqual("King's Guard", unit.Name);
        }

        [TestMethod]
        public void DisplayNames()
        {
            Assert.AreEqual("Dwarven Infantry",      UnitTemplateName.DwarvenInfantry.ToDisplayName());
            Assert.AreEqual("Dwarven Dragoons",      UnitTemplateName.DwarvenDragoons.ToDisplayName());
            Assert.AreEqual("Dwarven Crossbowmen",   UnitTemplateName.DwarvenCrossbowmen.ToDisplayName());
        }
    }
}
