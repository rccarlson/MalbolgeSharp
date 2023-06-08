using System;
using System.Buffers;

namespace Malbolge;

public static class Utility
{
	public static (T[], int) ToRentedArray<T>(this IEnumerable<T> enumerable, int initialSize = 4)
	{
		var pool = ArrayPool<T>.Shared;

		int size = initialSize;
		int i = 0;
		T[] arr = pool.Rent(size);
		foreach(var item in enumerable)
		{
			if (i < size)
			{
				// array can handle
				arr[i++] = item;
			}
			else
			{
				// array too small
				T[] newArr = pool.Rent(size * 2);
				Array.Copy(arr, newArr, size);
				pool.Return(arr);
				arr = newArr;
				size = newArr.Length;
				arr[i++] = item;
			}
		}
		return (arr, i);
	}

	public static int IndexOf(this string str, char c)
	{
		for(int i=0; i < str.Length; i++)
		{
			if (str[i] == c) return i;
		}
		return -1;
	}
	public static int IndexOf<T>(this T[] array, T item)
	{
		if(array is null) return -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i]?.Equals(item) ?? false) return i;
		}
		return -1;
	}
}
