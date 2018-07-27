using System;
using System.Linq;

//using System.Web;

namespace RandWord.Models
{
    public class RandomWordGenerator
    {
        // Settings
        private const int FirstSyllableHasStartConsonant = 60; // % 1st syllables which start with a consonant

        private const int SyllableHasStartConsonant = 50; // % syllables which start with a consonant
        private const int SyllableHasEndConsonant = 70; // % syllables which end with a consonant
        private const int StartConsonantComplex = 40; // % end consonants which are complex
        private const int EndConsonantComplex = 50; // % end consonants which are complex
        private const int FinalConsonantsModified = 30; // % modifiable end consonants which are modified
        private const int VowelComplex = 30; // % vowels which are complex

        // Used to determine the syllable count randomly. Higher numbers mean a syllable is more likely to be chosen.
        // Index 0 is 1 syllable, index 1 is 2 syllables, etc.
        private static int[] SyllableWeights = {
            2, // 1 syllable words
			5, // 2 syllable words
			3  // 3 syllable words
		};

        private static int CombinedSyllableWeights;

        static RandomWordGenerator()
        {
            CombinedSyllableWeights = 0;
            foreach (int weight in SyllableWeights)
            {
                CombinedSyllableWeights += weight;
            }
        }

        private static char[] vowels = {
            'a', 'e', 'i', 'o', 'u', 'y'
        };

        private static string[] complexVowels = {
            "ai", "au",
            "ea", "ee",
            "ie",
            "oo", "oa", "oi", "ou",
            "ua"
        };

        private static string[] simpleConsonants = {
            "b", "c", "d", "g", "l", "m", "n", "p", "s", "t", "w", "z", "v"
        };

        // complex consonant sounds
        private static string[] startConsonants = {
            "bl", "br",
            "ch", "cl", "cr",
            "dr",
            "f", "fl", "fr",
            "gl", "gr",
            "h",
            "j",
            "k", "kl", "kn", "kr",
            "pe", "ph", "pl", "pr",
            "qu",
            "rh",
            "sc", "sh", "sk", "sl", "sm", "sn", "sp", "st", "str",
            "th", "tr", "tw",
            "v",
            "wh", "wr",
            "y"
        };

        private static string[] endConsonants = {
            "ch", "ck",
            "dge",
            "fe", "ff", "ft",
            "ght", //"gh"
			"ke",
            "ld", "ll",
            "nd", "ng", "nk", "nt",
            "mg", "mp",
            "re", "rp", "rt",
            "sh", "sk", "sp", "ss", "st",
            "tch", "th",
            "x"
        };

        // consonants which can be modified (softening the consonant or changing the vowel) by adding an "e" to the end
        private static char[] endConsonantsModifiable = {
            'b', 'd', 'g', 'l', 'm', 'n', 'r', 't'
        };

        // consonants which must be modified if they occur at the end of a word
        private static char[] endConsonantsMustBeModified = {
            'c', 'l', 'v', 's'
        };

        // consonants where another consonant never follows mid-word
        private static char[] consonantNeverFollows = {
           'b'
        };

        private static Random rand = new Random();

        // simple test - run through and
        public static void Test()
        {
            for (int i = 0; i < 30; i++)
            {
                Console.WriteLine(Word());
            }
        }

        public static string Word()
        {
            // determine no of syllables
            int syllables = 0;
            var selector = rand.Next(CombinedSyllableWeights - 1);
            for (var i = 0; i < SyllableWeights.Length; i++)
            {
                var weight = SyllableWeights[i];
                if (selector < weight)
                {
                    syllables = i + 1;
                    break;
                }
                selector -= weight;
            }
            return Word(syllables, true);
        }

        public static string Word(int syllables)
        {
            return Word(syllables, true);
        }

        public static string Word(int syllables, bool filter)
        {
            if (syllables < 1) throw new ArgumentException("Word must have at least 1 syllable.");
            string word = "";
            string lastSyllable = null;
            for (int i = 0; i < syllables; i++)
            {
                bool makeItShort = i > 0 && syllables > 2;
                bool last = i == syllables - 1;
                string syllable = Syllable(lastSyllable, makeItShort, last);

                // single syllable words must be more than one character
                while (syllables == 1 && syllable.Length == 1)
                {
                    syllable = Syllable(lastSyllable, makeItShort, last);
                }

                //if(i > 0) word += "-";
                word += syllable;
                lastSyllable = syllable;
            }

            if (filter)
            {
                // check for filtered words
                foreach (var badWord in filtered)
                {
                    if (word.Contains(badWord)) return Word(syllables, true);
                }
            }
            return word;
        }

