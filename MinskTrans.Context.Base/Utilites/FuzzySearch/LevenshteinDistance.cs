using System;
using System.Collections.Generic;
using System.Linq;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.Utilites.FuzzySearch
{
	public class Levenshtein
	{
		public static int LevenshteinDistance(string src, string dest)
		{
			int[,] d = new int[src.Length + 1, dest.Length + 1];
			int i, j, cost;
			char[] str1 = src.ToCharArray();
			char[] str2 = dest.ToCharArray();

			for (i = 0; i <= str1.Length; i++)
			{
				d[i, 0] = i;
			}
			for (j = 0; j <= str2.Length; j++)
			{
				d[0, j] = j;
			}
			for (i = 1; i <= str1.Length; i++)
			{
				for (j = 1; j <= str2.Length; j++)
				{

					if (str1[i - 1] == str2[j - 1])
						cost = 0;
					else
						cost = 1;

					d[i, j] =
						Math.Min(
							d[i - 1, j] + 1, // Deletion
							Math.Min(
								d[i, j - 1] + 1, // Insertion
								d[i - 1, j - 1] + cost)); // Substitution

					if ((i > 1) && (j > 1) && (str1[i - 1] ==
					                           str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
					{
						d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
					}
				}
			}

			return d[str1.Length, str2.Length];
		}
	public static List<Stop> Search(
	string word,
	IList<Stop> wordList,
	double fuzzyness)
	{
		// Tests have prove that the !LINQ-variant is about 3 times
		// faster!
		List<Stop> foundWords =
			(
				from s in wordList
				let levenshteinDistance = LevenshteinDistance(word, s.SearchName)
				let length = Math.Max(s.SearchName.Length, word.Length)
				let score = 1.0 - (double)levenshteinDistance / length
				where score >= fuzzyness
				orderby score descending 
				select s
			).ToList();

		return foundWords;
	}
	}

}
