using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Linq;
public class ProvinceNameGenerator
{
    static ChanceBox<string> prefix;
    static ChanceBox<string> postfix;
    public ProvinceNameGenerator()
    {
        postfix = new ChanceBox<string>();
        postfix.add("burg", 2.2f);
        postfix.add("bridge", 0.1f);
        postfix.add("coln", 0.2f);


        postfix.add("field", 2f);
        postfix.add("hill", 1f);
        postfix.add("ford", 0.5f);
        postfix.add("land", 1f);
        postfix.add("landia", 0.3f);
        postfix.add("lia", 1f);
        postfix.add("mia", 0.1f);
        postfix.add("stad", 0.2f);

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
        postfix.add("", 25f);
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
        prefix.add("", 80f);
        prefix.initiate();
    }
    public string generateProvinceName()
    {

        return prefix.getRandom() + UtilsMy.generateWord(Game.random.Next(2, 5)) + postfix.getRandom();
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
        float randomNumver = UnityEngine.Random.value;
        foreach (Mean next in list)
            if (randomNumver <= next.weight)
                return next.element;
        return default(T);
    }
}

public class PricePool
{
    Dictionary<Product, DataStorage2> pool = new Dictionary<Product, DataStorage2>();
    static internal int lenght = 40; // !! duplicate of DataStorage!!
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
    public static Color getRandomColor()
    {
        return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1f);
    }
    public static void Clear(this StringBuilder value)
    {
        value.Length = 0;
    }
    public static Color setAlphaToZero(this Color color)
    {
        color.a = 0f;
        return color;
    }
    public static Color setAlphaToMax(this Color color)
    {
        color.a = 1f;
        return color;
    }
    public static void setColor(this Texture2D image, Color color)
    {
        for (int j = 0; j < image.height; j++) // cicle by province        
            for (int i = 0; i < image.width; i++)
                image.SetPixel(i, j, color);
    }
    public static void setAlphaToMax(this Texture2D image)
    {
        for (int j = 0; j < image.height; j++) // cicle by province        
            for (int i = 0; i < image.width; i++)
               // if (image.GetPixel(i, j) != Color.black)
                    image.SetPixel(i, j, image.GetPixel(i, j).setAlphaToMax());
    }
    static void drawSpot(Texture2D image, int x, int y, Color color)
    {
        int straightBorderChance = 5;
        if (x >= 0 && x < image.width && y >= 0 && y < image.height)
            if (Game.random.Next(straightBorderChance) != 1)
                //if (image.GetPixel(x, y).a != 1f || image.GetPixel(x, y) == Color.black)
                if ( image.GetPixel(x, y) == Color.black)
                    image.SetPixel(x, y, color.setAlphaToZero());
    }
    public static void drawRandomSpot(this Texture2D image, int x, int y, Color color)
    {
        //draw 4 points around x, y
        //int chance = 90;
        drawSpot(image, x - 1, y, color);
        drawSpot(image, x + 1, y, color);
        drawSpot(image, x, y - 1, color);
        drawSpot(image, x, y + 1, color);
        //if (x - 1 >= 0 && (image.GetPixel(x - 1, y).a != 1f || image.GetPixel(x - 1, y) == Color.black) && )
        //    image.SetPixel(x - 1, y, color.setAlphaToZero());
        //if (x + 1 < image.width && (image.GetPixel(x + 1, y).a != 1f || image.GetPixel(x - 1, y) == Color.black) && Game.random.Next(chance) != 1)
        //    image.SetPixel(x + 1, y, color.setAlphaToZero());
        //if (y - 1 >= 0 && (image.GetPixel(x, y - 1).a != 1f || image.GetPixel(x - 1, y) == Color.black) && Game.random.Next(chance) != 1)
        //    image.SetPixel(x, y - 1, color.setAlphaToZero());
        //if (y + 1 < image.height && (image.GetPixel(x, y + 1).a != 1f || image.GetPixel(x - 1, y) == Color.black) && Game.random.Next(chance) != 1)
        //    image.SetPixel(x, y + 1, color.setAlphaToZero());

    }
    public static int getRandomX(this Texture2D image)
    {
        return Game.random.Next(0, image.width);
    }
    public static int getRandomY(this Texture2D image)
    {
        return Game.random.Next(0, image.height);
    }
    public static string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }
    public static string generateWord(int length)
    {
        var rnd = Game.random;
        string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
        string[] vowels = { "a", "e", "i", "o", "u" };

        string result = "";

        if (length == 1)
            result = GetRandomLetter(rnd, vowels);
        else
        {
            for (int i = 0; i < length; i += 2)
            {
                result += GetRandomLetter(rnd, consonants) + GetRandomLetter(rnd, vowels);
                if (rnd.Next(4) == 1) result += GetRandomLetter(rnd, consonants);
            }
        }
        return FirstLetterToUpper(result);
    }

    private static string GetRandomLetter(System.Random rnd, string[] letters)
    {
        return letters[rnd.Next(0, letters.Length - 1)];
    }

    public static bool isSameColorsWithoutAlpha(Color colorA, Color colorB)
    {
        if (colorA.b == colorB.b && colorA.g == colorB.g && colorA.r == colorB.r)
            return true;
        else
            return false;

    }
    public static float getHumidityRatio(float massVapor, float massDryAir)
    {
        return massVapor / massDryAir;
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

    public static Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;


        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
            }
        }
        flipped.Apply();

        return flipped;
    }
}
