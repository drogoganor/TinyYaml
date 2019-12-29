using System.Collections.Generic;

namespace TinyYaml.UnitTests
{
	internal class YamlTestObject
	{
		public string StringProperty { get; set; }
		public string StringField;
		public short ShortField = 0;
		public int IntField;
		public long LongField = 0;
		public float FloatField = 0;
		public double DoubleField = 0;
		public YamlTestObject ChildObject;
		public List<string> StringList;
	}
}
