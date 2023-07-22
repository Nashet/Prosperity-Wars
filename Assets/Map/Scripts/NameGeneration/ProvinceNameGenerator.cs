using Nashet.Map.Utils;
using System.Text;

namespace Nashet.NameGeneration
{
	public static class ProvinceNameGenerator
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
            return NameHelper.FirstLetterToUpper(sb.ToString());
            //return Game.threadDangerSB.ToString();
        }

        static ProvinceNameGenerator()
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

        private static StringBuilder result = new StringBuilder();

        public static string generateProvinceName()
        {
            result.Clear();
            result.Append(prefix.GetRandom());
            if (Rand.Get.Next(3) == 1) result.Append(generateWord(Rand.Get.Next(2, 5)));
            else
                result.Append(generateWord(Rand.Get.Next(3, 5)));
            result.Append(postfix.GetRandom());

            return NameHelper.FirstLetterToUpper(result.ToString());
        }
    }
}