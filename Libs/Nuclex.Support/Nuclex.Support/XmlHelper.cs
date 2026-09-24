using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Nuclex.Support;

/// <summary>Helper routines for handling XML code</summary>
public static class XmlHelper
{
	/// <summary>Handles any events occurring when an XML schema is loaded</summary>
	private class ValidationEventProcessor
	{
		/// <summary>Exception that has occurred during schema loading</summary>
		public Exception OccurredException;

		/// <summary>
		///   Callback for notifications being sent by the XmlSchema.Read() method
		/// </summary>
		/// <param name="sender">Not used</param>
		/// <param name="arguments">Contains the notification being sent</param>
		public void OnValidationEvent(object sender, ValidationEventArgs arguments)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (OccurredException == null && (int)arguments.Severity == 0)
			{
				OccurredException = ((arguments.Exception != null) ? ((Exception)(object)arguments.Exception) : ((Exception)new XmlSchemaValidationException("Unspecified schema validation error")));
			}
		}
	}

	/// <summary>Loads a schema from a file</summary>
	/// <param name="schemaPath">Path to the file containing the schema</param>
	/// <returns>The loaded schema</returns>
	public static XmlSchema LoadSchema(string schemaPath)
	{
		using FileStream schemaStream = openFileForSharedReading(schemaPath);
		return LoadSchema(schemaStream);
	}

	/// <summary>Loads a schema from the provided stream</summary>
	/// <param name="schemaStream">Stream containing the schema that will be loaded</param>
	/// <returns>The loaded schema</returns>
	public static XmlSchema LoadSchema(Stream schemaStream)
	{
		return LoadSchema(new StreamReader(schemaStream));
	}

	/// <summary>Loads a schema from a text reader</summary>
	/// <param name="schemaReader">Text reader through which the schema can be read</param>
	/// <returns>The loaded schema</returns>
	public static XmlSchema LoadSchema(TextReader schemaReader)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		ValidationEventProcessor validationEventProcessor = new ValidationEventProcessor();
		XmlSchema result = XmlSchema.Read(schemaReader, new ValidationEventHandler(validationEventProcessor.OnValidationEvent));
		if (validationEventProcessor.OccurredException != null)
		{
			throw validationEventProcessor.OccurredException;
		}
		return result;
	}

	/// <summary>Attempts to load a schema from a file</summary>
	/// <param name="schemaPath">Path to the file containing the schema</param>
	/// <param name="schema">Output parameter that will receive the loaded schema</param>
	/// <returns>True if the schema was loaded successfully, otherwise false</returns>
	public static bool TryLoadSchema(string schemaPath, out XmlSchema schema)
	{
		if (!tryOpenFileForSharedReading(schemaPath, out var fileStream))
		{
			schema = null;
			return false;
		}
		using (fileStream)
		{
			return TryLoadSchema(fileStream, out schema);
		}
	}

	/// <summary>Attempts to load a schema from the provided stream</summary>
	/// <param name="schemaStream">Stream containing the schema that will be loaded</param>
	/// <param name="schema">Output parameter that will receive the loaded schema</param>
	/// <returns>True if the schema was loaded successfully, otherwise false</returns>
	public static bool TryLoadSchema(Stream schemaStream, out XmlSchema schema)
	{
		return TryLoadSchema(new StreamReader(schemaStream), out schema);
	}

	/// <summary>Attempts to load a schema from the provided text reader</summary>
	/// <param name="schemaReader">Reader from which the schema can be read</param>
	/// <param name="schema">Output parameter that will receive the loaded schema</param>
	/// <returns>True if the schema was loaded successfully, otherwise false</returns>
	public static bool TryLoadSchema(TextReader schemaReader, out XmlSchema schema)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		try
		{
			ValidationEventProcessor validationEventProcessor = new ValidationEventProcessor();
			schema = XmlSchema.Read(schemaReader, new ValidationEventHandler(validationEventProcessor.OnValidationEvent));
			if (validationEventProcessor.OccurredException == null)
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		schema = null;
		return false;
	}

	/// <summary>Loads an XML document from a file</summary>
	/// <param name="schema">Schema to use for validating the XML document</param>
	/// <param name="documentPath">
	///   Path to the file containing the XML document that will be loaded
	/// </param>
	/// <returns>The loaded XML document</returns>
	public static XDocument LoadDocument(XmlSchema schema, string documentPath)
	{
		using FileStream documentStream = openFileForSharedReading(documentPath);
		return LoadDocument(schema, documentStream);
	}

	/// <summary>Loads an XML document from a stream</summary>
	/// <param name="schema">Schema to use for validating the XML document</param>
	/// <param name="documentStream">
	///   Stream from which the XML document will be read
	/// </param>
	/// <returns>The loaded XML document</returns>
	public static XDocument LoadDocument(XmlSchema schema, Stream documentStream)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		XmlReaderSettings val = new XmlReaderSettings();
		val.Schemas.Add(schema);
		XmlReader val2 = XmlReader.Create(documentStream, val);
		try
		{
			XDocument val3 = XDocument.Load(val2, (LoadOptions)0);
			XmlSchemaSet val4 = new XmlSchemaSet();
			val4.Add(schema);
			ValidationEventProcessor validationEventProcessor = new ValidationEventProcessor();
			System.Xml.Schema.Extensions.Validate(val3, val4, new ValidationEventHandler(validationEventProcessor.OnValidationEvent));
			if (validationEventProcessor.OccurredException != null)
			{
				throw validationEventProcessor.OccurredException;
			}
			return val3;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	/// <summary>Opens a file for shared reading</summary>
	/// <param name="path">Path to the file that will be opened</param>
	/// <returns>The opened file's stream</returns>
	private static FileStream openFileForSharedReading(string path)
	{
		return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	/// <summary>Opens a file for shared reading</summary>
	/// <param name="path">Path to the file that will be opened</param>
	/// <param name="fileStream">
	///   Output parameter that receives the opened file's stream
	/// </param>
	/// <returns>True if the file was opened successfully</returns>
	private static bool tryOpenFileForSharedReading(string path, out FileStream fileStream)
	{
		try
		{
			fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			return true;
		}
		catch (Exception)
		{
		}
		fileStream = null;
		return false;
	}
}
