using System.Collections.Generic;
using NUnit.Framework;
using Project.Data;
using UnityEngine;

namespace Project.Tests
{
    public sealed class UpgradeRollerTests
    {
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        UpgradeDefinition Make(string id, UpgradeRarity rarity)
        {
            var u = ScriptableObject.CreateInstance<UpgradeDefinition>();
            u.Id = id;
            u.Rarity = rarity;
            _created.Add(u);
            return u;
        }

        [Test]
        public void Roll_ReturnsDistinctOptionsUpToCount()
        {
            var pool = new List<UpgradeDefinition>
            {
                Make("a", UpgradeRarity.Common), Make("b", UpgradeRarity.Rare), Make("c", UpgradeRarity.Epic), Make("d", UpgradeRarity.Common)
            };
            var results = new List<UpgradeDefinition>();

            UpgradeRoller.Roll(pool, _ => true, 3, new System.Random(1), results);

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(3, new HashSet<UpgradeDefinition>(results).Count);
        }

        [Test]
        public void Roll_SkipsUnavailableUpgrades()
        {
            UpgradeDefinition blocked = Make("blocked", UpgradeRarity.Epic);
            var pool = new List<UpgradeDefinition> { blocked, Make("ok", UpgradeRarity.Common) };
            var results = new List<UpgradeDefinition>();

            UpgradeRoller.Roll(pool, u => u != blocked, 3, new System.Random(2), results);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("ok", results[0].Id);
        }

        [Test]
        public void Roll_EmptyPoolYieldsNoResults()
        {
            var results = new List<UpgradeDefinition>();

            UpgradeRoller.Roll(new List<UpgradeDefinition>(), _ => true, 3, new System.Random(3), results);

            Assert.AreEqual(0, results.Count);
        }
    }
}
