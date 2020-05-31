using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegraph.Net.Models;

namespace IT.PostManager.Core.Logic
{
    public static class NodeElementListExtensions
    {
        public static string ToText(this List<NodeElement> list)
        {
            var sb = new StringBuilder();
            foreach (var node in list)
            {
                if (NodeIsText(node))
                {
                    sb = ProcessTextNode(sb, node);
                } 
                else if (NodeIsValidParagraph(node))
                {
                    sb = ProcessParagraphNode(sb, node);
                }
            }
            return sb.ToString();
        }

        private static bool NodeIsText(NodeElement node)
        {
            return node.Tag == "_text";
        }

        private static StringBuilder ProcessTextNode(StringBuilder sb, NodeElement node)
        {
            sb.Append(node.Attributes["value"]);
            sb.Append("\n\n");
            return sb;
        }
        
        private static bool NodeIsValidParagraph(NodeElement node)
        {
            return node.Tag == "p" && node.Children.Any();
        }
        
        private static StringBuilder ProcessParagraphNode(StringBuilder sb, NodeElement node)
        {
            return node.Children.Where(NodeIsText).Aggregate(sb, ProcessTextNode);
        }
    }
}