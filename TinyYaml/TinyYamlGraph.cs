using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyYaml
{
	public static class TinyYamlGraph
	{
		public static async Task<List<TinyYamlNode>> YamlToNodes(string yaml)
		{
			var lines = yaml.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			return await LinesToNodes(lines);
		}

		public static async Task<List<TinyYamlNode>> FileToNodes(string filename)
		{
			var lines = await File.ReadAllLinesAsync(filename);
			return await LinesToNodes(lines);
		}

		/// <summary>
		/// Convert a file to yaml nodes.
		/// Parses the file line by line.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		private static async Task<List<TinyYamlNode>> LinesToNodes(string[] lines)
		{
			var rootNode = new TinyYamlNode(-1, string.Empty, isRootNode: true);
			var nodeList = new List<TinyYamlNode>();

			// First assemble a list of nodes for each line
			for (int lineNum = 0; lineNum < lines.Length; lineNum++)
			{
				var line = lines[lineNum];
				if (string.IsNullOrWhiteSpace(line)) continue;

				nodeList.Add(new TinyYamlNode(lineNum, line.TrimEnd()));
			}

			// Assemble the tree
			var assemble = Task.Run(() =>
			{
				var stack = new Stack<TinyYamlNode>(new[] { rootNode });
				foreach (var node in nodeList)
				{
					var currentParent = stack.Peek();
					var indentDiff = node.Indent - currentParent.Indent;
					if (indentDiff > 1)
						throw new Exception($"Indenting was invalid on line {node.LineNumber}: {node.Text}");
					if (indentDiff == 0)
					{
						stack.Pop();
						currentParent = stack.Peek();
						currentParent.Nodes.Add(node);
					}
					else if (indentDiff > 0)
					{
						currentParent.Nodes.Add(node);
					}
					else
					{
						for (int i = 0; i <= Math.Abs(indentDiff); i++)
							stack.Pop();

						currentParent = stack.Peek();
						currentParent.Nodes.Add(node);
					}

					stack.Push(node);
				}
			});

			// Parse values
			var parse = Task.Run(() =>
			{
				foreach (var node in nodeList)
				{
					var lineSpan = node.Text.Trim().AsSpan();
					var contentSpan = lineSpan;
					var nameSpan = lineSpan;
					ReadOnlySpan<char> commentSpan;
					ReadOnlySpan<char> valueSpan;

					var commentIndex = lineSpan.IndexOf('#');
					if (commentIndex >= 0)
					{
						contentSpan = lineSpan.Slice(0, commentIndex);
						commentSpan = lineSpan.Slice(commentIndex + 1);
						node.Comment = commentSpan.ToString().Trim();
					}
					else
					{
						node.Comment = string.Empty;
					}

					var colonIndex = contentSpan.IndexOf(':');
					if (colonIndex >= 0)
					{
						nameSpan = contentSpan.Slice(0, colonIndex);
						valueSpan = contentSpan.Slice(colonIndex + 1);
						node.Name = nameSpan.ToString().Trim();
						node.Value = valueSpan.ToString().Trim();
					}
					else
					{
						node.Name = contentSpan.ToString().Trim();
						node.Value = string.Empty;
					}
				}
			});

			await Task.WhenAll(new[] { assemble, parse });

			return rootNode.Nodes;
		}

		/// <summary>
		/// Convert a list of yaml nodes to yaml string output
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static async Task<string> NodesToYaml(List<TinyYamlNode> nodes)
		{
			var lineList = new List<string>();
			foreach (var n in nodes)
			{
				lineList.AddRange(await NodeToYamlLinesRecursive(n, 0));
			}

			var result = String.Join("\r\n", lineList);
			return result;
		}

		static async Task<List<string>> NodeToYamlLinesRecursive(TinyYamlNode node, int level)
		{
			var lineList = new List<string>();
			var sb = new StringBuilder();
			sb.Append(string.Concat(Enumerable.Repeat("\t", level)));
			sb.Append(node.Name);
			if (!string.IsNullOrWhiteSpace(node.Value))
			{
				sb.Append(": ");
				sb.Append(node.Value);
			}

			if (!string.IsNullOrWhiteSpace(node.Comment))
			{
				sb.Append(" # ");
				sb.Append(node.Comment);
			}

			lineList.Add(node.Text);
			foreach (var child in node.Nodes)
			{
				lineList.AddRange(await NodeToYamlLinesRecursive(child, level + 1));
			}

			return lineList;
		}


		/*
		// Convert an object to TinyYaml string
		public static string ToYaml<T>(this T obj)
		{
			var nodes = ObjectToNodes(obj);
			return NodesToYaml(nodes);
		}

		// Write an object to TinyYaml file
		public static void ToYamlFile<T>(this T obj, string filename)
		{
			var yaml = ToYaml(obj);

			if (string.IsNullOrWhiteSpace(yaml))
				return;

			try
			{
				File.WriteAllText(filename, yaml);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	
		// Read a Yaml file and create an object
		public static T FromYamlFile<T>(string filename) where T : new()
		{
			var nodes = FileToNodes(filename);
			var result = new T();
			var type = typeof(T);

			if (IsDictionary(type))
			{
				return (T)NodesToDictionary(nodes, type.GenericTypeArguments[0], type.GenericTypeArguments[1]);
			}
			else if (IsList(type))
			{
				return (T)NodesToList(nodes, type.GenericTypeArguments[0]);
			}
			
			PopulateObject(result, nodes);
			return result;
		}

		// Read a Yaml file and create an object
		public static IList FromYamlFile(string filename, Type t)
		{
			var nodes = FileToNodes(filename);
			return (IList)NodesToList(nodes, t);
		}
		*/

	}
}
