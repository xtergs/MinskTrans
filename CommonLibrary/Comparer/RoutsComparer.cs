using System.Collections.Generic;
using MinskTrans.Context.Base.BaseModel;

namespace CommonLibrary.Comparer
{
	public class RoutsComparer:IEqualityComparer<Rout>
	{
		#region Implementation of IEqualityComparer<in Rout>

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		/// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
		public bool Equals(Rout x, Rout y)
		{
			if (x.Transport == y.Transport && x.RouteNum == y.RouteNum)
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
		public int GetHashCode(Rout obj)
		{
			return obj.RouteNum.GetHashCode();
		}

		#endregion
	}
}
