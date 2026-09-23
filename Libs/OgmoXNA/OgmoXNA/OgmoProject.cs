using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using OgmoXNA.Layers.Settings;
using OgmoXNA.Values;

namespace OgmoXNA
{
	/// <summary>
	/// Contains data about an Ogmo Editor project.
	/// </summary>
	public class OgmoProject
	{
		private Dictionary<string, OgmoLayerSettings> layerSettings = new Dictionary<string, OgmoLayerSettings>();

		private Dictionary<string, OgmoObjectTemplate> objects = new Dictionary<string, OgmoObjectTemplate>();

		private Dictionary<string, OgmoTileset> tilesets = new Dictionary<string, OgmoTileset>();

		private Dictionary<string, OgmoValueTemplate> values = new Dictionary<string, OgmoValueTemplate>();

		/// <summary>
		/// Gets the project layer settings.
		/// </summary>
		public OgmoLayerSettings[] LayerSettings => layerSettings.Values.ToArray();

		/// <summary>
		/// Gets the name of the project.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the project objects.
		/// </summary>
		public OgmoObjectTemplate[] ObjectTemplates => objects.Values.ToArray();

		/// <summary>
		/// Gets the settings for the project.
		/// </summary>
		public OgmoProjectSettings Settings { get; private set; }

		/// <summary>
		/// Gets the project tilesets.
		/// </summary>
		public OgmoTileset[] Tilesets => tilesets.Values.ToArray();

		/// <summary>
		/// Gets the project values.
		/// </summary>
		public OgmoValueTemplate[] ValueTemplates => values.Values.ToArray();

		internal OgmoProject(ContentReader reader)
		{
			Name = reader.ReadString();
			Settings = new OgmoProjectSettings(reader);
			int num = reader.ReadInt32();
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					OgmoValueTemplate ogmoValueTemplate = OgmoValueTemplateReader.Read(reader);
					if (ogmoValueTemplate != null)
					{
						values.Add(ogmoValueTemplate.Name, ogmoValueTemplate);
					}
				}
			}
			int num2 = reader.ReadInt32();
			if (num2 > 0)
			{
				for (int j = 0; j < num2; j++)
				{
					OgmoTileset ogmoTileset = new OgmoTileset(reader);
					if (ogmoTileset != null)
					{
						tilesets.Add(ogmoTileset.Name, ogmoTileset);
					}
				}
			}
			int num3 = reader.ReadInt32();
			if (num3 > 0)
			{
				for (int k = 0; k < num3; k++)
				{
					OgmoObjectTemplate ogmoObjectTemplate = new OgmoObjectTemplate(reader);
					if (ogmoObjectTemplate != null)
					{
						objects.Add(ogmoObjectTemplate.Name, ogmoObjectTemplate);
					}
				}
			}
			int num4 = reader.ReadInt32();
			if (num4 <= 0)
			{
				return;
			}
			for (int l = 0; l < num4; l++)
			{
				OgmoLayerSettings ogmoLayerSettings = OgmoLayerSettingsReader.Read(reader);
				if (ogmoLayerSettings != null)
				{
					layerSettings.Add(ogmoLayerSettings.Name, ogmoLayerSettings);
				}
			}
		}

		/// <summary>
		/// Gets the specified layer settings.
		/// </summary>
		/// <typeparam name="T">The type of layer settings to retrieve.</typeparam>
		/// <param name="name">The name of the layer settings.</param>
		/// <returns>Returns the specified layer settings if found; otherwise, <c>null</c>.</returns>
		public T GetLayerSettings<T>(string name) where T : OgmoLayerSettings
		{
			OgmoLayerSettings value = null;
			if (layerSettings.TryGetValue(name, out value))
			{
				return value as T;
			}
			return null;
		}

		/// <summary>
		/// Gets the specified object template.
		/// </summary>
		/// <param name="name">The name of the object template.</param>
		/// <returns>Returns the specified object template if found; otherwise, <c>null</c>.</returns>
		public OgmoObjectTemplate GetObjectTemplate(string name)
		{
			OgmoObjectTemplate value = null;
			if (objects.TryGetValue(name, out value))
			{
				return value;
			}
			return null;
		}

		/// <summary>
		/// Gets the specified tileset.
		/// </summary>
		/// <param name="name">The name of the tileset.</param>
		/// <returns>Returns the specified tileset if found; otherwise, <c>null</c>.</returns>
		public OgmoTileset GetTileset(string name)
		{
			OgmoTileset value = null;
			if (tilesets.TryGetValue(name, out value))
			{
				return value;
			}
			return null;
		}

		/// <summary>
		/// Gets the specified project value template.
		/// </summary>
		/// <typeparam name="T">The type of value template to retrieve.</typeparam>
		/// <param name="name">The name of the value template.</param>
		/// <returns>Returns the specified value template if found; otherwise, <c>null</c>.</returns>
		public T GetValueTemplate<T>(string name) where T : OgmoValueTemplate
		{
			OgmoValueTemplate value = null;
			if (values.TryGetValue(name, out value))
			{
				return value as T;
			}
			return null;
		}
	}
}
