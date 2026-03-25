using System;

public partial class HealthModel
{
	public float HP { get; private set; }
	public bool isDead = false;
	public void Instantiate()
	{
			HP = 100f;
	}
	public void Damage(float damage)
	{
		HP -= damage;
		if (HP <= 0)
		{
			isDead = true;
		}
	}


}
