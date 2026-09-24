using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nuclex.Support.Parsing;

/// <summary>Parses and stores an application's command line parameters</summary>
/// <remarks>
///   <para>
///     At the time of the creation of this component, there are already several command
///     line parsing libraries out there. Most of them, however, do way too much at once
///     or at the very least rely on huge, untested clutters of classes and methods to
///     arrive at their results.
///   </para>
///   <para>
///     This command line parser does nothing more than represent the command line to
///     the application through a convenient interface. It parses a command line and
///     extracts the arguments, but doesn't interpret them and or check them for validity.
///   </para>
///   <para>
///     This design promotes simplicity and makes is an ideal building block to create
///     actual command line interpreters that connect the parameters to program
///     instructions and or fill structures in code.
///   </para>
///   <para>
///     Terminology
///     <list type="table">
///       <item>
///         <term>Command line</term>
///         <description>
///           The entire command line either as a string or as
///           an already parsed data structure
///         </description>
///       </item>
///       <item>
///         <term>Argument</term>
///         <description>
///           Either an option or a loose value (see below) being specified on
///           the command line
///         </description>
///       </item>
///       <item>
///         <term>Option</term>
///         <description>
///           Can be specified on the command line and typically alters the behavior
///           of the application or changes a setting. For example, '--normalize' or
///           '/safemode'.
///         </description>
///       </item>
///       <item>
///         <term>Value</term>
///         <description>
///           Can either sit loosely in the command line (eg. 'update' or 'textfile.txt')
///           or as assignment to an option (eg. '--width=1280' or '/overwrite:always')
///         </description>
///       </item>
///     </list>
///   </para>
/// </remarks>
public class CommandLine
{
	/// <summary>Argument being specified on an application's command line</summary>
	public class Argument
	{
		/// <summary>
		///   Contains the entire option as it was specified on the command line
		/// </summary>
		private StringSegment raw;

		/// <summary>Absolute index in the raw string the option name starts at</summary>
		private int nameStart;

		/// <summary>Number of characters in the option name</summary>
		private int nameLength;

		/// <summary>Absolute index in the raw string the value starts at</summary>
		private int valueStart;

		/// <summary>Number of characters in the value</summary>
		private int valueLength;

		/// <summary>Contains the raw string the command line argument was parsed from</summary>
		public string Raw => raw.ToString();

		/// <summary>Characters used to initiate this option</summary>
		public string Initiator
		{
			get
			{
				if (nameStart == -1)
				{
					return null;
				}
				return raw.Text.Substring(raw.Offset, nameStart - raw.Offset);
			}
		}

		/// <summary>Name of the command line option</summary>
		public string Name
		{
			get
			{
				if (nameStart == -1)
				{
					return null;
				}
				return raw.Text.Substring(nameStart, nameLength);
			}
		}

		/// <summary>Characters used to associate a value to this option</summary>
		public string Associator
		{
			get
			{
				if (nameStart == -1)
				{
					return null;
				}
				int num = nameStart + nameLength;
				if (valueStart == -1)
				{
					if (raw.Offset + raw.Count - num == 0)
					{
						return null;
					}
				}
				else if (valueStart == num)
				{
					return null;
				}
				return raw.Text.Substring(num, 1);
			}
		}

		/// <summary>Name of the command line option</summary>
		public string Value
		{
			get
			{
				if (valueStart == -1)
				{
					return null;
				}
				return raw.Text.Substring(valueStart, valueLength);
			}
		}

		/// <summary>The raw length of the command line argument</summary>
		internal int RawLength => raw.Count;

		/// <summary>Initializes a new option with only a name</summary>
		/// <param name="raw">
		///   String segment with the entire argument as it was given on the command line
		/// </param>
		/// <param name="nameStart">Absolute index the argument name starts at</param>
		/// <param name="nameLength">Number of characters in the option name</param>
		/// <returns>The newly created option</returns>
		internal static Argument OptionOnly(StringSegment raw, int nameStart, int nameLength)
		{
			return new Argument(raw, nameStart, nameLength, -1, -1);
		}

		/// <summary>Initializes a new argument with only a value</summary>
		/// <param name="raw">
		///   String segment with the entire argument as it was given on the command line
		/// </param>
		/// <param name="valueStart">Absolute index the value starts at</param>
		/// <param name="valueLength">Number of characters in the value</param>
		/// <returns>The newly created option</returns>
		internal static Argument ValueOnly(StringSegment raw, int valueStart, int valueLength)
		{
			return new Argument(raw, -1, -1, valueStart, valueLength);
		}

