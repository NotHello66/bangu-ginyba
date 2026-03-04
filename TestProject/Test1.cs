namespace TestProject;

using Godot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Intrinsics.X86;
using twodog;

[TestClass]
public class Test1
{
    [TestMethod]
    public void test_enemyDeath()
    {
        // Example change inside test (run while Godot engine/SceneTree is running)
        HealthModel hm = new HealthModel();
        hm.Instantiate();
        hm.Damage(10000f);
        Assert.IsTrue(hm.isDead);
    }
    [TestMethod]
    public void test_healthDamged()
    {
        HealthModel hm = new HealthModel();
        hm.Instantiate();
        hm.Damage(10f);
        Assert.AreEqual(90, hm.HP);
    }
}