using System;
using System.Text;

public class PropertyConfig
{
	public bool Ignore { get; set; } = false;
	public Encoding? Encoding { get; set; }
	public Func<object, object> Selector { get; set; }
}
