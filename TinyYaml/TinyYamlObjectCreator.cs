using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TinyYaml
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YamlIgnoreAttribute : Attribute
    {
        public YamlIgnoreAttribute() { }
    }

    public class TinyYamlObjectCreator
    {
		private readonly Assembly DataAssembly;
        private readonly YamlParsers Parsers;

        public TinyYamlObjectCreator(Assembly assembly)
		{
			DataAssembly = assembly;
            Parsers = new YamlParsers();
        }

        // Convert an object to yaml nodes
        public async Task<TinyYamlNode> ObjectToNodes(object obj)
        {
            if (obj == null)
                return null;

            var result = new TinyYamlNode(-1, string.Empty, isRootNode: true);

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var dataTypes = DataAssembly.DefinedTypes;

            var taskList = new List<Func<Task<TinyYamlNode>>>();

            foreach (var f in fields)
            {
                if (Attribute.IsDefined(f, typeof(YamlIgnoreAttribute)))
                    continue;

                taskList.Add(() => CreateNode(f.Name, f.FieldType, f.GetValue(obj)));
            }
            foreach (var f in properties)
            {
                if (Attribute.IsDefined(f, typeof(YamlIgnoreAttribute)))
                    continue;

                if (!f.CanWrite)
                    continue;

                taskList.Add(() => CreateNode(f.Name, f.PropertyType, f.GetValue(obj)));
            }

            await Task.WhenAll(taskList.Select(async func => result.Nodes.Add(await func())));

            return result;
        }

        private async Task<TinyYamlNode> CreateNode(string name, Type type, object? value)
        {
            var node = new TinyYamlNode()
            {
                Name = name,
            };

            if (value == null)
                return node;

            var parser = Parsers.Get(type);
            if (parser != null)
            {
                parser.Populate(node, value);
                return node;
            }
            else
            {
                return await ObjectToNodes(value);
            }
        }

        public async Task NodesToObject(List<TinyYamlNode> nodes, object obj)
        {
            if (nodes == null)
                return;

            if (obj == null)
                return;

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            var taskList = new List<Func<Task>>();

            foreach (var node in nodes)
            {
                var matchingProperty = properties.FirstOrDefault(x => x.Name == node.Name);
                if (matchingProperty != null && matchingProperty.CanWrite)
                {
                    if (Attribute.IsDefined(matchingProperty, typeof(YamlIgnoreAttribute)))
                        continue;

                    if (!matchingProperty.CanWrite)
                        continue;

                    taskList.Add(async () => matchingProperty.SetValue(obj, await PopulateObject(node, matchingProperty.PropertyType)));
                }

                var matchingField = fields.FirstOrDefault(x => x.Name == node.Name);
                if (matchingField != null)
                {
                    if (Attribute.IsDefined(matchingField, typeof(YamlIgnoreAttribute)))
                        continue;

                    taskList.Add(async () => matchingField.SetValue(obj, await PopulateObject(node, matchingField.FieldType)));
                }
            }

            await Task.WhenAll(taskList.Select(async func => await func()));
        }

        private async Task<object> PopulateObject(TinyYamlNode node, Type type)
        {
            var dataTypes = DataAssembly.DefinedTypes;

            if (node == null)
                return null;

            var parser = Parsers.Get(type);
            if (parser != null)
            {
                return parser.Parse(node);
            }
            else
            {
                // It might be a type from DEngine.Data
                if (dataTypes.Any(x => x.Name == type.Name))
                {
                    var newObj = Activator.CreateInstance(type);
                    await NodesToObject(node.Nodes, newObj);
                    return newObj;
                }
            }

            return null;
        }

        private bool IsDictionary(Type type) { return type.Name.StartsWith("Dictionary") || type.Name.StartsWith("IDictionary"); }

        private bool IsList(Type type) { return type.Name.StartsWith("List") || type.Name.StartsWith("IList"); }
    }
}
