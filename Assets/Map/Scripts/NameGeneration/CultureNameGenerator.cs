using Nashet.Map.Utils;
using System.Text;

namespace Nashet.NameGeneration
{
    public static class NameHelper
    {
		public static string FirstLetterToUpper(string str)
		{
			if (str == null)
				return null;

			if (str.Length > 1)
				return char.ToUpper(str[0]) + str.Substring(1);

			return str.ToUpper();
		}
	}
	public static class CultureNameGenerator
    {
        private static ChanceBox<string> prefix;
        private static ChanceBox<string> postfix;

		static CultureNameGenerator()
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

        private static StringBuilder result = new StringBuilder();

        public static string generateCultureName()
        {
            result.Clear();
            result.Append(prefix.GetRandom());

            //result.Append(UtilsMy.FirstLetterToUpper(RandWord.Models.RandomWordGenerator.Word(Rand.random2.Next(3) + 1, true)));
            result.Append(NameHelper.FirstLetterToUpper(ProvinceNameGenerator.generateWord(Rand.Get.Next(3, 5))));
            result.Append(postfix.GetRandom());

            return (result.ToString());
        }
	}
}