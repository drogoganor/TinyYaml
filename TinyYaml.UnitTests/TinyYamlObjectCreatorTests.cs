using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TinyYaml;
using NUnit.Framework;

namespace TinyYaml.UnitTests
{
    [TestFixture]
    public class TinyYamlObjectCreatorTests
    {
        private readonly TinyYamlObjectCreator ObjectCreator;

        private const string TestObjectYaml = @"
StringField: string
IntField: 1
StringList: 1, 2, 3
ChildObject:
	StringField: string2
	IntField: 2";

        private const string ValidYaml = @":
	StringField: string
	IntField: 1
:
	StringField: string2
	IntField: 2";

        private readonly YamlTestObject TestObject = new YamlTestObject
        {
            StringField = "string",
            IntField = 1,
            StringList = new List<string>() { "1", "2", "3" },
            ChildObject = new YamlTestObject
            {
                StringField = "string2",
                IntField = 2
            }
        };


        public TinyYamlObjectCreatorTests()
		{
			ObjectCreator = new TinyYamlObjectCreator(Assembly.GetExecutingAssembly());
		}

		[SetUp]
		public void SetUp()
		{
        }

        [Test]
        public async Task CreateNodesFromObject_IsPopulatedCorrectly()
        {
            var node = await ObjectCreator.ObjectToNodes(TestObject);

            Assert.AreEqual(node.Nodes.First(n => n.Name == "StringField").Value, "string");
            Assert.AreEqual(node.Nodes.First(n => n.Name == "IntField").Value, "1");
            Assert.AreEqual(node.Nodes.First(n => n.Name == "StringList").Value, "1, 2, 3");
        }

        [Test]
		public async Task CreateNodesFromYaml_IsPopulatedCorrectly()
        {
            var nodes = await TinyYamlGraph.YamlToNodes(TestObjectYaml);

            Assert.AreEqual(nodes.First(n => n.Name == "StringField").Value, "string");
            Assert.AreEqual(nodes.First(n => n.Name == "IntField").Value, "1");
            Assert.AreEqual(nodes.First(n => n.Name == "StringList").Value, "1, 2, 3");
        }

        [Test]
        public async Task CreateObjectFromYaml_IsPopulatedCorrectly()
        {
            var nodes = await TinyYamlGraph.YamlToNodes(TestObjectYaml);
            var obj = new YamlTestObject();
            await ObjectCreator.NodesToObject(nodes, obj);

            Assert.AreEqual(obj.StringField, TestObject.StringField);
            Assert.AreEqual(obj.IntField, TestObject.IntField);
            Assert.AreEqual(obj.StringList, TestObject.StringList);
            Assert.AreEqual(obj.ChildObject.StringField, TestObject.ChildObject.StringField);
            Assert.AreEqual(obj.ChildObject.IntField, TestObject.ChildObject.IntField);
        }
    }
}
