using System.Collections.ObjectModel;

namespace HangmanGame.Utils
{
	public static class CollectionExtensions
	{
		public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
		{
			return new ObservableCollection<T>(source);
		}
	}
}
