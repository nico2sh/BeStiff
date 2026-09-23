namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// Contains filter data that can determine whether an object should be processed or not.
	/// </summary>
	public abstract class FilterData
	{
		public Category DisabledOnCategories;

		public int DisabledOnGroup;

		public Category EnabledOnCategories = Category.All;

		public int EnabledOnGroup;

		public virtual bool IsActiveOn(Body body)
		{
			if (body == null || !body.Enabled || body.IsStatic)
			{
				return false;
			}
			if (body.FixtureList == null)
			{
				return false;
			}
			foreach (Fixture fixture in body.FixtureList)
			{
				if (fixture.CollisionGroup == DisabledOnGroup && fixture.CollisionGroup != 0 && DisabledOnGroup != 0)
				{
					return false;
				}
				if ((fixture.CollisionCategories & DisabledOnCategories) != Category.None)
				{
					return false;
				}
				if (EnabledOnGroup != 0 || EnabledOnCategories != Category.All)
				{
					if (fixture.CollisionGroup == EnabledOnGroup && fixture.CollisionGroup != 0 && EnabledOnGroup != 0)
					{
						return true;
					}
					if ((fixture.CollisionCategories & EnabledOnCategories) != Category.None && EnabledOnCategories != Category.All)
					{
						return true;
					}
					continue;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Adds the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void AddDisabledCategory(Category category)
		{
			DisabledOnCategories |= category;
		}

		/// <summary>
		/// Removes the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void RemoveDisabledCategory(Category category)
		{
			DisabledOnCategories &= ~category;
		}

		/// <summary>
		/// Determines whether this body ignores the the specified controller.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <returns>
		/// 	<c>true</c> if the object has the specified category; otherwise, <c>false</c>.
		/// </returns>
		public bool IsInDisabledCategory(Category category)
		{
			return (DisabledOnCategories & category) == category;
		}

		/// <summary>
		/// Adds the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void AddEnabledCategory(Category category)
		{
			EnabledOnCategories |= category;
		}

		/// <summary>
		/// Removes the category.
		/// </summary>
		/// <param name="category">The category.</param>
		public void RemoveEnabledCategory(Category category)
		{
			EnabledOnCategories &= ~category;
		}

		/// <summary>
		/// Determines whether this body ignores the the specified controller.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <returns>
		/// 	<c>true</c> if the object has the specified category; otherwise, <c>false</c>.
		/// </returns>
		public bool IsInEnabledCategory(Category category)
		{
			return (EnabledOnCategories & category) == category;
		}
	}
}
