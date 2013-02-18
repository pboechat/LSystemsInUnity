public class Arrays
{
	
	public static int[] Sequence (int min, int max)
	{
		int[] arr = new int[max - min + 1];
		for (int i = min, c = 0; i <= max; i++) {
			arr [c++] = i;
		}
		return arr;
	}
	
	public static T[] New<T> (T initialValue, int size)
	{
		T[] arr = new T[size];
		for (int i = 0; i < size; i++) {
			arr [i] = initialValue;
		}
		return arr;
	}
	
}