		/// <summary>Creates a new option with a name and an assigned value</summary>
		/// <param name="raw">
		///   String segment containing the entire option as it was given on the command line
		/// </param>
		/// <param name="nameStart">Absolute index the option name starts at</param>
		/// <param name="nameLength">Number of characters in the option name</param>
		/// <param name="valueStart">Absolute index the value starts at</param>
		/// <param name="valueLength">Number of characters in the value</param>
		/// <returns>The newly created option</returns>
		internal Argument(StringSegment raw, int nameStart, int nameLength, int valueStart, int valueLength)
		{
			this.raw = raw;
			this.nameStart = nameStart;
			this.nameLength = nameLength;
			this.valueStart = valueStart;
			this.valueLength = valueLength;
		}
	}

	/// <summary>Formats a command line instance into a string</summary>
	internal static class Formatter
	{
		/// <summary>
		///   Formats all arguments in the provided command line instance into a string
		/// </summary>
		/// <param name="commandLine">Command line instance that will be formatted</param>
		/// <returns>All arguments in the command line instance as a string</returns>
		public static string FormatCommandLine(CommandLine commandLine)
		{
			int num = 0;
			for (int i = 0; i < commandLine.arguments.Count; i++)
			{
				if (i != 0)
				{
					num++;
				}
				num += commandLine.arguments[i].RawLength;
			}
			StringBuilder stringBuilder = new StringBuilder(num);
			for (int j = 0; j < commandLine.arguments.Count; j++)
			{
				if (j != 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(commandLine.arguments[j].Raw);
			}
			return stringBuilder.ToString();
		}
	}

	/// <summary>Parses command line strings</summary>
	private class Parser
	{
		/// <summary>Characters which end an option name when they are encountered</summary>
		private static readonly char[] NameEndingCharacters = new char[5] { ' ', '\t', '=', ':', '"' };

		/// <summary>Characters the parser considers to be whitespace</summary>
		private static readonly char[] WhitespaceCharacters = new char[2] { ' ', '\t' };

		/// <summary>Argument list being filled by the parser</summary>
		private List<Argument> arguments;

		/// <summary>Whether the '/' character initiates an argument</summary>
		private bool windowsMode;

		/// <summary>Initializes a new command line parser</summary>
		/// <param name="windowsMode">Whether the / character initiates an argument</param>
		private Parser(bool windowsMode)
		{
			this.windowsMode = windowsMode;
			arguments = new List<Argument>();
		}

		/// <summary>Parses a string containing command line arguments</summary>
		/// <param name="commandLineString">String that will be parsed</param>
		/// <param name="windowsMode">Whether the / character initiates an argument</param>
		/// <returns>The parsed command line arguments from the string</returns>
		public static List<Argument> Parse(string commandLineString, bool windowsMode)
		{
			Parser parser = new Parser(windowsMode);
			parser.parseFullCommandLine(commandLineString);
			return parser.arguments;
		}

		/// <summary>
		///   Parses the provided string and adds the parameters found to
		///   the command line representation
		/// </summary>
		/// <param name="commandLineString">
		///   String containing the command line arguments that will be parsed
		/// </param>
		private void parseFullCommandLine(string commandLineString)
		{
			if (commandLineString == null)
			{
				return;
			}
			int index = 0;
			while (index < commandLineString.Length)
			{
				index = commandLineString.IndexNotOfAny(WhitespaceCharacters, index);
				if (index == -1)
				{
					break;
				}
				parseChunk(commandLineString, ref index);
			}
		}

		/// <summary>
		///   Parses a chunk of characters and adds it as an option or a loose value to
		///   the command line representation we're building
		/// </summary>
		/// <param name="commandLineString">
		///   String containing the chunk of characters that will be parsed
		/// </param>
		/// <param name="index">Index in the string at which to begin parsing</param>
		/// <returns>The number of characters that were consumed</returns>
		private void parseChunk(string commandLineString, ref int index)
		{
			int num = index;
			switch (commandLineString[index])
			{
			case '-':
				index++;
				if (index >= commandLineString.Length)
				{
					addValue(new StringSegment(commandLineString, num, 1));
					return;
				}
				if (commandLineString[index] == '-')
				{
					index++;
				}
				parsePotentialOption(commandLineString, num, ref index);
				return;
			case '/':
				if (windowsMode)
				{
					index++;
					parsePotentialOption(commandLineString, num, ref index);
					return;
				}
				break;
			case '"':
				parseQuotedValue(commandLineString, ref index);
				return;
			}
			parseNakedValue(commandLineString, ref index);
		}

