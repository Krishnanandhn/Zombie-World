using NUnit.Framework;
using Project.Data;

namespace Project.Tests
{
    public sealed class StatSheetTests
    {
        [Test]
        public void Get_AppliesFlatThenPercent()
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.MaxHealth, 100f);
            sheet.Add(new StatModifier(StatType.MaxHealth, ModifierOp.Flat, 25f));
            sheet.Add(new StatModifier(StatType.MaxHealth, ModifierOp.Percent, 0.5f));

            Assert.AreEqual(187.5f, sheet.Get(StatType.MaxHealth), 0.001f);
        }

        [Test]
        public void Get_PercentModifiersStackAdditively()
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.Damage, 1f);
            sheet.Add(new StatModifier(StatType.Damage, ModifierOp.Percent, 0.15f));
            sheet.Add(new StatModifier(StatType.Damage, ModifierOp.Percent, 0.15f));

            Assert.AreEqual(1.3f, sheet.Get(StatType.Damage), 0.001f);
        }

        [Test]
        public void Get_NeverReturnsNegative()
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.DashCooldown, 1f);
            sheet.Add(new StatModifier(StatType.DashCooldown, ModifierOp.Percent, -2f));

            Assert.AreEqual(0f, sheet.Get(StatType.DashCooldown));
        }

        [Test]
        public void ClearModifiers_RestoresBaseValues()
        {
            var sheet = new StatSheet();
            sheet.SetBase(StatType.MoveSpeed, 6f);
            sheet.Add(new StatModifier(StatType.MoveSpeed, ModifierOp.Percent, 0.5f));
            sheet.ClearModifiers();

            Assert.AreEqual(6f, sheet.Get(StatType.MoveSpeed), 0.001f);
        }
    }
}
