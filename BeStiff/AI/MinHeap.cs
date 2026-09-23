using System;

namespace Be_Stiff.AI
{
	public class MinHeap<T> where T : IComparable<T>
	{
		private int count;

		private int capacity;

		private T temp;

		private T mheap;

		private T[] array;

		private T[] tempArray;

		public int Count => count;

		public MinHeap()
			: this(16)
		{
		}

		public MinHeap(int capacity)
		{
			count = 0;
			this.capacity = capacity;
			array = new T[capacity];
		}

		public void Clear()
		{
			count = 0;
		}

		public void BuildHead()
		{
			for (int num = count - 1 >> 1; num >= 0; num--)
			{
				MinHeapify(num);
			}
		}

		public void Add(T item)
		{
			count++;
			if (count > capacity)
			{
				DoubleArray();
			}
			array[count - 1] = item;
			int num = count - 1;
			int num2 = num - 1 >> 1;
			while (num > 0)
			{
				ref readonly T reference = ref array[num2];
				T other = array[num];
				if (reference.CompareTo(other) > 0)
				{
					temp = array[num];
					array[num] = array[num2];
					array[num2] = temp;
					num = num2;
					num2 = num - 1 >> 1;
					continue;
				}
				break;
			}
		}

		private void DoubleArray()
		{
			capacity <<= 1;
			tempArray = new T[capacity];
			CopyArray(array, tempArray);
			array = tempArray;
		}

		private static void CopyArray(T[] source, T[] destination)
		{
			for (int i = 0; i < source.Length; i++)
			{
				destination[i] = source[i];
			}
		}

		public T Peek()
		{
			if (count == 0)
			{
				throw new InvalidOperationException("Heap is empty");
			}
			return array[0];
		}

		public T ExtractFirst()
		{
			if (count == 0)
			{
				throw new InvalidOperationException("Heap is empty");
			}
			temp = array[0];
			array[0] = array[count - 1];
			count--;
			MinHeapify(0);
			return temp;
		}

		private void MinHeapify(int position)
		{
			while (true)
			{
				int num = (position << 1) + 1;
				int num2 = num + 1;
				int num3;
				if (num < count)
				{
					ref readonly T reference = ref array[num];
					T other = array[position];
					if (reference.CompareTo(other) < 0)
					{
						num3 = num;
						goto IL_0041;
					}
				}
				num3 = position;
				goto IL_0041;
				IL_0041:
				if (num2 < count)
				{
					ref readonly T reference2 = ref array[num2];
					T other2 = array[num3];
					if (reference2.CompareTo(other2) < 0)
					{
						num3 = num2;
					}
				}
				if (num3 != position)
				{
					mheap = array[position];
					array[position] = array[num3];
					array[num3] = mheap;
					position = num3;
					continue;
				}
				break;
			}
		}
	}
}
