using System.Threading.Tasks;
using TinyYaml;
using NUnit.Framework;

namespace TinyYaml.UnitTests
{
	[TestFixture]
	public class TinyYamlGraphTests
	{
		public const string Yaml = @"Level1: Level1Value # Comment
	Level2
		Level3
Level1a
	Level2
	Level2a
Level1b
# NoNameOrValue
: NoNameOrComment
:
#";

		[Test]
		public async Task ParseYaml_CreatesCorrectTree()
		{
			var nodes = await TinyYamlGraph.YamlToNodes(Yaml);

			Assert.True(nodes.Count == 7);
			Assert.True(nodes[0].Nodes.Count == 1);
			Assert.True(nodes[1].Nodes.Count == 2);
			Assert.True(nodes[2].Nodes.Count == 0);
			Assert.True(nodes[0].Nodes[0].Nodes.Count == 1);

			Assert.True(nodes[0].Name.Equals("Level1"));
			Assert.True(nodes[0].Value.Equals("Level1Value"));
			Assert.True(nodes[0].Comment.Equals("Comment"));

			Assert.True(nodes[3].Name.Equals(string.Empty));
			Assert.True(nodes[3].Value.Equals(string.Empty));
			Assert.True(nodes[3].Comment.Equals("NoNameOrValue"));

			Assert.True(nodes[4].Name.Equals(string.Empty));
			Assert.True(nodes[4].Value.Equals("NoNameOrComment"));
			Assert.True(nodes[4].Comment.Equals(string.Empty));

			Assert.True(nodes[5].Name.Equals(string.Empty));
			Assert.True(nodes[5].Value.Equals(string.Empty));
			Assert.True(nodes[5].Comment.Equals(string.Empty));

			Assert.True(nodes[6].Name.Equals(string.Empty));
			Assert.True(nodes[6].Value.Equals(string.Empty));
			Assert.True(nodes[6].Comment.Equals(string.Empty));
		}

		[Test]
		public async Task ConstructYaml_MatchesInput()
		{
			var nodes = await TinyYamlGraph.YamlToNodes(Yaml);
			var yaml = await TinyYamlGraph.NodesToYaml(nodes);

			Assert.True(yaml == Yaml);
		}
	}
}
