using System.Text;

namespace Be_Stiff.Utils
{
	internal static class StringBuilderExtensions
	{
		private static readonly char[] numberBuffer = new char[10];

		/// <summary>
		/// Appends an integer, zero-padded to <paramref name="minDigits"/>, without allocating.
		/// </summary>
		public static StringBuilder AppendNumber(this StringBuilder stringBuilder, int number, int minDigits)
		{
			if (number < 0)
			{
				stringBuilder.Append('-');
				number = -number;
			}
			int num = 0;
			do
			{
				numberBuffer[num] = (char)('0' + number % 10);
				number /= 10;
				num++;
			}
			while (number > 0 || num < minDigits);
			for (num--; num >= 0; num--)
			{
				stringBuilder.Append(numberBuffer[num]);
			}
			return stringBuilder;
		}
	}
}
