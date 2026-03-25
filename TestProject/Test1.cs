namespace TestProject;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vector3 = System.Numerics.Vector3;

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
	//[TestMethod]
	//public void Test_GetClosestEnemy_ReturnsClosestAliveEnemy()
	//{
	//	// 1. Arrange: Set up the player and the test conditions
	//	MockPlayer player = new MockPlayer();
	//	player.GlobalPosition = new Vector3(0, 0, 0); // Player is at the origin

	//	// Create a dead enemy that is very close (should be ignored)
	//	HealthModel deadHealth = new HealthModel { isDead = true };
	//	MockEnemy deadCloseEnemy = new MockEnemy(deadHealth, new Vector3(1, 0, 0));

	//	// Create an alive enemy at a medium distance (This should be the target)
	//	HealthModel aliveHealth1 = new HealthModel { isDead = false };
	//	MockEnemy aliveMidEnemy = new MockEnemy(aliveHealth1, new Vector3(5, 0, 0));

	//	// Create an alive enemy far away
	//	HealthModel aliveHealth2 = new HealthModel { isDead = false };
	//	MockEnemy aliveFarEnemy = new MockEnemy(aliveHealth2, new Vector3(10, 0, 0));

	//	// Put them in an array
	//	MockEnemy[] enemies = new MockEnemy[] { deadCloseEnemy, aliveMidEnemy, aliveFarEnemy };

	//	// 2. Act: Call the method being tested
	//	MockEnemy closest = player.GetClosestEnemy(enemies);

	//	// 3. Assert: Verify the results
	//	Assert.IsNotNull(closest, "The method should have found an enemy.");
	//	Assert.AreSame(aliveMidEnemy, closest, "The method did not return the closest ALIVE enemy.");
	//}

	//[TestMethod]
	//public void Test_GetClosestEnemy_ReturnsNullWhenAllDead()
	//{
	//	// Arrange
	//	MockPlayer player = new MockPlayer { GlobalPosition = new Vector3(0, 0, 0) };

	//	HealthModel deadHealth = new HealthModel { isDead = true };
	//	MockEnemy[] enemies = new MockEnemy[]
	//	{
	//			new MockEnemy(deadHealth, new Vector3(2, 0, 0)),
	//			new MockEnemy(deadHealth, new Vector3(5, 0, 0))
	//	};

	//	// Act
	//	MockEnemy closest = player.GetClosestEnemy(enemies);

	//	// Assert
	//	Assert.IsNull(closest, "The method should return null if all enemies are dead.");
	//}
}
