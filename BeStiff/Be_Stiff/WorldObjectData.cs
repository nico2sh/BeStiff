namespace Be_Stiff
{
	public class WorldObjectData
	{
		private WorldObjectType type;

		private WorldObject wObject;

		public WorldObjectType Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}

		public WorldObject Object => wObject;

		public WorldObjectData(WorldObjectType type, WorldObject wObject)
		{
			this.type = type;
			this.wObject = wObject;
		}
	}
}