        private static string Syllable(string previous, bool makeItShort, bool isLast)
        {
            bool isFirst = previous == null;
            bool lastSyllableSimple = previous != null && previous.Length < 3;
            bool lastSyllableComplexEnd = false;
            bool lastLetterIsNeverFollowedByConsonant = false;

            if (previous != null)
            {
                int count = 0;
                for (int i = previous.Length - 1; i > 0; i--)
                {
                    char lookAt = previous[i];
                    if (!vowels.Contains(lookAt)) count++;
                    else break;
                }
                lastSyllableComplexEnd = count > 1;

                if (consonantNeverFollows.Contains(previous[previous.Length - 1])) lastLetterIsNeverFollowedByConsonant = true;
            }

            bool startConsonant = isFirst ?
                rand.Next(100) < FirstSyllableHasStartConsonant : rand.Next(100) < SyllableHasStartConsonant;

            startConsonant = startConsonant && (isFirst || lastLetterIsNeverFollowedByConsonant);

            // always start with consonant if previous syllable ends in vowel
            if (!startConsonant && previous != null && !lastLetterIsNeverFollowedByConsonant)
            {
                char lastChar = previous[previous.Length - 1];
                if (vowels.Contains(lastChar)) startConsonant = true;
            }

            bool endConsonant = !(makeItShort && startConsonant) && rand.Next(100) < SyllableHasEndConsonant;

            // always end in consonant if the last syllable was short
            if (lastSyllableSimple && !endConsonant)
            {
                endConsonant = true;
            }

            string syllable = "";

            string start = null;
            bool complexStartConsonant = false;
            if (startConsonant)
            {
                complexStartConsonant = !lastSyllableComplexEnd && rand.Next(100) < StartConsonantComplex;
                if (complexStartConsonant) start = startConsonants.Random();
                else start = simpleConsonants.Random();
                syllable += start;
            }

            bool complexVowel = startConsonant && !complexStartConsonant && rand.Next(100) < VowelComplex;
            if (complexVowel)
                syllable += complexVowels.Random();
            else
                syllable += vowels.Random().ToString();

            string end;
            if (endConsonant)
            {
                bool complex = rand.Next(100) < EndConsonantComplex;
                bool startEndsWithR = start != null && start.EndsWith("r");
                do
                {
                    if (complex) end = endConsonants.Random();
                    else end = simpleConsonants.Random();
                } while (startEndsWithR && end.StartsWith("r"));

                bool modify = isLast && (endConsonantsMustBeModified.Contains(end[end.Length - 1]) || (
                    (end == "ch" || endConsonantsModifiable.Contains(end[end.Length - 1])) && rand.Next(100) < FinalConsonantsModified));
                if (modify)
                    end += "e";

                syllable += end;
            }

            return syllable;
        }

        // Don't output any of these, in the unlikely event they get generated!
        private static string[] filtered = {
            "allah",
            "anal",
            "anus",
            "arse",
            "ass",
            "bitch",
            "bloody",
            "bollock",
            "boob",
            "cock",
            "cunt",
            "crap",
            "cum",
            "damn",
            "dick",
            "fag",
            "fart",
            "fuck",
            "gimp",
            "hell",
            "homo",
            "nigga",
            "nigger",
            "penis",
            "piss",
            "poo",
            "pussy",
            "rape",
            "rapist",
            "sex",
            "shit",
            "slut",
            "spastic",
            "tits",
            "twat",
            "vag",
            "vagina",
            "vomit",
            "wank",
            "whore"
        };
    }

    public static class RandomWordExtensions
    {
        private static Random rand = new Random();

        public static string Random(this string[] source)
        {
            return source[rand.Next(source.Length - 1)];
        }

        public static char Random(this char[] source)
        {
            return source[rand.Next(source.Length - 1)];
        }
    }
}