using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Linq;
using System.Runtime.Serialization;

public class CultureNameGenerator
{
    static ChanceBox<string> prefix;
    static ChanceBox<string> postfix;
    public CultureNameGenerator()
    {
        postfix = new ChanceBox<string>();
        postfix.add("nian", 1.6f);
        postfix.add("rian", 1f);
        postfix.add("man", 3.0f);
        postfix.add("men", 2.2f);
        postfix.add("tian", 1f);
        postfix.add("sian", 1.5f);

        postfix.add("pian", 1f);
        postfix.add("vian", 1f);
        postfix.add("lian", 1.8f);


        postfix.add("", 5f);
        postfix.initiate();

        prefix = new ChanceBox<string>();

        prefix.add("South ", 0.3f);
        prefix.add("West ", 0.3f);
        prefix.add("North ", 0.3f);
        prefix.add("East ", 0.3f);
        prefix.add("Great ", 0.8f);
        prefix.add("Upper ", 0.2f);
        prefix.add("Middle ", 0.1f);
        prefix.add("", 40f);
        prefix.initiate();
    }
    StringBuilder result = new StringBuilder();
    public string generateCultureName()
    {
        result.Clear();
        result.Append(prefix.getRandom());

        //result.Append(UtilsMy.FirstLetterToUpper(RandWord.Models.RandomWordGenerator.Word(Game.random.Next(3) + 1, true)));
        result.Append(UtilsMy.FirstLetterToUpper(ProvinceNameGenerator.generateWord(Game.Random.Next(3, 5))));
        result.Append(postfix.getRandom());

        return (result.ToString());
    }
}
public class CountryNameGenerator
{
    static ChanceBox<string> prefix;
    static ChanceBox<string> postfix;


