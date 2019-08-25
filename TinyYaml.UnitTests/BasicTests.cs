using NUnit.Framework;
using TinyYaml;

namespace TinyYaml.UnitTests
{
    [TestFixture]
    public class BasicTests
    {
        [Test]
        public void ObjectToYaml_ReturnsMatch()
        {
            var testObject = new TestObject()
            {
                ID = 5,
                Name = "Test"
            };

            var matchingYaml = @"ID: 5
Name: Test";

            var yaml = Yaml.ToYaml(testObject);

            Assert.AreEqual(yaml, matchingYaml);
        }
    }
}