		/// <summary>Parses a potential command line option</summary>
		/// <param name="commandLineString">String containing the command line arguments</param>
		/// <param name="initiatorStartIndex">
		///   Index of the option's initiator ('-' or '--' or '/')
		/// </param>
		/// <param name="index">
		///   Index at which the option name is supposed start (if it's an actual option)
		/// </param>
		/// <returns>The number of characters consumed</returns>
		private void parsePotentialOption(string commandLineString, int initiatorStartIndex, ref int index)
		{
			if (index == commandLineString.Length)
			{
				addValue(new StringSegment(commandLineString, initiatorStartIndex, commandLineString.Length - initiatorStartIndex));
				return;
			}
			int num = index;
			if (commandLineString[index] != commandLineString[initiatorStartIndex])
			{
				index = commandLineString.IndexOfAny(NameEndingCharacters, num);
				if (index == -1)
				{
					index = commandLineString.Length;
				}
			}
			if (index == num)
			{
				index = commandLineString.IndexOfAny(WhitespaceCharacters, index);
				if (index == -1)
				{
					index = commandLineString.Length;
				}
				addValue(new StringSegment(commandLineString, initiatorStartIndex, index - initiatorStartIndex));
			}
			else
			{
				parsePotentialOptionAssignment(commandLineString, initiatorStartIndex, num, ref index);
			}
		}

		/// <summary>Parses the value assignment in a command line option</summary>
		/// <param name="commandLineString">String containing the command line arguments</param>
		/// <param name="initiatorStartIndex">
		///   Position of the character that started the option
		/// </param>
		/// <param name="nameStartIndex">
		///   Position of the first character in the option's name
		/// </param>
		/// <param name="index">Index at which the option name ended</param>
		private void parsePotentialOptionAssignment(string commandLineString, int initiatorStartIndex, int nameStartIndex, ref int index)
		{
			int num = index;
			if (index < commandLineString.Length && isAssignmentCharacter(commandLineString[index]))
			{
				index++;
				parseOptionValue(commandLineString, initiatorStartIndex, nameStartIndex, ref index);
				return;
			}
			int num2;
			int num3;
			if (commandLineString[index - 1] == '+' || commandLineString[index - 1] == '-')
			{
				num2 = index - 1;
				num3 = index;
				num--;
			}
			else
			{
				num2 = -1;
				num3 = -1;
			}
			int count = index - initiatorStartIndex;
			arguments.Add(new Argument(new StringSegment(commandLineString, initiatorStartIndex, count), nameStartIndex, num - nameStartIndex, num2, num3 - num2));
		}

		/// <summary>Parses the value assignment in a command line option</summary>
		/// <param name="commandLineString">String containing the command line arguments</param>
		/// <param name="initiatorStartIndex">
		///   Position of the character that started the option
		/// </param>
		/// <param name="nameStartIndex">
		///   Position of the first character in the option's name
		/// </param>
		/// <param name="index">Index at which the option name ended</param>
		private void parseOptionValue(string commandLineString, int initiatorStartIndex, int nameStartIndex, ref int index)
		{
			int num = index - 1;
			int num2;
			int num3;
			if (index == commandLineString.Length)
			{
				num2 = -1;
				num3 = -1;
			}
			else
			{
				char c = commandLineString[index];
				if (c == '"')
				{
					index++;
					num2 = index;
					index = commandLineString.IndexOf('"', index);
					if (index == -1)
					{
						index = commandLineString.Length;
						num3 = index;
					}
					else
					{
						num3 = index;
						index++;
					}
				}
				else
				{
					num2 = index;
					index = commandLineString.IndexOfAny(WhitespaceCharacters, index);
					if (index == -1)
					{
						index = commandLineString.Length;
						num3 = index;
					}
					else if (index == num2)
					{
						num2 = -1;
						num3 = -1;
					}
					else
					{
						num3 = index;
					}
				}
			}
			int count = index - initiatorStartIndex;
			arguments.Add(new Argument(new StringSegment(commandLineString, initiatorStartIndex, count), nameStartIndex, num - nameStartIndex, num2, num3 - num2));
		}

