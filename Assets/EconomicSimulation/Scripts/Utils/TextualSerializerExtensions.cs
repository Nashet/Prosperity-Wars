using System;
using System.Collections.Generic;
using System.Text;
namespace TextualSerializer
{
    public static class TextualSerializerExtensions
    {
        private static readonly char[] limits = new char[2] { ' ', '>' };

        internal static string MakeNode(string tagName, string tagValue)
        {
            var sb = new StringBuilder();
            sb.AppendLine().Append("<").Append(tagName).Append(">");
            sb.Append(tagValue);
            sb.Append("<").Append("/").Append(tagName).Append(">");
            return sb.ToString();
        }

        /// <summary>
        ///Format assumes that ID node will be first subnode of every reference type node
        /// </summary>        
        //public static string Serialize(this ListNode node)
        //{
        //    var tagBody = new StringBuilder();
        //    tagBody.Append(MakeNode("ID", node.GetHashCode().ToString()));
        //    if (node.Prev != null)
        //        tagBody.Append(MakeNode("Prev", node.Prev.GetHashCode().ToString()));
        //    if (node.Next != null)
        //        tagBody.Append(MakeNode("Next", node.Next.GetHashCode().ToString()));
        //    if (node.Rand != null)
        //        tagBody.Append(MakeNode("Rand", node.Rand.GetHashCode().ToString()));
        //    if (node.Data != null)
        //        tagBody.Append(MakeNode("Data", node.Data));

        //    return MakeNode("ListNode", tagBody.ToString());
        //}

        public static IEnumerable<KeyValuePair<string, string>> GetNodes(this string source)
        {
            source = source.Replace("\n", String.Empty);
            source = source.Replace("\r", String.Empty);
            source = source.Replace("\t", String.Empty);
            int offset = 0;
            while (true)
            {
                var tagOpensPosition = source.IndexOf("<", offset);
                if (tagOpensPosition == -1)//no more tags
                    break;
                var spaceIndex = source.IndexOfAny(limits, offset);
                
                var tagName = source.Substring(tagOpensPosition + 1, spaceIndex - tagOpensPosition - 1);

                var endWord = "</" + tagName + ">";
                var endClosingPosition = source.IndexOf(endWord, offset);
                offset = endClosingPosition + endWord.Length;

                var res = new KeyValuePair<string, string>(tagName,
                    source.Substring(tagOpensPosition + tagName.Length + 2, endClosingPosition - tagOpensPosition - endWord.Length + 1));

                yield return res;

            }
        }

        internal static T GetObjectByID<T>(this Dictionary<int, T> restoredElements, string textualID) where T : new()
        {
            var id = Int32.Parse(textualID);
            T value;
            if (restoredElements.TryGetValue(id, out value))
            {
                return value;
            }
            else
            {

                var f = new T();
                restoredElements.Add(id, f);
                return f;
            }
        }

        //public static string Serialize(this ListRand source)
        //{
        //    var sb = new StringBuilder();

        //    var nextNode = source.Head;
        //    while (nextNode != null)
        //    {
        //        sb.Append(nextNode.Serialize());
        //        nextNode = nextNode.Next;
        //    }
        //    if (source.Head != null)
        //        sb.Append(MakeNode("Head", source.Head.GetHashCode().ToString()));
        //    if (source.Tail != null)
        //        sb.Append(MakeNode("Tail", source.Tail.GetHashCode().ToString()));
        //    return sb.ToString();
        //}

        /// <summary>
        ///Format assumes that ID node will be first subnode of every reference type node
        /// </summary>        
        //private static void DeserialiseNode(Dictionary<int, ListNode> restoredElements, KeyValuePair<string, string> node)
        //{
        //    ListNode newNode = null;

        //    foreach (var subnode in node.Value.GetNodes())
        //    {
        //        switch (subnode.Key)
        //        {
        //            case "ID":
        //                newNode = restoredElements.GetObjectByID(subnode.Value);
        //                break;
        //            case "Data":
        //                newNode.Data = subnode.Value;
        //                break;
        //            case "Next":
        //                newNode.Next = restoredElements.GetObjectByID(subnode.Value);
        //                break;
        //            case "Prev":
        //                newNode.Prev = restoredElements.GetObjectByID(subnode.Value);
        //                break;
        //            case "Rand":
        //                newNode.Rand = restoredElements.GetObjectByID(subnode.Value);
        //                break;

        //            default:
        //                //Debug.Log("Unknown XML node:" + subnode.Key);
        //                break;
        //        }
        //    }
        //}
    }
}