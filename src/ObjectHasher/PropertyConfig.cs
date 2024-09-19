using System;
using System.Text;

public class PropertyConfig<T> where T : class
{
	public bool Ignore { get; set; } = false;
	public Encoding? Encoding { get; set; }
	public Func<T, object> Selector { get; set; }
}
