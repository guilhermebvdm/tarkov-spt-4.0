using System.Threading.Tasks;

public interface GInterface43
{
	Task LoadSummer();

	Task LoadWinter();

	Task LoadSpring();

	Task LoadSpringEarly();

	Task LoadAutumn();

	Task LoadAutumnLate();

	void Fix();

	void Unload();
}
