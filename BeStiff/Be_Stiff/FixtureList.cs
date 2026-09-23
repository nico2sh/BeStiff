using FarseerPhysics.Dynamics;

namespace Be_Stiff
{
	public class FixtureList
	{
		private int maxNumber;

		private int currentElements;

		private Fixture[] list;

		public Fixture[] Elements => list;

		public int CurrentElements => currentElements;

		public FixtureList(int maxNum)
		{
			list = new Fixture[maxNum];
			currentElements = 0;
			maxNumber = maxNum;
		}

		public void Add(Fixture fixture)
		{
			if (currentElements < maxNumber)
			{
				list[currentElements] = fixture;
				currentElements++;
			}
		}

		public void Clear()
		{
			currentElements = 0;
		}
	}
}