		/// <summary>Parses a quoted value from the input string</summary>
		/// <param name="commandLineString">String the quoted value is parsed from</param>
		/// <param name="index">Index at which the quoted value begins</param>
		private void parseQuotedValue(string commandLineString, ref int index)
		{
			int num = index;
			char value = commandLineString[index];
			int num2 = num + 1;
			index = commandLineString.IndexOf(value, num2);
			if (index == -1)
			{
				index = commandLineString.Length;
				arguments.Add(Argument.ValueOnly(new StringSegment(commandLineString, num, index - num), num2, index - num2));
			}
			else
			{
				arguments.Add(Argument.ValueOnly(new StringSegment(commandLineString, num, index - num + 1), num2, index - num2));
				index++;
			}
		}

		/// <summary>Parses a plain, unquoted value from the input string</summary>
		/// <param name="commandLineString">String containing the value to be parsed</param>
		/// <param name="index">Index at which the value begins</param>
		private void parseNakedValue(string commandLineString, ref int index)
		{
			int num = index;
			index = commandLineString.IndexOfAny(WhitespaceCharacters, index);
			if (index == -1)
			{
				index = commandLineString.Length;
			}
			addValue(new StringSegment(commandLineString, num, index - num));
		}

		/// <summary>Adds a loose value to the command line</summary>
		/// <param name="value">Value taht will be added</param>
		private void addValue(StringSegment value)
		{
			arguments.Add(Argument.ValueOnly(value, value.Offset, value.Count));
		}

		/// <summary>
		///   Determines whether the specified character indicates an assignment
		/// </summary>
		/// <param name="character">
		///   Character that will be checked for being an assignemnt
		/// </param>
		/// <returns>
		///   True if the specified character indicated an assignment, otherwise false
		/// </returns>
		private static bool isAssignmentCharacter(char character)
		{
			if (character != ':')
			{
				return character == '=';
			}
			return true;
		}
	}

	/// <summary>
	///   Whether the command line should use Windows mode by default
	/// </summary>
	public static readonly bool WindowsModeDefault = Path.DirectorySeparatorChar == '\\';

	/// <summary>Options that were specified on the command line</summary>
	private List<Argument> arguments;

	/// <summary>Whether the / character initiates an argument</summary>
	private bool windowsMode;

	/// <summary>Options that were specified on the command line</summary>
	public IList<Argument> Arguments => arguments;

	/// <summary>Initializes a new command line</summary>
	public CommandLine()
		: this(new List<Argument>(), WindowsModeDefault)
	{
	}

	/// <summary>Initializes a new command line</summary>
	/// <param name="windowsMode">Whether the / character initiates an argument</param>
	public CommandLine(bool windowsMode)
		: this(new List<Argument>(), windowsMode)
	{
	}

	/// <summary>Initializes a new command line</summary>
	/// <param name="argumentList">List containing the parsed arguments</param>
	private CommandLine(List<Argument> argumentList)
		: this(argumentList, WindowsModeDefault)
	{
	}

	/// <summary>Initializes a new command line</summary>
	/// <param name="argumentList">List containing the parsed arguments</param>
	/// <param name="windowsMode">Whether the / character initiates an argument</param>
	private CommandLine(List<Argument> argumentList, bool windowsMode)
	{
		arguments = argumentList;
		this.windowsMode = windowsMode;
	}

	/// <summary>Parses the command line arguments from the provided string</summary>
	/// <param name="commandLineString">String containing the command line arguments</param>
	/// <returns>The parsed command line</returns>
	/// <remarks>
	///   You should always pass Environment.CommandLine to this method to avoid
	///   some problems with the built-in command line tokenizer in .NET
	///   (which splits '--test"hello world"/v' into '--testhello world/v')
	/// </remarks>
	public static CommandLine Parse(string commandLineString)
	{
		bool flag = Path.DirectorySeparatorChar != '/';
		return Parse(commandLineString, flag);
	}

	/// <summary>Parses the command line arguments from the provided string</summary>
	/// <param name="commandLineString">String containing the command line arguments</param>
	/// <param name="windowsMode">Whether the / character initiates an argument</param>
	/// <returns>The parsed command line</returns>
	/// <remarks>
	///   You should always pass Environment.CommandLine to this methods to avoid
	///   some problems with the built-in command line tokenizer in .NET
	///   (which splits '--test"hello world"/v' into '--testhello world/v')
	/// </remarks>
	public static CommandLine Parse(string commandLineString, bool windowsMode)
	{
		return new CommandLine(Parser.Parse(commandLineString, windowsMode));
	}