    public CountryNameGenerator()
    {
        postfix = new ChanceBox<string>();
        postfix.add("burg", 1.2f);

        postfix.add("hill", 0.31f);

        postfix.add("land", 1.0f);
        postfix.add("lands", 1.2f);
        postfix.add("landia", 0.3f);
        postfix.add("stan", 0.3f);

        postfix.add("lia", 1.8f);
        postfix.add("mia", 0.1f);
        postfix.add("nia", 1.1f);
        postfix.add("sia", 1.1f);
        postfix.add("cia", 1.1f);
        postfix.add("ria", 1.1f);

        postfix.add("stad", 0.3f);

        postfix.add("holm", 0.3f);
        postfix.add("bruck", 0.3f);

        postfix.add("berg", 1f);

        postfix.add("polis", 2f);
        postfix.add("", 10f);
        postfix.initiate();

        prefix = new ChanceBox<string>();

        prefix.add("South ", 0.3f);
        prefix.add("West ", 0.3f);
        prefix.add("North ", 0.3f);
        prefix.add("East ", 0.3f);
        prefix.add("Holy ", 0.1f);
        prefix.add("Great ", 0.8f);
        prefix.add("Saint ", 0.2f);
        prefix.add("Dark ", 0.01f);
        prefix.add("Upper ", 0.2f);
        prefix.add("Middle ", 0.1f);

        prefix.add("", 80f);
        prefix.initiate();
    }
    StringBuilder result = new StringBuilder();
    public string generateCountryName()
    {
        result.Clear();
        result.Append(prefix.getRandom());

        //result.Append(UtilsMy.FirstLetterToUpper(RandWord.Models.RandomWordGenerator.Word(Game.random.Next(3) + 1, true)));
        result.Append(UtilsMy.FirstLetterToUpper(ProvinceNameGenerator.generateWord(Game.Random.Next(3, 5))));
        result.Append(postfix.getRandom());

        return (result.ToString());
    }
}
public class ProvinceNameGenerator
{
    static ChanceBox<string> prefix;
    static ChanceBox<string> postfix;
    static ChanceBox<string> vowels = new ChanceBox<string>();
    static ChanceBox<string> consonants = new ChanceBox<string>();
    public static string generateWord(int length)
    {
        Game.threadDangerSB.Clear();
        if (Game.Random.Next(10) == 1)
        {
            Game.threadDangerSB.Append(vowels.getRandom());
            if (Game.Random.Next(2) == 1)
                Game.threadDangerSB.Append(consonants.getRandom());
        }
        //if (Game.random.Next(6) == 1)
        //    Game.threadDangerSB.Append(consonants.getRandom());

        for (int i = 0; i < length; i += 2)
        {
            Game.threadDangerSB.Append(consonants.getRandom()).Append(vowels.getRandom());
            if (Game.Random.Next(5) == 1 || length == 2) Game.threadDangerSB.Append(consonants.getRandom());
        }
        return UtilsMy.FirstLetterToUpper(Game.threadDangerSB.ToString());
        //return Game.threadDangerSB.ToString();
    }
    public ProvinceNameGenerator()
    {
        postfix = new ChanceBox<string>();
        postfix.add("burg", 2.2f);
        postfix.add("bridge", 0.1f);
        postfix.add("coln", 0.2f);

        postfix.add("field", 2f);
        postfix.add("hill", 1f);
        postfix.add("ford", 0.5f);
        postfix.add("land", 2.5f);
        postfix.add("landia", 0.3f);
        postfix.add("lia", 2.5f);
        postfix.add("mia", 0.1f);
        postfix.add("stad", 0.3f);

        postfix.add("holm", 1f);
        postfix.add("bruck", 0.3f);
        postfix.add("bridge", 0.3f);
        postfix.add("berg", 1f);
        postfix.add(" Creek", 1f);
        postfix.add(" Lakes", 1.5f);
        postfix.add(" Falls", 1f);
        postfix.add("rock", 2f);
        postfix.add("ville", 2f);
        postfix.add("polis", 2f);

        postfix.add("lyn", 2f);
        postfix.add("minster", 0.1f);
        postfix.add("ton", 2f);
        postfix.add("bury", 2f);
        postfix.add("wich", 1f);

        postfix.add("caster", 0.1f);
        postfix.add("ham", 2f);
        postfix.add("mouth", 2f);

        postfix.add("ness", 2f);
        postfix.add("pool", 2f);
        postfix.add("stead", 2f);
        postfix.add("wick", 1f);

        postfix.add("worth", 2f);


        postfix.add("", 10f);
        postfix.initiate();

        prefix = new ChanceBox<string>();
        prefix.add("Fort ", 0.5f);
        prefix.add("South ", 0.3f);
        prefix.add("West ", 0.3f);
        prefix.add("North ", 0.3f);
        prefix.add("East ", 0.3f);
        prefix.add("Saint ", 0.1f);
        prefix.add("Great ", 0.2f);
        prefix.add("Dark ", 0.01f);
        prefix.add("Upper ", 0.2f);
        prefix.add("Middle ", 0.1f);

        prefix.add("Gate ", 0.2f);
        prefix.add("Kings ", 0.3f);
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


        prefix.add("", 60f);
        prefix.initiate();

        vowels.add("a", 8.167f);
        vowels.add("e", 12.702f);
        vowels.add("i", 6.966f);
        vowels.add("o", 7.507f);
        vowels.add("u", 2.758f);
        //vowels.add("a", 8.167f);
        vowels.initiate();

        consonants.add("b", 1.492f);
        consonants.add("c", 2.782f);
        consonants.add("d", 4.253f);

        consonants.add("f", 2.228f);
        consonants.add("g", 2.015f);
        consonants.add("h", 0.1f); //IRL -  6.094f);

        consonants.add("j", 0.03f);//0.153f);
        consonants.add("k", 0.772f);
        consonants.add("l", 4.025f);
        consonants.add("m", 2.406f);
        consonants.add("n", 6.749f);

        consonants.add("p", 1.929f);
        consonants.add("q", 0.095f);
        consonants.add("r", 5.987f);
        consonants.add("s", 6.327f);
        consonants.add("t", 9.056f);

        consonants.add("v", 0.978f);
        consonants.add("w", 2.360f);
        consonants.add("x", 0.150f);
        consonants.add("y", 0.174f); //IRL 1.974f
        consonants.add("z", 0.074f);
        consonants.initiate();
    }
    StringBuilder result = new StringBuilder();
    public string generateProvinceName()
    {
        result.Clear();
        result.Append(prefix.getRandom());
        if (Game.Random.Next(3) == 1) result.Append(generateWord(Game.Random.Next(2, 5)));
        else
            result.Append(generateWord(Game.Random.Next(3, 5)));
        result.Append(postfix.getRandom());

        return UtilsMy.FirstLetterToUpper(result.ToString());
    }
}
public class ChanceBox<T>
{
    class Mean
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
            return element.ToString() + " " + weight;
        }
    }
    //SortedDictionary
    //SortedDictionary<T, float> list = new SortedDictionary<T, float>();
    //todo make it dictionary
    List<Mean> list = new List<Mean>();
    public void add(T obj, float chance)
    {
        list.Add(new Mean(obj, chance));
    }
    public void initiate()
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
            next.weight = next.weight / totalWeight; ;
            //next.weight = next.weight / list.Count ;
        }
        for (int i = 1; i < list.Count; i++)
        {
            list[i].weight += list[i - 1].weight;
        }
    }
    /// <summary>Gives random T according element weight  /// </summary>    
    public T getRandom()
    {
        //float randomNumver = UnityEngine.Random.value;
        float randomNumber = Game.Random.getFloat(0f, 1f);
        foreach (Mean next in list)
            if (randomNumber <= next.weight)
                return next.element;
        return default(T);
    }
}

