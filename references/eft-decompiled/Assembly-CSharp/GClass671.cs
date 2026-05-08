public class GClass671
{
	public int Khorovod;

	public int ForceAttack;

	public int ForcePersuit;

	public int FollowPlayer;

	public int HalloweenHide;

	public int GoToGenerator;

	public GClass671(int priorityKhorovod, int priorityForceAttack, int priorityForcePersuit, int followPlayer, int halloweenHide = -1, int gotoGenerator = -1)
	{
		Khorovod = priorityKhorovod;
		ForceAttack = priorityForceAttack;
		ForcePersuit = priorityForcePersuit;
		FollowPlayer = followPlayer;
		HalloweenHide = halloweenHide;
		GoToGenerator = gotoGenerator;
	}
}
