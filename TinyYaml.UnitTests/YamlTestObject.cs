using System.Collections.Generic;

namespace TinyYaml.UnitTests
{
	internal class YamlTestObject
	{
		public string StringProperty { get; set; }
		public string StringField;
		public short ShortField;
		public int IntField;
		public long LongField;
		public float FloatField;
		public double DoubleField;
		public YamlTestObject ChildObject;
		public List<string> StringList;
	}
}
