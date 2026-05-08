using System.Threading.Tasks;
using Diz.Binding;

public interface GInterface62<T> : IBindable<T>
{
	Task<T> Set(T t);
}
