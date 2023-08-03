using System.Collections.Generic;

namespace Nashet.Utils
{
	//DateTime
	//public struct Date
	//{
	//    int blya;
	//    public Date(int date)
	//    {
	//        this.blya = date;
	//    }
	//    /// <summary>
	//    /// copy constructor
	//    /// </summary>
	//    public Date(Date date)
	//    {
	//        this.blya = date.blya;
	//    }
	//    public int getDate()
	//    {
	//        return blya;
	//    }
	//    public int getTimeSince(Date anotherDate)
	//    {
	//        return blya - anotherDate.getDate();
	//    }

	//    public void StepSimulation()
	//    {
	//        //blya++;
	//        blya = 100;
	//    }

	//    public bool isItTimeOf(Date anotherDate)
	//    {
	//        return blya == anotherDate.blya;
	//    }
	//    public override string ToString()
	//    {
	//        return "year " + blya;
	//    }

	//    public void set(Date date)
	//    {
	//        this.blya = date.blya;
	//    }
	//}
	public static class EdgeHelpers
    {
        public struct Edge
        {
            public int v1;
            public int v2;
            public int triangleIndex;

            public Edge(int aV1, int aV2, int aIndex)
            {
                v1 = aV1;
                v2 = aV2;
                triangleIndex = aIndex;
            }

            public static bool operator ==(Edge c1, Edge c2)
            {
                return (c1.v1 == c2.v1 && c1.v2 == c2.v2) || (c1.v1 == c2.v2 && c1.v2 == c2.v1);
            }

            public static bool operator !=(Edge c1, Edge c2)
            {
                return (c1.v1 != c2.v1 || c1.v2 != c2.v2) && (c1.v1 != c2.v2 || c1.v2 != c2.v1);
            }
        }

        public static List<Edge> GetEdges(int[] aIndices)
        {
            List<Edge> result = new List<Edge>();
            for (int i = 0; i < aIndices.Length; i += 3)
            {
                int v1 = aIndices[i];
                int v2 = aIndices[i + 1];
                int v3 = aIndices[i + 2];
                result.Add(new Edge(v1, v2, i));
                result.Add(new Edge(v2, v3, i));
                result.Add(new Edge(v3, v1, i));
            }

            return result;
        }

        public static List<Edge> FindBoundary(this List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = result.Count - 1; i > 0; i--)
            {
                for (int n = i - 1; n >= 0; n--)
                {
                    if (result[i].v1 == result[n].v2 && result[i].v2 == result[n].v1)
                    {
                        // shared edge so remove both
                        result.RemoveAt(i);
                        result.RemoveAt(n);
                        i--;
                        break;
                    }
                }
            }
            return result;
        }

        public static List<Edge> SortEdges(this List<Edge> aEdges)
        {
            List<Edge> result = new List<Edge>(aEdges);
            for (int i = 0; i < result.Count - 2; i++)
            {
                Edge E = result[i];
                for (int n = i + 1; n < result.Count; n++)
                {
                    Edge a = result[n];
                    if (E.v2 == a.v1)
                    {
                        // in this case they are already in order so just continoue with the next one
                        if (n == i + 1)
                            break;
                        // if we found a match, swap them with the next one after "i"
                        result[n] = result[i + 1];
                        result[i + 1] = a;
                        break;
                    }
                }
            }
            return result;
        }
    }
}