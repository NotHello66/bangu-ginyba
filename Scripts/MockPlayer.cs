using Vector3 = System.Numerics.Vector3;

public partial class MockPlayer
{
	public Vector3 GlobalPosition { get; set; }

	public MockEnemy GetClosestEnemy(MockEnemy[] enemies)
	{
		MockEnemy closest = null;
		float closestDistance = float.MaxValue;

		foreach (var enemy in enemies)
		{
			if (!enemy.HealthModel.isDead)
			{
				float distance = DistanceTo(GlobalPosition, enemy.GlobalPosition);

				if (distance < closestDistance)
				{
					closestDistance = distance;
					closest = enemy;
				}
			}
		}

		if (closest == null) return null;

		return closest;
	}
	public static float DistanceTo(Vector3 a, Vector3 b)
	{
		return Vector3.Distance(a, b);
	}
}
