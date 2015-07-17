using System;
using System.Collections.Generic;
using System.Text;

namespace MyLibrary
{
	public struct NewsEntry
	{
		public NewsEntry(DateTime dateTimeNews, string decodedString) : this()
		{
			PostedUtc = dateTimeNews;
			Message = decodedString.Replace("  ", " ").Trim();
			CollectedUtc = DateTime.UtcNow;
		}

		public NewsEntry(DateTime dateTimeNews, string decodedString, DateTime possibleDateTime) : this(dateTimeNews, decodedString)
		{
			RepairedLineUtc = possibleDateTime;
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
			return
				(new StringBuilder()).AppendLine(PostedLocal.ToString())
					.AppendLine(Message)
					.ToString();
		}

		#endregion

		public string Message { get; set; }

		public DateTime PostedUtc { get; set; }

		public DateTime PostedLocal
		{
			get
			{
				return PostedUtc.ToLocalTime();
			}
		}

		public DateTime CollectedUtc { get; set; }

		public DateTime CollectedLocal
		{
			get
			{
				return CollectedUtc.ToLocalTime();
			}
		}

		public bool IsEmpty
		{
			get { return CollectedUtc == default(DateTime); }
		}

		public DateTime RepairedLineUtc { get; set; }

		public DateTime RepairedLineLocal
		{
			get
			{
				return RepairedLineUtc.ToLocalTime();
			}
		}
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
			if (x.PostedUtc == y.PostedUtc && x.RepairedLineUtc == y.RepairedLineUtc && x.Message == y.Message)
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
