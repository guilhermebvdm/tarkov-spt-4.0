using System;

namespace Diz.Binding;

public interface IBindableEvent
{
	Action Subscribe(Action handler);

	Action Bind(Action handler);
}