	/// <summary>Returns whether an argument with the specified name exists</summary>
	/// <param name="name">Name of the argument whose existence will be checked</param>
	/// <returns>True if an argument with the specified name exists</returns>
	public bool HasArgument(string name)
	{
		return indexOfArgument(name) != -1;
	}

	/// <summary>Adds a value to the command line</summary>
	/// <param name="value">Value that will be added</param>
	public void AddValue(string value)
	{
		int num = value?.Length ?? 0;
		if (requiresQuotes(value))
		{
			StringBuilder stringBuilder = new StringBuilder(num + 2);
			stringBuilder.Append('"');
			stringBuilder.Append(value);
			stringBuilder.Append('"');
			arguments.Add(Argument.ValueOnly(new StringSegment(stringBuilder.ToString(), 0, num + 2), 1, num));
		}
		else
		{
			arguments.Add(Argument.ValueOnly(new StringSegment(value), 0, num));
		}
	}

	/// <summary>Adds an option to the command line</summary>
	/// <param name="name">Name of the option that will be added</param>
	public void AddOption(string name)
	{
		AddOption("-", name);
	}

	/// <summary>Adds an option to the command line</summary>
	/// <param name="initiator">Initiator that will be used to start the option</param>
	/// <param name="name">Name of the option that will be added</param>
	public void AddOption(string initiator, string name)
	{
		StringBuilder stringBuilder = new StringBuilder(initiator.Length + name.Length);
		stringBuilder.Append(initiator);
		stringBuilder.Append(name);
		arguments.Add(Argument.OptionOnly(new StringSegment(stringBuilder.ToString()), initiator.Length, name.Length));
	}

	/// <summary>Adds an option with an assignment to the command line</summary>
	/// <param name="name">Name of the option that will be added</param>
	/// <param name="value">Value that will be assigned to the option</param>
	public void AddAssignment(string name, string value)
	{
		AddAssignment("-", name, value);
	}

	/// <summary>Adds an option with an assignment to the command line</summary>
	/// <param name="initiator">Initiator that will be used to start the option</param>
	/// <param name="name">Name of the option that will be added</param>
	/// <param name="value">Value that will be assigned to the option</param>
	public void AddAssignment(string initiator, string name, string value)
	{
		bool flag = containsWhitespace(value);
		StringBuilder stringBuilder = new StringBuilder(initiator.Length + name.Length + 1 + value.Length + (flag ? 2 : 0));
		stringBuilder.Append(initiator);
		stringBuilder.Append(name);
		stringBuilder.Append('=');
		if (flag)
		{
			stringBuilder.Append('"');
			stringBuilder.Append(value);
			stringBuilder.Append('"');
		}
		else
		{
			stringBuilder.Append(value);
		}
		arguments.Add(new Argument(new StringSegment(stringBuilder.ToString()), initiator.Length, name.Length, initiator.Length + name.Length + 1 + (flag ? 1 : 0), value.Length));
	}

	/// <summary>Returns a string that contains the entire command line</summary>
	/// <returns>The entire command line as a single string</returns>
	public override string ToString()
	{
		return Formatter.FormatCommandLine(this);
	}

	/// <summary>Retrieves the index of the argument with the specified name</summary>
	/// <param name="name">Name of the argument whose index will be returned</param>
	/// <returns>
	///   The index of the indicated argument of -1 if no argument with that name exists
	/// </returns>
	private int indexOfArgument(string name)
	{
		for (int i = 0; i < arguments.Count; i++)
		{
			if (arguments[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	///   Determines whether the string requires quotes to survive the command line
	/// </summary>
	/// <param name="value">Value that will be checked for requiring quotes</param>
	/// <returns>True if the value requires quotes to survive the command line</returns>
	private bool requiresQuotes(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return true;
		}
		bool flag = containsWhitespace(value) || value[0] == '-';
		if (windowsMode)
		{
			flag |= value[0] == '/';
		}
		return flag;
	}

	/// <summary>
	///   Determines whether the string contains any whitespace characters
	/// </summary>
	/// <param name="value">String that will be scanned for whitespace characters</param>
	/// <returns>True if the provided string contains whitespace characters</returns>
	private static bool containsWhitespace(string value)
	{
		if (value.IndexOf(' ') == -1)
		{
			return value.IndexOf('\t') != -1;
		}
		return true;
	}
}
