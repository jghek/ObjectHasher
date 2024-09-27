using System;
using System.Text;

namespace ObjectHasher;

public class PropertyConfig
{
	public bool Ignore { get; set; } = false;
	public Encoding? Encoding { get; set; }
	public Func<object, object> ObjectSelector { get; protected set; }
}

public class PropertyConfig<T> : PropertyConfig
{
	Func<T, object> _selector;

	public Func<T, object> Selector
	{
		get => _selector;
		set
		{
			_selector = value;
			ObjectSelector = o => Selector((T)o);
		}
	}

}
