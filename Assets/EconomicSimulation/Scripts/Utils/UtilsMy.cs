using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Nashet.EconomicSimulation;
using Nashet.ValueSpace;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nashet.Utils
{
    public interface ICopyable<T>
    {
        T Copy();
    }

    public class CultureNameGenerator
    {
        private static ChanceBox<string> prefix;
        private static ChanceBox<string> postfix;

        public CultureNameGenerator()
        {
            postfix = new ChanceBox<string>();
            postfix.Add("nian", 1.6f);
            postfix.Add("rian", 1f);
            postfix.Add("man", 3.0f);
            postfix.Add("men", 2.2f);
            postfix.Add("tian", 1f);
            postfix.Add("sian", 1.5f);

            postfix.Add("pian", 1f);
            postfix.Add("vian", 1f);
            postfix.Add("lian", 1.8f);

            postfix.Add("", 5f);
            postfix.Initiate();

            prefix = new ChanceBox<string>();

            prefix.Add("South ", 0.3f);
            prefix.Add("West ", 0.3f);
            prefix.Add("North ", 0.3f);
            prefix.Add("East ", 0.3f);
            prefix.Add("Great ", 0.8f);
            prefix.Add("Upper ", 0.2f);
            prefix.Add("Middle ", 0.1f);
            prefix.Add("", 40f);
            prefix.Initiate();
        }

        private StringBuilder result = new StringBuilder();

        public string generateCultureName()
        {
            result.Clear();
            result.Append(prefix.GetRandom());

            //result.Append(UtilsMy.FirstLetterToUpper(RandWord.Models.RandomWordGenerator.Word(Rand.random2.Next(3) + 1, true)));
            result.Append(UtilsMy.FirstLetterToUpper(ProvinceNameGenerator.generateWord(Rand.Get.Next(3, 5))));
            result.Append(postfix.GetRandom());

            return (result.ToString());
        }
    }

    public class CountryNameGenerator
    {
        private static ChanceBox<string> prefix;
        private static ChanceBox<string> postfix;

        public CountryNameGenerator()
        {
            postfix = new ChanceBox<string>();
            postfix.Add("burg", 1.2f);

            postfix.Add("hill", 0.31f);

            postfix.Add("land", 1.0f);
            postfix.Add("lands", 1.2f);
            postfix.Add("landia", 0.3f);
            postfix.Add("stan", 0.3f);

            postfix.Add("lia", 1.8f);
            postfix.Add("mia", 0.1f);
            postfix.Add("nia", 1.1f);
            postfix.Add("sia", 1.1f);
            postfix.Add("cia", 1.1f);
            postfix.Add("ria", 1.1f);

            postfix.Add("stad", 0.3f);

            postfix.Add("holm", 0.3f);
            postfix.Add("bruck", 0.3f);

            postfix.Add("berg", 1f);

            postfix.Add("polis", 2f);
            postfix.Add("", 10f);
            postfix.Initiate();

            prefix = new ChanceBox<string>();

            prefix.Add("South ", 0.3f);
            prefix.Add("West ", 0.3f);
            prefix.Add("North ", 0.3f);
            prefix.Add("East ", 0.3f);
            prefix.Add("Holy ", 0.1f);
            prefix.Add("Great ", 0.8f);
            prefix.Add("Saint ", 0.2f);
            prefix.Add("Dark ", 0.01f);
            prefix.Add("Upper ", 0.2f);
            prefix.Add("Middle ", 0.1f);

            prefix.Add("", 80f);
            prefix.Initiate();
        }

        private StringBuilder result = new StringBuilder();

        public string generateCountryName()
        {
            result.Clear();
            result.Append(prefix.GetRandom());

            //result.Append(UtilsMy.FirstLetterToUpper(RandWord.Models.RandomWordGenerator.Word(Rand.random2.Next(3) + 1, true)));
            result.Append(UtilsMy.FirstLetterToUpper(ProvinceNameGenerator.generateWord(Rand.Get.Next(3, 5))));
            result.Append(postfix.GetRandom());

            return (result.ToString());
        }
    }

    public class ProvinceNameGenerator
    {
        private static ChanceBox<string> prefix;
        private static ChanceBox<string> postfix;
        private static ChanceBox<string> vowels = new ChanceBox<string>();
        private static ChanceBox<string> consonants = new ChanceBox<string>();

        public static string generateWord(int length)
        {
            var sb = new StringBuilder();
            if (Rand.Get.Next(10) == 1)
            {
                sb.Append(vowels.GetRandom());
                if (Rand.Get.Next(2) == 1)
                    sb.Append(consonants.GetRandom());
            }
            //if (Rand.random2.Next(6) == 1)
            //    Game.threadDangerSB.Append(consonants.getRandom());

            for (int i = 0; i < length; i += 2)
            {
                sb.Append(consonants.GetRandom()).Append(vowels.GetRandom());
                if (Rand.Get.Next(5) == 1 || length == 2)
                    sb.Append(consonants.GetRandom());
            }
            return UtilsMy.FirstLetterToUpper(sb.ToString());
            //return Game.threadDangerSB.ToString();
        }

        public ProvinceNameGenerator()
        {
            postfix = new ChanceBox<string>();
            postfix.Add("burg", 2.2f);
            postfix.Add("bridge", 0.1f);
            postfix.Add("coln", 0.2f);

            postfix.Add("field", 2f);
            postfix.Add("hill", 1f);
            postfix.Add("ford", 0.5f);
            postfix.Add("land", 2.5f);
            postfix.Add("landia", 0.3f);
            postfix.Add("lia", 2.5f);
            postfix.Add("mia", 0.1f);
            postfix.Add("stad", 0.3f);

            postfix.Add("holm", 1f);
            postfix.Add("bruck", 0.3f);
            postfix.Add("bridge", 0.3f);
            postfix.Add("berg", 1f);
            postfix.Add(" Creek", 1f);
            postfix.Add(" Lakes", 1.5f);
            postfix.Add(" Falls", 1f);
            postfix.Add("rock", 2f);
            postfix.Add("ville", 2f);
            postfix.Add("polis", 2f);

            postfix.Add("lyn", 2f);
            postfix.Add("minster", 0.1f);
            postfix.Add("ton", 2f);
            postfix.Add("bury", 2f);
            postfix.Add("wich", 1f);

            postfix.Add("caster", 0.1f);
            postfix.Add("ham", 2f);
            postfix.Add("mouth", 2f);

            postfix.Add("ness", 2f);
            postfix.Add("pool", 2f);
            postfix.Add("stead", 2f);
            postfix.Add("wick", 1f);

            postfix.Add("worth", 2f);

            postfix.Add("", 10f);
            postfix.Initiate();

            prefix = new ChanceBox<string>();
            prefix.Add("Fort ", 0.5f);
            prefix.Add("South ", 0.3f);
            prefix.Add("West ", 0.3f);
            prefix.Add("North ", 0.3f);
            prefix.Add("East ", 0.3f);
            prefix.Add("Saint ", 0.1f);
            prefix.Add("Great ", 0.2f);
            prefix.Add("Dark ", 0.01f);
            prefix.Add("Upper ", 0.2f);
            prefix.Add("Middle ", 0.1f);

            prefix.Add("Gate ", 0.2f);
            prefix.Add("Kings ", 0.3f);
            //prefix.add("Knock", 0.6f);
            //prefix.add("Ling", 0.6f);
            //prefix.add("Weald", 0.6f);

            //prefix.add("Kirk", 0.6f);
            //prefix.add("Brad", 0.6f);
            //prefix.add("Inner ", 0.6f);
            //prefix.add("Lang", 0.6f);
            //prefix.add("Nor", 0.6f);

            //prefix.add("Pen", 0.6f);
            //prefix.add("Sud", 0.6f);

            prefix.Add("", 60f);
            prefix.Initiate();

            vowels.Add("a", 8.167f);
            vowels.Add("e", 12.702f);
            vowels.Add("i", 6.966f);
            vowels.Add("o", 7.507f);
            vowels.Add("u", 2.758f);
            //vowels.add("a", 8.167f);
            vowels.Initiate();

            consonants.Add("b", 1.492f);
            consonants.Add("c", 2.782f);
            consonants.Add("d", 4.253f);

            consonants.Add("f", 2.228f);
            consonants.Add("g", 2.015f);
            consonants.Add("h", 0.1f); //IRL -  6.094f);

            consonants.Add("j", 0.03f);//0.153f);
            consonants.Add("k", 0.772f);
            consonants.Add("l", 4.025f);
            consonants.Add("m", 2.406f);
            consonants.Add("n", 6.749f);

            consonants.Add("p", 1.929f);
            consonants.Add("q", 0.095f);
            consonants.Add("r", 5.987f);
            consonants.Add("s", 6.327f);
            consonants.Add("t", 9.056f);

            consonants.Add("v", 0.978f);
            consonants.Add("w", 2.360f);
            consonants.Add("x", 0.150f);
            consonants.Add("y", 0.174f); //IRL 1.974f
            consonants.Add("z", 0.074f);
            consonants.Initiate();
        }

        private StringBuilder result = new StringBuilder();

        public string generateProvinceName()
        {
            result.Clear();
            result.Append(prefix.GetRandom());
            if (Rand.Get.Next(3) == 1) result.Append(generateWord(Rand.Get.Next(2, 5)));
            else
                result.Append(generateWord(Rand.Get.Next(3, 5)));
            result.Append(postfix.GetRandom());

            return UtilsMy.FirstLetterToUpper(result.ToString());
        }
    }

    /// <summary>
    /// Redo input to dictionary
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChanceBox<T>
    {
        private class Mean
        {
            //KeyValuePair<string,int>
            public T element;

            public float weight;

            public Mean(T obj, float inchance)
            {
                element = obj;
                weight = inchance;
            }

            public override string ToString()
            {
                return element + " " + weight;
            }
        }

        //SortedDictionary
        //SortedDictionary<T, float> list = new SortedDictionary<T, float>();
        //todo make it dictionary
        private List<Mean> list = new List<Mean>();

        public void Add(T obj, float chance)
        {
            list.Add(new Mean(obj, chance));
        }

        public void Initiate()
        {
            float totalWeight = 0f;

            //list = list.OrderByDescending(o => o.weight).ToList();
            list = list.OrderBy(o => o.weight).ToList();
            int count = list.Count;
            foreach (var next in list)
            {
                // next.weight += count;
                totalWeight += next.weight;
                count--;
            }

            foreach (Mean next in list)
            {
                next.weight = next.weight / totalWeight;
                //next.weight = next.weight / list.Count ;
            }
            for (int i = 1; i < list.Count; i++)
            {
                list[i].weight += list[i - 1].weight;
            }
        }

        /// <summary>Gives random T according element weight  /// </summary>
        public T GetRandom()
        {
            float randomNumber = Rand.getFloat(0f, 1f);
            foreach (Mean next in list)
                if (randomNumber <= next.weight)
                    return next.element;
            return default(T);
        }
    }

    public class PricePool
    {
        private Dictionary<Product, DataStorageProduct> pool = new Dictionary<Product, DataStorageProduct>();
        public static readonly int lenght = 40; // !! duplicate of DataStorage!!

        public PricePool()
        {
            foreach (var product in Product.AllNonAbstract())
                if (product != Product.Gold)
                    for (int i = 0; i < lenght; i++)
                        addData(product, new Value(0f));
        }

        public void addData(Product product, Value indata)
        {
            DataStorageProduct cell;
            if (!pool.TryGetValue(product, out cell))
            {
                cell = new DataStorageProduct(product);
                pool.Add(product, cell);
            }
            cell.addData(indata);
        }

        //public System.Collections.IEnumerator GetEnumerator()
        //{
        //    for (int i = 0; i < pool.Count; i++)
        //    {
        //        yield return pool.GetEnumerator();
        //    }
        //}
        public DataStorageProduct getPool(Product product)
        {
            //return pool[pro];
            DataStorageProduct result;
            if (pool.TryGetValue(product, out result)) // Returns true.
            {
                return result;
            }
            else
                return null;
        }
    }

    public class DataStorageProduct : DataStorage<Product>
    {
        public DataStorageProduct(Product inn) : base(inn)
        {
        }
    }

    public class DataStorage<IDTYPE>
    {
        private static int length = 40;

        //todo use LinkedList<T> instead of queue?
        public LimitedQueue<Value> data;

        private IDTYPE ID;

        public DataStorage(IDTYPE inn)
        {
            data = new LimitedQueue<Value>(length);
            ID = inn;
        }

        public void addData(Value indata)
        {
            data.Enqueue(new Value(indata.get()));
        }
    }

    public class LimitedQueue<T> : Queue<T>
    {
        public int Limit { get; set; }

        public LimitedQueue(int limit) : base(limit)
        {
            Limit = limit;
        }

        public new void Enqueue(T item)
        {
            while (Count >= Limit)
            {
                Dequeue();
            }
            base.Enqueue(item);
        }
    }

    public static class UtilsMy
    {
        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static GameObject CreateButton(Transform parent, float x, float y,
                                            float w, float h, string message,
                                            UnityAction eventListner)
        {
            GameObject buttonObject = new GameObject("Button");
            buttonObject.transform.SetParent(parent);

            //buttonObject.layer = LayerUI;

            RectTransform trans = buttonObject.AddComponent<RectTransform>();
            SetSize(trans, new Vector2(w, h));
            trans.anchoredPosition3D = new Vector3(0, 0, 0);
            trans.anchoredPosition = new Vector2(x, y);
            trans.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            trans.localPosition.Set(0, 0, 0);

            CanvasRenderer renderer = buttonObject.AddComponent<CanvasRenderer>();

            Image image = buttonObject.AddComponent<Image>();

            Texture2D tex = Resources.Load<Texture2D>("button_bkg");
            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                                      new Vector2(0.5f, 0.5f));

            Button button = buttonObject.AddComponent<Button>();
            button.interactable = true;
            button.onClick.AddListener(eventListner);

            GameObject textObject = CreateText(buttonObject.transform, 0, 0, 0, 0,
                                                       message, 24);

            return buttonObject;
        }

        private static void SetSize(RectTransform trans, Vector2 size)
        {
            Vector2 currSize = trans.rect.size;
            Vector2 sizeDiff = size - currSize;
            trans.offsetMin = trans.offsetMin -
                                      new Vector2(sizeDiff.x * trans.pivot.x,
                                          sizeDiff.y * trans.pivot.y);
            trans.offsetMax = trans.offsetMax +
                                      new Vector2(sizeDiff.x * (1.0f - trans.pivot.x),
                                          sizeDiff.y * (1.0f - trans.pivot.y));
        }

        private static GameObject CreateText(Transform parent, float x, float y,
                                         float w, float h, string message, int fontSize)
        {
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(parent);

            //textObject.layer = LayerUI;

            RectTransform trans = textObject.AddComponent<RectTransform>();
            trans.sizeDelta.Set(w, h);
            trans.anchoredPosition3D = new Vector3(0, 0, 0);
            trans.anchoredPosition = new Vector2(x, y);
            trans.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            trans.localPosition.Set(0, 0, 0);

            CanvasRenderer renderer = textObject.AddComponent<CanvasRenderer>();

            Text text = textObject.AddComponent<Text>();
            text.supportRichText = true;
            text.text = message;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.color = new Color(0, 0, 1);

            return textObject;
        }
    }

    public class DontUseThatMethod : Exception
    {
        /// <summary>
        /// Just create the exception
        /// </summary>
        public DontUseThatMethod()
        {
        }

        /// <summary>
        /// Create the exception with description
        /// </summary>
        /// <param name="message">Exception description</param>
        public DontUseThatMethod(String message)
          : base(message)
        {
        }

        /// <summary>
        /// Create the exception with description and inner cause
        /// </summary>
        /// <param name="message">Exception description</param>
        /// <param name="innerException">Exception inner cause</param>
        public DontUseThatMethod(String message, Exception innerException)
          : base(message, innerException)
        {
        }

        /// <summary>
        /// Create the exception from serialized data.
        /// Usual scenario is when exception is occurred somewhere on the remote workstation
        /// and we have to re-create/re-throw the exception on the local machine
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected DontUseThatMethod(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }

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

    public abstract class ThreadedJob
    {
        private bool m_IsDone;
        private string status = "Not started yet";
        private object m_Handle = new object();
        private Thread m_Thread;

        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (m_Handle)
                {
                    tmp = m_IsDone;
                }
                return tmp;
            }
            set
            {
                lock (m_Handle)
                {
                    m_IsDone = value;
                }
            }
        }

        public void updateStatus(String status)
        {
            lock (this.status)
            {
                this.status = status;
            }
        }

        public string getStatus()
        {
            //tmp = status;
            lock (status)
            {
                return status;
            }
        }

        public virtual void Start()
        {
            m_Thread = new Thread(Run);
            m_Thread.Start();
        }

        public virtual void Abort()
        {
            m_Thread.Abort();
        }

        protected abstract void ThreadFunction();

        protected virtual void OnFinished()
        {
        }

        public virtual bool Update()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }

        public IEnumerator WaitFor()
        {
            while (!Update())
            {
                yield return null;
            }
        }

        private void Run()
        {
            ThreadFunction();
            IsDone = true;
        }
    }

    public class MyTexture
    {
        private readonly int width, height;
        private readonly Color[] map;

        public MyTexture(Texture2D image)
        {
            width = image.width;
            height = image.height;
            map = image.GetPixels();
        }

        public int getWidth()
        {
            return width;
        }

        public int getHeight()
        {
            return height;
        }

        public Color GetPixel(int x, int v)
        {
            return map[x + v * width];
        }

        public Color getRandomPixel()
        {
            return map[Rand.Get.Next((width * height) - 1)];
        }
        public List<Color> AllUniqueColors()
        {
            var res = new List<Color>();
            ProvinceNameGenerator nameGenerator = new ProvinceNameGenerator();
            Color nextColor = map[0];

            for (int i = 0; i < map.Length; i++)
            {
                if (nextColor != map[i]
                    && !res.Contains(nextColor))
                {
                    res.Add(nextColor);
                }
                nextColor = map[i];

            }
            return res;
        }
        
        public Dictionary<Color, bool> AllUniqueColors2()
        {
            // true means is a sea
            var res = new Dictionary<Color, bool>();
            Color nextColor = map[0];
            for (int y = 0; y < height; y++)
            {
                if (nextColor != map[y * width]
                   && !res.ContainsKey(nextColor))
                {

                    res.Add(nextColor, true);
                }
                nextColor = map[y * width];
            }
            for (int y = 0; y < height; y++)
            {
                if (nextColor != map[width - 1 + y * width]
                   && !res.ContainsKey(nextColor))
                {

                    res.Add(nextColor, true);
                }
                nextColor = map[width - 1 + y * width];
            }
            for (int x = 0; x < width; x++)
            {
                if (nextColor != map[x]
                   && !res.ContainsKey(nextColor))
                {

                    res.Add(nextColor, true);
                }
                nextColor = map[x];
            }
            for (int x = 0; x < width; x++)
            {
                if (nextColor != map[x + (height - 1) * width]
                   && !res.ContainsKey(nextColor))
                {

                    res.Add(nextColor, true);
                }
                nextColor = map[x + (height - 1) * width];
            }


            for (int y = 1; y < height - 1; y++)
                for (int x = 1; x < width - 1; x++)
                {
                    if (nextColor != map[x + y * width]
                        && !res.ContainsKey(nextColor))
                    {

                        res.Add(nextColor, false);
                    }
                    nextColor = map[x + y * width];

                }
            return res;
        }
    }
}