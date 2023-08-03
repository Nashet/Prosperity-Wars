using Nashet.Map.Utils;
using System.Text;

namespace Nashet.NameGeneration
{
	public static class CountryNameGenerator
    {
        private static ChanceBox<string> prefix;
        private static ChanceBox<string> postfix;

		static CountryNameGenerator()
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

        private static StringBuilder result = new StringBuilder();

        public static string generateCountryName()
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