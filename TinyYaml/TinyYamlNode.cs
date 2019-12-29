using System.Collections.Generic;
using System.Linq;

namespace TinyYaml
{
	public class TinyYamlNode
	{
		public readonly int LineNumber;
		public readonly string Text;
		public readonly int Indent;

		public List<TinyYamlNode> Nodes = new List<TinyYamlNode>();

		public string Name;
		public string Value;
		public string Comment;

		public TinyYamlNode() { }

		public TinyYamlNode(int lineNumber, string text, bool isRootNode = false)
		{
			LineNumber = lineNumber;
			Text = text;
			if (isRootNode)
				Indent = -1;
			else
				Indent = text.Count(chr => chr == '\t');
		}
	}
}