public class PricePool
{
    Dictionary<Product, DataStorage2> pool = new Dictionary<Product, DataStorage2>();
    static readonly internal int lenght = 40; // !! duplicate of DataStorage!!
    internal PricePool()
    {
        foreach (var pro in Product.allProducts)
            for (int i = 0; i < lenght; i++)
                this.addData(pro, new Value(0f));
    }
    internal void addData(Product pro, Value indata)
    {
        DataStorage2 cell;
        if (!pool.TryGetValue(pro, out cell))
        {
            cell = new DataStorage2(pro);
            pool.Add(pro, cell);
        }
        cell.addData(indata);
    }
    public System.Collections.IEnumerator GetEnumerator()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            yield return pool.GetEnumerator();
        }
    }
    internal DataStorage2 getPool(Product pro)
    {
        //return pool[pro];
        DataStorage2 result;
        if (pool.TryGetValue(pro, out result)) // Returns true.
        {
            return result;
        }
        else
            return null;
    }
}
public class DataStorage2 : DataStorage<Product>
{
    public DataStorage2(Product inn) : base(inn)
    {
    }
}
public class DataStorage<IDTYPE>
{
    static int length = 40;
    //todo use LinkedList<T> instead of queue?
    internal LimitedQueue<Value> data;
    IDTYPE ID;
    internal DataStorage(IDTYPE inn)
    {
        data = new LimitedQueue<Value>(length);
        ID = inn;
    }
    internal void addData(Value indata)
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
    public static float getFloat(this System.Random random, float minValue, float maxValue)
    {
        //float m = (maxValue - minValue) ;

        return (float)(random.NextDouble()) * (maxValue - minValue) + minValue;
    }

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
      : base()
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

//    internal void StepSimulation()
//    {
//        //blya++;
//        blya = 100;
//    }

//    internal bool isItTimeOf(Date anotherDate)
//    {
//        return blya == anotherDate.blya;
//    }
//    public override string ToString()
//    {
//        return "year " + blya;
//    }

//    internal void set(Date date)
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
    private bool m_IsDone = false;
    private string status = "Not started yet";
    private object m_Handle = new object();
    private System.Threading.Thread m_Thread = null;
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
        m_Thread = new System.Threading.Thread(Run);
        m_Thread.Start();
    }
    public virtual void Abort()
    {
        m_Thread.Abort();
    }

    protected abstract void ThreadFunction();

    protected virtual void OnFinished() { }

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
    readonly int width, height;
    readonly Color[] map;
    public MyTexture(Texture2D image)
    {
        width = image.width;
        height = image.height;
        map = image.GetPixels();
    }
    internal int getWidth()
    {
        return width;
    }
    internal int getHeight()
    {
        return height;
    }
    internal Color GetPixel(int x, int v)
    {
        return map[x + v * width];
    }
    public Color getRandomPixel()
    {
        return map[Game.Random.Next((width * height) - 1)];
    }
}
public abstract class Name
{
    protected readonly string name;
    protected Name(string name)
    {
        this.name = name;
    }
    virtual public string getName()
    {
        return name;
    }
    public string getDescription()
    {
        return name;
    }
    public override string ToString()
    {
        return name;
    }
}