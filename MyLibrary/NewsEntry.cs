using System;
using System.Collections.Generic;

namespace MyLibrary
{
	public struct NewsEntry
	{
		public NewsEntry(DateTime dateTimeNews, string decodedString) : this()
		{
			Posted = dateTimeNews;
			Message = decodedString;
			Collected = DateTime.UtcNow;
		}

		public NewsEntry(DateTime dateTimeNews, string decodedString, DateTime possibleDateTime) : this(dateTimeNews, decodedString)
		{
			RepairedLIne = possibleDateTime;
		}

		#region Overrides of ValueType

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		public override string ToString()
		{
			return Posted + "\n" + Message;
		}

		#endregion

		public string Message { get; set; }

		public DateTime Posted { get; set; }

		public DateTime Collected { get; set; }
		public DateTime RepairedLIne { get; set; }
	}

	public class NewsEntryEqualityComparer: IEqualityComparer<NewsEntry>
	{
		#region Implementation of IEqualityComparer<in NewsEntry>

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		/// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
		public bool Equals(NewsEntry x, NewsEntry y)
		{
			if (x.Posted == y.Posted && x.RepairedLIne == y.RepairedLIne && x.Message == y.Message)
				return true;
			return false;
		}

		/// <summary>
		/// Returns a hash code for the specified object.
		/// </summary>
		/// <returns>
		/// A hash code for the specified object.
		/// </returns>
		/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
		public int GetHashCode(NewsEntry obj)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
