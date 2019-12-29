using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyYaml
{
    public interface IYamlTypeParser
    {
        Type Type { get; }
    }

    public abstract class YamlTypeParser<T> : IYamlTypeParser
    {
        public Type Type { get { return typeof(T); } }
        public abstract T Parse(TinyYamlNode node);
        public abstract void Populate(TinyYamlNode node, object obj);
    }

    public class YamlParsers
    {
        private Dictionary<Type, dynamic> Dictionary = new Dictionary<Type, dynamic>();

        public YamlParsers()
        {
            var parsers = new IYamlTypeParser[]
            {
                new YamlStringParser(),
                new YamlShortParser(),
                new YamlLongParser(),
                new YamlIntParser(),
                new YamlFloatParser(),
                new YamlDoubleParser(),
                new YamlListStringParser(),
            };

            Initialize(parsers);
        }

        public YamlParsers(IYamlTypeParser[] parsers)
        {
            Initialize(parsers);
        }

        private void Initialize(IYamlTypeParser[] parsers)
        {
            foreach (var parser in parsers)
            {
                Dictionary.Add(parser.Type, parser);
            }
        }

        public dynamic Get(Type type)
        {
            if (!Dictionary.ContainsKey(type))
                return null;

            return Dictionary[type];
        }
    }

	public class YamlStringParser : YamlTypeParser<string>
	{
		public override string Parse(TinyYamlNode node)
		{
			return node.Value;
        }

        public override void Populate(TinyYamlNode node, object obj)
        {
            node.Value = obj.ToString();
        }
    }

    public class YamlIntParser : YamlTypeParser<int>
    {
        public override int Parse(TinyYamlNode node)
        {
            return Convert.ToInt32(node.Value);
        }

        public override void Populate(TinyYamlNode node, object obj)
        {
            node.Value = obj.ToString();
        }
    }

    public class YamlShortParser : YamlTypeParser<short>
    {
        public override short Parse(TinyYamlNode node)
        {
            return Convert.ToInt16(node.Value);
        }

        public override void Populate(TinyYamlNode node, object obj)
        {
            node.Value = obj.ToString();
        }
    }

    public class YamlLongParser : YamlTypeParser<long>
    {
        public override long Parse(TinyYamlNode node)
        {
            return Convert.ToInt64(node.Value);
        }

        public override void Populate(TinyYamlNode node, object obj)
        {
            node.Value = obj.ToString();
        }
    }

    public class YamlFloatParser : YamlTypeParser<float>
    {
        public override float Parse(TinyYamlNode node)
        {
            return Convert.ToSingle(node.Value);
        }

        public override void Populate(TinyYamlNode node, object obj)
        {
            node.Value = obj.ToString();
        }
    }

    public class YamlDoubleParser : YamlTypeParser<double>
    {
        public override double Parse(TinyYamlNode node)
        {
            return Convert.ToSingle(node.Value);
        }

        public override void Populate(TinyYamlNode node, object obj)
        {
            node.Value = obj.ToString();
        }
    }

    public class YamlListStringParser : YamlTypeParser<List<string>>
    {
        public override List<string> Parse(TinyYamlNode node)
        {
            return node.Value.Split(new[] { ',' }).Select(x => x.Trim()).ToList();
        }

        public override void Populate(TinyYamlNode node, object obj)
        {
            node.Value = string.Join(", ", (obj as List<string>).ToArray());
        }
    }
}
