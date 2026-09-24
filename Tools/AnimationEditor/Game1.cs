using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nuclex.Input;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using SKAnimation;

namespace AnimationTest;

public class Game1 : Game
{
	private GraphicsDeviceManager graphics;

	private GuiManager gui;

	private InputManager input;

	private SpriteBatch spriteBatch;

	private SpriteFont spriteFont;

	private SpriteFont spriteFontBold;

	private Texture2D backgroundTexture;

	private bool debug;

	private bool drawHBoundingBox;

	private bool drawVBoundingBox;

	private bool drawBackground;

	private RenderTarget2D mainRenderTarget;

	private RenderTarget2D animRenderTarget;

	private Body body;

	private World world;

	private int selectedBone;

	private int selectedAnimation;

	private int selectedKeyFrame;

	private string projectPath;

	private string basePath;

	private string spritesSubPath;

	private string skeletonFile;

	private string animationsFile;

	private Vector2 boundingBox;

	private List<string> pastProjectsPath;

	private ListControl boneList;

	private ListControl animationList;

	private ListControl keyFrameList;

	private WindowControl aew;

	private WindowControl bew;

	private bool aewOpen;

	private bool bewOpen;

	private bool editingAnimation;

	private KeyboardState oldKeyState;

	private BasicEffect basicEffect;

	private BasicEffect basicEffectRT;

	private Matrix view;

	private Vector2 cameraPosition = Vector2.Zero;

	private Skeleton hs;

	private KeyFrame copiedKeyFrame;

	private KeyFrameInfo copiedBoneKeyFrame;

	private Dictionary<string, KeyFrameInfo> copiedBonesKeyFrameList;

	private string copiedBoneNameKeyFrame;

	public Game1()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		graphics = new GraphicsDeviceManager((Game)(object)this);
		input = new InputManager(((Game)this).Services, ((Game)this).Window);
		gui = new GuiManager(((Game)this).Services);
		uiScale = InitialUiScale();
		InputManager.MouseScale = uiScale;
		graphics.PreferredBackBufferWidth = BaseWidth * uiScale;
		graphics.PreferredBackBufferHeight = BaseHeight * uiScale;
		((Game)this).Content.RootDirectory = "Content";
		((Game)this).Window.AllowUserResizing = true;
		((Game)this).Window.ClientSizeChanged += Window_ClientSizeChanged;
		((Collection<IGameComponent>)(object)((Game)this).Components).Add((IGameComponent)(object)input);
		((Collection<IGameComponent>)(object)((Game)this).Components).Add((IGameComponent)(object)gui);
		gui.DrawOrder = 1000;
		bewOpen = false;
		aewOpen = false;
		((Game)this).IsMouseVisible = true;
		((Game)this).IsFixedTimeStep = true;
		((Game)this).TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
		copiedKeyFrame = null;
		copiedBoneKeyFrame = default(KeyFrameInfo);
		copiedBoneNameKeyFrame = "";
		copiedBonesKeyFrameList = null;
	}

	private void Window_ClientSizeChanged(object sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		int num = ((Game)this).Window.ClientBounds.Width;
		int num2 = ((Game)this).Window.ClientBounds.Height;
		if (num <= 10)
		{
			num = 20;
		}
		if (num2 <= 10)
		{
			num2 = 20;
		}
		graphics.PreferredBackBufferWidth = num;
		graphics.PreferredBackBufferHeight = num2;
		graphics.ApplyChanges();
		gui.Screen.Width = LogicalViewport.Width;
		gui.Screen.Height = LogicalViewport.Height;
		RecreateSceneTarget();
		PresentationParameters presentationParameters = graphics.GraphicsDevice.PresentationParameters;
		mainRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, LogicalViewport.Width, LogicalViewport.Height);
		animRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, LogicalViewport.Width, LogicalViewport.Height);
		BasicEffect obj = basicEffect;
		Viewport viewport = LogicalViewport;
		float num3 = -viewport.Width / 2;
		Viewport viewport2 = LogicalViewport;
		float num4 = viewport2.Width / 2;
		Viewport viewport3 = LogicalViewport;
		float num5 = viewport3.Height / 2;
		Viewport viewport4 = LogicalViewport;
		obj.Projection = Matrix.CreateOrthographicOffCenter(num3, num4, num5, (float)(-viewport4.Height / 2), 0f, 1f);
		basicEffectRT.Projection = basicEffect.Projection;
	}

	protected override void Initialize()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		Viewport viewport = LogicalViewport;
		Screen screen = new Screen(viewport.Width, viewport.Height);
		gui.Screen = screen;
		screen.Desktop.Bounds = new UniRectangle(new UniScalar(0f, 0f), new UniScalar(0f, 0f), new UniScalar(1f, 0f), new UniScalar(1f, 0f));
		CreateDesktopControls(screen);
		basicEffect = new BasicEffect(((Game)this).GraphicsDevice);
		basicEffect.VertexColorEnabled = true;
		BasicEffect obj = basicEffect;
		Viewport viewport2 = LogicalViewport;
		float num = -viewport2.Width / 2;
		Viewport viewport3 = LogicalViewport;
		float num2 = viewport3.Width / 2;
		Viewport viewport4 = LogicalViewport;
		float num3 = viewport4.Height / 2;
		Viewport viewport5 = LogicalViewport;
		obj.Projection = Matrix.CreateOrthographicOffCenter(num, num2, num3, (float)(-viewport5.Height / 2), 0f, 1f);
		basicEffectRT = new BasicEffect(((Game)this).GraphicsDevice);
		basicEffectRT.VertexColorEnabled = true;
		BasicEffect obj2 = basicEffectRT;
		Viewport viewport6 = LogicalViewport;
		float num4 = -viewport6.Width / 2;
		Viewport viewport7 = LogicalViewport;
		float num5 = viewport7.Width / 2;
		Viewport viewport8 = LogicalViewport;
		float num6 = viewport8.Height / 2;
		Viewport viewport9 = LogicalViewport;
		obj2.Projection = Matrix.CreateOrthographicOffCenter(num4, num5, num6, (float)(-viewport9.Height / 2), 0f, 1f);
		selectedAnimation = -1;
		selectedKeyFrame = -1;
		editingAnimation = false;
		base.Initialize();
	}

	protected override void LoadContent()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		if (!LoadData())
		{
			pastProjectsPath = new List<string>();
		}
		projectPath = "";
		boundingBox = new Vector2(0f, 0f);
		spriteBatch = new SpriteBatch(((Game)this).GraphicsDevice);
		world = new World(new Vector2(0f, 0f));
		body = new Body(world);
		body.Position = Vector2.Zero;
		new BoneReader();
		hs = new Skeleton(Vector2.Zero, 0f, body);
		basePath = "";
		skeletonFile = "";
		animationsFile = "";
		spritesSubPath = "";
		hs.Initialize(((Game)this).GraphicsDevice);
		selectedBone = 0;
		spriteFont = ((Game)this).Content.Load<SpriteFont>("debugfont");
		spriteFontBold = ((Game)this).Content.Load<SpriteFont>("debugfontbold");
		backgroundTexture = new Texture2D(((Game)this).GraphicsDevice, 1, 1);
		RefreshBoneList();
		RefreshAnimationList();
		PresentationParameters presentationParameters = graphics.GraphicsDevice.PresentationParameters;
		mainRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, LogicalViewport.Width, LogicalViewport.Height);
		animRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, LogicalViewport.Width, LogicalViewport.Height);
		RecreateSceneTarget();
		if (ImportArgs != null)
		{
			ContentImporter.Run(((Game)this).Services, ((Game)this).GraphicsDevice, ImportArgs);
			((Game)this).Exit();
			return;
		}
		if (!string.IsNullOrEmpty(StartupProject) && LoadProject(StartupProject))
		{
			RefreshBoneList();
			RefreshAnimationList();
			projectPath = StartupProject;
		}
	}

	private const int BaseWidth = 1024;

	private const int BaseHeight = 768;

	private const int MaxUiScale = 3;

	/// <summary>Window pixels per editor pixel (F7), for high-resolution screens.</summary>
	private int uiScale = 1;

	/// <summary>Where the editor draws when zoomed; null at 1x (draws to the window).</summary>
	private RenderTarget2D sceneTarget;

	/// <summary>The window divided by the zoom: the size the editor lays itself out in.</summary>
	private Viewport LogicalViewport
	{
		get
		{
			PresentationParameters pp = graphics.GraphicsDevice.PresentationParameters;
			return new Viewport(0, 0, Math.Max(1, pp.BackBufferWidth / uiScale), Math.Max(1, pp.BackBufferHeight / uiScale));
		}
	}

	/// <summary>EDITOR_ZOOM if set, else the largest zoom whose window fits the screen.</summary>
	private static int InitialUiScale()
	{
		if (int.TryParse(Environment.GetEnvironmentVariable("EDITOR_ZOOM"), out int zoom))
		{
			return MathHelper.Clamp(zoom, 1, MaxUiScale);
		}
		DisplayMode mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
		int scale = Math.Min(mode.Width * 9 / 10 / BaseWidth, mode.Height * 9 / 10 / BaseHeight);
		return MathHelper.Clamp(scale, 1, MaxUiScale);
	}

	private void SetUiScale(int scale)
	{
		uiScale = scale;
		InputManager.MouseScale = scale;
		graphics.PreferredBackBufferWidth = BaseWidth * scale;
		graphics.PreferredBackBufferHeight = BaseHeight * scale;
		graphics.ApplyChanges();
		Window_ClientSizeChanged(this, EventArgs.Empty);
	}

	private void RecreateSceneTarget()
	{
		sceneTarget?.Dispose();
		sceneTarget = uiScale > 1 ? new RenderTarget2D(graphics.GraphicsDevice, LogicalViewport.Width, LogicalViewport.Height) : null;
	}

	/// <summary>Scales the zoomed scene up to the window (nearest-neighbour, keeps pixel art crisp).</summary>
	private void PresentScene()
	{
		if (sceneTarget == null)
		{
			return;
		}
		graphics.GraphicsDevice.SetRenderTarget(null);
		graphics.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
		spriteBatch.Draw(sceneTarget, new Rectangle(0, 0, sceneTarget.Width * uiScale, sceneTarget.Height * uiScale), Color.White);
		spriteBatch.End();
	}

	/// <summary>Per-frame callbacks of open dialogs; removed when they return false.</summary>
	private readonly List<Func<bool>> dialogUpdaters = new List<Func<bool>>();

	/// <summary>Arguments of --import: convert game content to a project, then exit.</summary>
	public string[] ImportArgs;

	/// <summary>Project file to open at startup (first command-line argument).</summary>
	public string StartupProject;

	private bool LoadProject(string path)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		new XmlReaderSettings();
		ProjectData projectData;
		try
		{
			XmlReader val = XmlReader.Create(PathResolver.Resolve(path));
			try
			{
				projectData = IntermediateSerializer.Deserialize<ProjectData>(val, (string)null);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (FileNotFoundException)
		{
			MessageBoxWindow("File not Found, make sure the file and the path exist.");
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			MessageBoxWindow("Directory not Found, make sure the file and the path exist.");
			return false;
		}
		catch (ArgumentException)
		{
			MessageBoxWindow("There is an error in the path or filename");
			return false;
		}
		catch (Exception ex4)
		{
			MessageBoxWindow("Error while Loading:\n" + ex4.Message);
			return false;
		}
		basePath = ValidatePath(projectData.BasePath, isBasePath: true);
		if (string.IsNullOrEmpty(basePath) || !Directory.Exists(PathResolver.Resolve(basePath)))
		{
			// Saved somewhere else (e.g. "c:\\proj\\hero" on Windows): use the
			// folder the project file is in.
			basePath = Path.GetDirectoryName(Path.GetFullPath(PathResolver.Resolve(path))).Replace('\\', '/');
		}
		animationsFile = ValidatePath(projectData.AnimationsFile);
		skeletonFile = ValidatePath(projectData.SkeletonFile);
		spritesSubPath = ValidatePath(projectData.SpritesSubPath);
		boundingBox = projectData.BoundingBox;
		if (LoadSkeleton(basePath, skeletonFile, spritesSubPath))
		{
			if (!LoadAnimation(basePath, animationsFile))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private bool SaveProject(string path)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		ProjectData projectData = new ProjectData
		{
			BasePath = ValidatePath(basePath, isBasePath: true),
			AnimationsFile = ValidatePath(animationsFile),
			SkeletonFile = ValidatePath(skeletonFile),
			SpritesSubPath = ValidatePath(spritesSubPath),
			BoundingBox = boundingBox
		};
		XmlWriterSettings val = new XmlWriterSettings();
		val.Indent = true;
		try
		{
			XmlWriter val2 = XmlWriter.Create(path);
			try
			{
				IntermediateSerializer.Serialize<ProjectData>(val2, projectData, (string)null);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			MessageBoxWindow("Error while saving Project:\n" + ex.Message);
			return false;
		}
		SaveSkeleton(basePath, skeletonFile, spritesSubPath);
		SaveAnimation(basePath, animationsFile);
		return true;
	}

	private bool LoadSkeleton(string path, string skeleton, string spritesPath)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		body.Dispose();
		body = new Body(world);
		BoneXMLReadHelper[] bxrhl = new BoneXMLReadHelper[0];
		XmlReaderSettings val = new XmlReaderSettings();
		try
		{
			XmlReader val2 = XmlReader.Create(ProjectPath(path, skeleton), val);
			try
			{
				bxrhl = IntermediateSerializer.Deserialize<BoneXMLReadHelper[]>(val2, (string)null);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch (FileNotFoundException)
		{
			MessageBoxWindow("File not Found, make sure the file and the path exist.");
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			MessageBoxWindow("Directory not Found, make sure the file and the path exist.");
			return false;
		}
		catch (ArgumentException)
		{
			MessageBoxWindow("There is an error in the path or filename");
			return false;
		}
		catch (Exception ex4)
		{
			MessageBoxWindow("Error while Loading the Skeleton:\n" + ex4.Message);
			return false;
		}
		BoneReader boneReader = new BoneReader();
		hs = boneReader.ReadFromHelper(bxrhl, body);
		hs.Load(((Game)this).GraphicsDevice, ProjectPath(path, spritesPath), world, 1f);
		try
		{
			using Stream stream = File.OpenRead(ProjectPath(path, "background.png"));
			backgroundTexture = Texture2D.FromStream(((Game)this).GraphicsDevice, stream);
		}
		catch
		{
			backgroundTexture = new Texture2D(((Game)this).GraphicsDevice, 1, 1);
		}
		return true;
	}

	private bool SaveSkeleton(string path, string skeleton, string spritesPath)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		BoneXMLReadHelper[] helper = hs.GetHelper();
		XmlWriterSettings val = new XmlWriterSettings();
		val.Indent = true;
		try
		{
			XmlWriter val2 = XmlWriter.Create(ProjectPath(path, skeleton), val);
			try
			{
				IntermediateSerializer.Serialize<BoneXMLReadHelper[]>(val2, helper, (string)null);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch (FileNotFoundException)
		{
			MessageBoxWindow("File not Found, make sure the file and the path exist.");
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			MessageBoxWindow("Directory not Found, make sure the file and the path exist.");
			return false;
		}
		catch (ArgumentException)
		{
			MessageBoxWindow("There is an error in the path or filename");
			return false;
		}
		catch (Exception ex4)
		{
			MessageBoxWindow("Error while Saving the Skeleton:\n" + ex4.Message);
			return false;
		}
		return true;
	}

	private bool LoadAnimation(string path, string animations)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		XmlReaderSettings val = new XmlReaderSettings();
		BoneAnimationXMLReadHelper[] bshl;
		try
		{
			XmlReader val2 = XmlReader.Create(ProjectPath(path, animations), val);
			try
			{
				bshl = IntermediateSerializer.Deserialize<BoneAnimationXMLReadHelper[]>(val2, (string)null);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch (FileNotFoundException)
		{
			MessageBoxWindow("File not Found, make sure the file and the path exist.");
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			MessageBoxWindow("Directory not Found, make sure the file and the path exist.");
			return false;
		}
		catch (ArgumentException)
		{
			MessageBoxWindow("There is an error in the path or filename");
			return false;
		}
		catch (Exception ex4)
		{
			MessageBoxWindow("Error while Loading the Animation:\n" + ex4.Message);
			return false;
		}
		List<Bone> list = hs.GetBoneList();
		BoneReader boneReader = new BoneReader();
		BoneAnimation[] animations2 = boneReader.ReadAnimationFromHelper(bshl, list);
		hs.LoadAnimations(animations2);
		hs.SetAnimation("NONE");
		return true;
	}

	private bool SaveAnimation(string path, string animations)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		BoneAnimationXMLReadHelper[] animationHelper = hs.GetAnimationHelper(animationList.Items.ToList());
		XmlWriterSettings val = new XmlWriterSettings();
		val.Indent = true;
		try
		{
			XmlWriter val2 = XmlWriter.Create(ProjectPath(path, animations), val);
			try
			{
				IntermediateSerializer.Serialize<BoneAnimationXMLReadHelper[]>(val2, animationHelper, (string)null);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch (FileNotFoundException)
		{
			MessageBoxWindow("File not Found, make sure the file and the path exist.");
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			MessageBoxWindow("Directory not Found, make sure the file and the path exist.");
			return false;
		}
		catch (ArgumentException)
		{
			MessageBoxWindow("There is an error in the path or filename");
			return false;
		}
		catch (Exception ex4)
		{
			MessageBoxWindow("Error while Saving the Animation:\n" + ex4.Message);
			return false;
		}
		return true;
	}

	protected override void UnloadContent()
	{
	}

	private void CreateDesktopControls(Screen mainScreen)
	{
		WindowControl windowControl = new WindowControl();
		windowControl.Bounds = new UniRectangle(0f, 0f, 385f, 470f);
		windowControl.Bounds = new UniRectangle(new UniScalar(1f, -385f), new UniScalar(0.5f, -235f), 385f, 470f);
		windowControl.Title = "Bones and Animations";
		LabelControl labelControl = new LabelControl("Bones");
		labelControl.Bounds = new UniRectangle(10f, 30f, 200f, 24f);
		windowControl.Children.Add(labelControl);
		LabelControl labelControl2 = new LabelControl("Animations");
		labelControl2.Bounds = new UniRectangle(200f, 30f, 200f, 24f);
		windowControl.Children.Add(labelControl2);
		boneList = new ListControl();
		boneList.Bounds = new UniRectangle(10f, 60f, 150f, 300f);
		boneList.Slider.Bounds.Location.X.Offset -= 1f;
		boneList.Slider.Bounds.Location.Y.Offset += 1f;
		boneList.Slider.Bounds.Size.Y.Offset -= 2f;
		boneList.SelectionMode = ListSelectionMode.Single;
		windowControl.Children.Add(boneList);
		boneList.SelectionChanged += delegate
		{
			if (animationList.SelectedItems.Count > 0 && bewOpen)
			{
				bew.Close();
				bewOpen = false;
			}
		};
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "^";
		buttonControl.Bounds = new UniRectangle(165f, 60f, 20f, 20f);
		buttonControl.Pressed += delegate
		{
			if (boneList.SelectedItems.Count > 0)
			{
				int num = boneList.SelectedItems[0];
				if (hs.MoveBoneUpInSortList(num))
				{
					RefreshBoneList();
					boneList.SelectedItems[0] = num - 1;
				}
			}
		};
		windowControl.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "v";
		buttonControl2.Bounds = new UniRectangle(165f, 85f, 20f, 20f);
		buttonControl2.Pressed += delegate
		{
			if (boneList.SelectedItems.Count > 0)
			{
				int num = boneList.SelectedItems[0];
				if (hs.MoveBoneDownInSortList(num))
				{
					RefreshBoneList();
					boneList.SelectedItems[0] = num + 1;
				}
			}
		};
		windowControl.Children.Add(buttonControl2);
		ButtonControl buttonControl3 = new ButtonControl();
		buttonControl3.Text = "+";
		buttonControl3.Bounds = new UniRectangle(165f, 120f, 20f, 20f);
		buttonControl3.Pressed += delegate
		{
			if (boneList.SelectedItems.Count > 0)
			{
				try
				{
					WindowControl windowControl2 = CreateBoneWindow(boneList.Items[boneList.SelectedItems[0]]);
					if (windowControl2 != null)
					{
						gui.Screen.Desktop.Children.Insert(0, windowControl2);
					}
					return;
				}
				catch (Exception ex)
				{
					MessageBoxWindow(ex.Message);
					return;
				}
			}
			WindowControl windowControl3 = CreateBoneWindow("");
			if (windowControl3 != null)
			{
				gui.Screen.Desktop.Children.Insert(0, windowControl3);
			}
		};
		windowControl.Children.Add(buttonControl3);
		ButtonControl buttonControl4 = new ButtonControl();
		buttonControl4.Text = "-";
		buttonControl4.Bounds = new UniRectangle(165f, 145f, 20f, 20f);
		buttonControl4.Pressed += delegate
		{
			if (boneList.SelectedItems.Count > 0)
			{
				WindowControl windowControl2 = DeleteBoneWindow(boneList.Items[boneList.SelectedItems[0]]);
				if (windowControl2 != null)
				{
					gui.Screen.Desktop.Children.Insert(0, windowControl2);
				}
			}
		};
		windowControl.Children.Add(buttonControl4);
		ButtonControl buttonControl5 = new ButtonControl();
		buttonControl5.Text = "Edit Bone";
		buttonControl5.Bounds = new UniRectangle(boneList.Bounds.Left, 370f, 100f, 32f);
		buttonControl5.Pressed += delegate
		{
			if (!bewOpen)
			{
				WindowControl windowControl2 = BoneEditWindow();
				if (windowControl2 != null)
				{
					gui.Screen.Desktop.Children.Insert(0, windowControl2);
				}
			}
		};
		windowControl.Children.Add(buttonControl5);
		ButtonControl buttonControl6 = new ButtonControl();
		buttonControl6.Text = "Load Skeleton";
		buttonControl6.Bounds = new UniRectangle(boneList.Bounds.Left, 410f, 120f, 32f);
		buttonControl6.Pressed += delegate
		{
			WindowControl windowControl2 = LoadSkeletonWindow();
			if (windowControl2 != null)
			{
				gui.Screen.Desktop.Children.Insert(0, windowControl2);
			}
		};
		windowControl.Children.Add(buttonControl6);
		animationList = new ListControl();
		animationList.Bounds = new UniRectangle(200f, 60f, 150f, 300f);
		animationList.Slider.Bounds.Location.X.Offset -= 1f;
		animationList.Slider.Bounds.Location.Y.Offset += 1f;
		animationList.Slider.Bounds.Size.Y.Offset -= 2f;
		animationList.SelectionMode = ListSelectionMode.Single;
		windowControl.Children.Add(animationList);
		animationList.SelectionChanged += delegate
		{
			if (animationList.SelectedItems.Count > 0)
			{
				hs.SetAnimation(animationList.Items[animationList.SelectedItems[0]]);
				selectedAnimation = animationList.SelectedItems[0];
				selectedKeyFrame = -1;
				if (aewOpen)
				{
					if (!aew.IsOpen)
					{
						aew = AnimationEditWindow();
						if (aew != null)
						{
							gui.Screen.Desktop.Children.Insert(0, aew);
						}
					}
					else if (aew.Title != animationList.Items[animationList.SelectedItems[0]])
					{
						aew.Close();
					}
				}
			}
		};
		ButtonControl buttonControl7 = new ButtonControl();
		buttonControl7.Text = "^";
		buttonControl7.Bounds = new UniRectangle(355f, 60f, 20f, 20f);
		buttonControl7.Pressed += delegate
		{
			if (animationList.SelectedItems.Count > 0)
			{
				int num = animationList.SelectedItems[0];
				if (num > 0)
				{
					int num2 = num - 1;
					string value = animationList.Items[num2];
					animationList.Items[num2] = animationList.Items[num];
					animationList.Items[num] = value;
					animationList.SelectedItems[0] = num2;
					selectedAnimation = num2;
				}
			}
		};
		windowControl.Children.Add(buttonControl7);
		ButtonControl buttonControl8 = new ButtonControl();
		buttonControl8.Text = "v";
		buttonControl8.Bounds = new UniRectangle(355f, 85f, 20f, 20f);
		buttonControl8.Pressed += delegate
		{
			if (animationList.SelectedItems.Count > 0)
			{
				int num = animationList.SelectedItems[0];
				if (num < animationList.Items.Count - 1)
				{
					int num2 = num + 1;
					string value = animationList.Items[num2];
					animationList.Items[num2] = animationList.Items[num];
					animationList.Items[num] = value;
					animationList.SelectedItems[0] = num2;
					selectedAnimation = num2;
				}
			}
		};
		windowControl.Children.Add(buttonControl8);
		ButtonControl buttonControl9 = new ButtonControl();
		buttonControl9.Text = "+";
		buttonControl9.Bounds = new UniRectangle(355f, 120f, 20f, 20f);
		buttonControl9.Pressed += delegate
		{
			WindowControl windowControl2 = CreateAnimationWindow();
			if (windowControl2 != null)
			{
				gui.Screen.Desktop.Children.Insert(0, windowControl2);
			}
		};
		windowControl.Children.Add(buttonControl9);
		ButtonControl buttonControl10 = new ButtonControl();
		buttonControl10.Text = "-";
		buttonControl10.Bounds = new UniRectangle(355f, 145f, 20f, 20f);
		buttonControl10.Pressed += delegate
		{
			if (animationList.SelectedItems.Count > 0)
			{
				WindowControl windowControl2 = DeleteAnimationWindow(animationList.Items[animationList.SelectedItems[0]]);
				if (windowControl2 != null)
				{
					gui.Screen.Desktop.Children.Insert(0, windowControl2);
				}
			}
		};
		windowControl.Children.Add(buttonControl10);
		ButtonControl buttonControl11 = new ButtonControl();
		buttonControl11.Text = "Edit Animation";
		buttonControl11.Bounds = new UniRectangle(animationList.Bounds.Left, 370f, 120f, 32f);
		buttonControl11.Pressed += delegate
		{
			if (!aewOpen)
			{
				aew = AnimationEditWindow();
				if (aew != null)
				{
					gui.Screen.Desktop.Children.Insert(0, aew);
				}
			}
		};
		windowControl.Children.Add(buttonControl11);
		ButtonControl buttonControl12 = new ButtonControl();
		buttonControl12.Text = "Load Animation";
		buttonControl12.Bounds = new UniRectangle(animationList.Bounds.Left, 410f, 140f, 32f);
		buttonControl12.Pressed += delegate
		{
			WindowControl windowControl2 = LoadAnimationWindow();
			if (windowControl2 != null)
			{
				gui.Screen.Desktop.Children.Insert(0, windowControl2);
			}
		};
		windowControl.Children.Add(buttonControl12);
		mainScreen.Desktop.Children.Add(windowControl);
		ButtonControl buttonControl13 = new ButtonControl();
		buttonControl13.Text = "Load Project";
		buttonControl13.Bounds = new UniRectangle(new UniScalar(1f, -130f), new UniScalar(0f, 10f), 120f, 32f);
		buttonControl13.Pressed += delegate
		{
			WindowControl windowControl2 = LoadProjectWindow();
			if (windowControl2 != null)
			{
				gui.Screen.Desktop.Children.Insert(0, windowControl2);
			}
		};
		mainScreen.Desktop.Children.Add(buttonControl13);
		ButtonControl buttonControl14 = new ButtonControl();
		buttonControl14.Text = "Save Project";
		buttonControl14.Bounds = new UniRectangle(new UniScalar(1f, -130f), new UniScalar(0f, 54f), 120f, 32f);
		buttonControl14.Pressed += delegate
		{
			WindowControl windowControl2 = SaveProjectWindow();
			if (windowControl2 != null)
			{
				gui.Screen.Desktop.Children.Insert(0, windowControl2);
			}
		};
		mainScreen.Desktop.Children.Add(buttonControl14);
		ButtonControl buttonControl15 = new ButtonControl();
		buttonControl15.Text = "Setup Project";
		buttonControl15.Bounds = new UniRectangle(new UniScalar(1f, -130f), new UniScalar(0f, 98f), 120f, 32f);
		buttonControl15.Pressed += delegate
		{
			WindowControl windowControl2 = SetProjectVariablesWindow();
			if (windowControl2 != null)
			{
				gui.Screen.Desktop.Children.Insert(0, windowControl2);
			}
		};
		mainScreen.Desktop.Children.Add(buttonControl15);
		ButtonControl buttonControl16 = new ButtonControl();
		buttonControl16.Text = "Quit";
		buttonControl16.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl16.Pressed += delegate
		{
			((Game)this).Exit();
		};
		mainScreen.Desktop.Children.Add(buttonControl16);
	}

	private WindowControl SetProjectVariablesWindow()
	{
		WindowControl spvw = new WindowControl();
		spvw.Bounds = new UniRectangle(0f, 0f, 450f, 240f);
		spvw.Title = "Setup Project";
		LabelControl labelControl = new LabelControl("Base Path");
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		spvw.Children.Add(labelControl);
		InputControl pathInputControl = new InputControl();
		pathInputControl.Bounds = new UniRectangle(180f, 30f, 250f, 24f);
		pathInputControl.Text = basePath;
		spvw.Children.Add(pathInputControl);
		LabelControl labelControl2 = new LabelControl("Skeleton File Name");
		labelControl2.Bounds = new UniRectangle(10f, 60f, 150f, 24f);
		spvw.Children.Add(labelControl2);
		InputControl fileNameInputControl = new InputControl();
		fileNameInputControl.Bounds = new UniRectangle(180f, 60f, 250f, 24f);
		fileNameInputControl.Text = skeletonFile;
		spvw.Children.Add(fileNameInputControl);
		LabelControl labelControl3 = new LabelControl("Sprites SubPath");
		labelControl3.Bounds = new UniRectangle(10f, 90f, 100f, 24f);
		spvw.Children.Add(labelControl3);
		InputControl spritesPathInputControl = new InputControl();
		spritesPathInputControl.Bounds = new UniRectangle(180f, 90f, 250f, 24f);
		spritesPathInputControl.Text = spritesSubPath;
		spvw.Children.Add(spritesPathInputControl);
		LabelControl labelControl4 = new LabelControl("Animation File Name");
		labelControl4.Bounds = new UniRectangle(10f, 120f, 150f, 24f);
		spvw.Children.Add(labelControl4);
		InputControl animationFileNameInputControl = new InputControl();
		animationFileNameInputControl.Bounds = new UniRectangle(180f, 120f, 250f, 24f);
		animationFileNameInputControl.Text = animationsFile;
		spvw.Children.Add(animationFileNameInputControl);
		LabelControl labelControl5 = new LabelControl("Bounding Box Width");
		labelControl5.Bounds = new UniRectangle(10f, 150f, 100f, 24f);
		spvw.Children.Add(labelControl5);
		InputControl boundingBoxXInputControl = new InputControl();
		boundingBoxXInputControl.Bounds = new UniRectangle(160f, 150f, 30f, 24f);
		boundingBoxXInputControl.Text = boundingBox.X.ToString();
		spvw.Children.Add(boundingBoxXInputControl);
		LabelControl labelControl6 = new LabelControl("Bounding Box Height");
		labelControl6.Bounds = new UniRectangle(210f, 150f, 100f, 24f);
		spvw.Children.Add(labelControl6);
		InputControl boundingBoxYInputControl = new InputControl();
		boundingBoxYInputControl.Bounds = new UniRectangle(360f, 150f, 30f, 24f);
		boundingBoxYInputControl.Text = boundingBox.Y.ToString();
		spvw.Children.Add(boundingBoxYInputControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Update";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			basePath = ValidatePath(pathInputControl.Text, isBasePath: true);
			skeletonFile = ValidatePath(fileNameInputControl.Text);
			spritesSubPath = ValidatePath(spritesPathInputControl.Text);
			animationsFile = ValidatePath(animationFileNameInputControl.Text);
			if (float.TryParse(boundingBoxXInputControl.Text, out var result) && float.TryParse(boundingBoxYInputControl.Text, out var result2))
			{
				boundingBox = new Vector2(result, result2);
				spvw.Close();
			}
		};
		spvw.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Close";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			spvw.Close();
		};
		spvw.Children.Add(buttonControl2);
		return spvw;
	}

	/// <summary>
	/// Normalizes a path typed in the editor: '/' separators (Windows accepts
	/// them too) and no trailing separator. Sub-paths also lose any leading
	/// separator so they combine with the base path; the base path keeps it,
	/// since on Linux and macOS it marks an absolute path.
	/// </summary>
	/// <summary>Joins project path parts and finds the file whatever its name case.</summary>
	private static string ProjectPath(string basePath, string relativePath)
	{
		return PathResolver.Resolve(Path.Combine(basePath, relativePath));
	}

	private string ValidatePath(string path, bool isBasePath = false)
	{
		if (string.IsNullOrEmpty(path))
		{
			return path;
		}
		path = path.Replace('\\', '/').TrimEnd('/');
		return isBasePath ? path : path.TrimStart('/');
	}

	private WindowControl LoadSkeletonWindow()
	{
		WindowControl lsw = new WindowControl();
		lsw.Bounds = new UniRectangle(0f, 0f, 450f, 180f);
		lsw.Title = "Load Skeleton";
		LabelControl labelControl = new LabelControl("Base Path");
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		lsw.Children.Add(labelControl);
		InputControl pathInputControl = new InputControl();
		pathInputControl.Bounds = new UniRectangle(180f, 30f, 250f, 24f);
		pathInputControl.Text = basePath;
		lsw.Children.Add(pathInputControl);
		LabelControl labelControl2 = new LabelControl("File Name");
		labelControl2.Bounds = new UniRectangle(10f, 60f, 100f, 24f);
		lsw.Children.Add(labelControl2);
		InputControl fileNameInputControl = new InputControl();
		fileNameInputControl.Bounds = new UniRectangle(180f, 60f, 250f, 24f);
		fileNameInputControl.Text = skeletonFile;
		lsw.Children.Add(fileNameInputControl);
		LabelControl labelControl3 = new LabelControl("Sprites SubPath");
		labelControl3.Bounds = new UniRectangle(10f, 90f, 100f, 24f);
		lsw.Children.Add(labelControl3);
		InputControl spritesPathInputControl = new InputControl();
		spritesPathInputControl.Bounds = new UniRectangle(180f, 90f, 250f, 24f);
		spritesPathInputControl.Text = spritesSubPath;
		lsw.Children.Add(spritesPathInputControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Load";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			basePath = ValidatePath(pathInputControl.Text, isBasePath: true);
			skeletonFile = ValidatePath(fileNameInputControl.Text);
			spritesSubPath = ValidatePath(spritesPathInputControl.Text);
			if (LoadSkeleton(basePath, skeletonFile, spritesSubPath))
			{
				RefreshBoneList();
				RefreshAnimationList();
				lsw.Close();
			}
		};
		lsw.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Close";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			lsw.Close();
		};
		lsw.Children.Add(buttonControl2);
		return lsw;
	}

	private WindowControl LoadProjectWindow()
	{
		WindowControl lpw = new WindowControl();
		lpw.Bounds = new UniRectangle(0f, 0f, 450f, 300f);
		lpw.Title = "Load Project";
		LabelControl labelControl = new LabelControl("Project Full Path:");
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		lpw.Children.Add(labelControl);
		InputControl fileNameInputControl = new InputControl();
		fileNameInputControl.Bounds = new UniRectangle(180f, 30f, 250f, 24f);
		fileNameInputControl.Text = string.IsNullOrEmpty(projectPath) ? Directory.GetCurrentDirectory() + "/" : projectPath;
		fileNameInputControl.CaretPosition = fileNameInputControl.Text.Length;
		lpw.Children.Add(fileNameInputControl);
		ListControl pathList = new ListControl();
		pathList.Bounds = new UniRectangle(10f, 60f, 430f, 150f);
		pathList.Slider.Bounds.Location.X.Offset -= 1f;
		pathList.Slider.Bounds.Location.Y.Offset += 1f;
		pathList.Slider.Bounds.Size.Y.Offset -= 2f;
		pathList.SelectionMode = ListSelectionMode.Single;
		lpw.Children.Add(pathList);
		// The list browses the folder of the typed path (recent projects when it
		// is empty); pathTargets holds the full path behind each entry.
		List<string> pathTargets = new List<string>();
		string listedFor = null;
		bool refreshingList = false;
		void RefreshPathList()
		{
			refreshingList = true;
			listedFor = fileNameInputControl.Text ?? "";
			pathList.SelectedItems.Clear();
			pathList.Items.Clear();
			pathTargets.Clear();
			if (listedFor.Length == 0)
			{
				for (int num = pastProjectsPath.Count - 1; num >= 0; num--)
				{
					pathList.Items.Add(pastProjectsPath[num]);
					pathTargets.Add(pastProjectsPath[num]);
				}
			}
			else
			{
				FileBrowser.List(listedFor, pathList.Items, pathTargets);
			}
			refreshingList = false;
		}
		RefreshPathList();
		pathList.SelectionChanged += delegate
		{
			if (refreshingList || pathList.SelectedItems.Count == 0)
			{
				return;
			}
			string target = pathTargets[pathList.SelectedItems[0]].Replace('\\', '/');
			fileNameInputControl.Text = Directory.Exists(target) ? target.TrimEnd('/') + "/" : target;
			fileNameInputControl.CaretPosition = fileNameInputControl.Text.Length;
		};
		dialogUpdaters.Add(delegate
		{
			if (lpw.Parent == null)
			{
				return false; // closed
			}
			if (fileNameInputControl.Text != listedFor)
			{
				RefreshPathList();
			}
			return true;
		});
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Load";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			// A full path to the project file, so keep a leading '/'.
			string text = ValidatePath(FileBrowser.Expand(fileNameInputControl.Text), isBasePath: true);
			if (LoadProject(text))
			{
				if (!pastProjectsPath.Contains(text))
				{
					pastProjectsPath.Add(text);
					if (pastProjectsPath.Count > 10)
					{
						pastProjectsPath.RemoveAt(0);
					}
					SaveData();
				}
				RefreshBoneList();
				RefreshAnimationList();
				projectPath = text;
				lpw.Close();
			}
		};
		lpw.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Close";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			lpw.Close();
		};
		lpw.Children.Add(buttonControl2);
		return lpw;
	}

	private WindowControl SaveProjectWindow()
	{
		WindowControl spw = new WindowControl();
		spw.Bounds = new UniRectangle(0f, 0f, 450f, 300f);
		spw.Title = "Save Project";
		LabelControl labelControl = new LabelControl("Project Full Path:");
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		spw.Children.Add(labelControl);
		InputControl fileNameInputControl = new InputControl();
		fileNameInputControl.Bounds = new UniRectangle(180f, 30f, 250f, 24f);
		fileNameInputControl.Text = projectPath;
		spw.Children.Add(fileNameInputControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Save";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			if (SaveProject(fileNameInputControl.Text))
			{
				if (!pastProjectsPath.Contains(fileNameInputControl.Text))
				{
					pastProjectsPath.Add(fileNameInputControl.Text);
					if (pastProjectsPath.Count > 10)
					{
						pastProjectsPath.RemoveAt(0);
					}
					SaveData();
				}
				projectPath = fileNameInputControl.Text;
				spw.Close();
			}
		};
		spw.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Close";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			spw.Close();
		};
		spw.Children.Add(buttonControl2);
		return spw;
	}

	private WindowControl DeleteBoneWindow(string boneName)
	{
		WindowControl dbw = new WindowControl();
		dbw.Bounds = new UniRectangle(0f, 0f, 450f, 250f);
		dbw.Title = "Delete Bone";
		LabelControl labelControl = new LabelControl("Bone to Delete: " + boneName);
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		dbw.Children.Add(labelControl);
		LabelControl labelControl2 = new LabelControl("The Following Bones will be also Deleted:");
		labelControl2.Bounds = new UniRectangle(10f, 60f, 100f, 24f);
		dbw.Children.Add(labelControl2);
		ListControl listControl = new ListControl();
		listControl.Bounds = new UniRectangle(10f, 90f, 220f, 100f);
		listControl.Slider.Bounds.Location.X.Offset -= 1f;
		listControl.Slider.Bounds.Location.Y.Offset += 1f;
		listControl.Slider.Bounds.Size.Y.Offset -= 2f;
		listControl.SelectionMode = ListSelectionMode.Single;
		dbw.Children.Add(listControl);
		listControl.Items.Clear();
		List<ChildBone> allChilds = hs.GetBoneByName(boneName).GetAllChilds();
		foreach (ChildBone item in allChilds)
		{
			listControl.Items.Add(item.Name);
		}
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Delete";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			if (hs.RemoveBone(boneName))
			{
				RefreshBoneList();
				if (boneList.Items.Count == 0)
				{
					boneList.SelectedItems.Clear();
					selectedBone = 0;
				}
				dbw.Close();
			}
		};
		dbw.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Cancel";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			dbw.Close();
		};
		dbw.Children.Add(buttonControl2);
		return dbw;
	}

	private WindowControl CreateBoneWindow(string parentBoneName)
	{
		WindowControl law = new WindowControl();
		law.Bounds = new UniRectangle(0f, 0f, 450f, 180f);
		law.Title = "Create Bone";
		if (parentBoneName == "")
		{
			parentBoneName = "Root";
		}
		LabelControl labelControl = new LabelControl("Parent Bone: " + parentBoneName);
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		law.Children.Add(labelControl);
		LabelControl labelControl2 = new LabelControl("Bone Name");
		labelControl2.Bounds = new UniRectangle(10f, 60f, 100f, 24f);
		law.Children.Add(labelControl2);
		InputControl boneNameInputControl = new InputControl();
		boneNameInputControl.Bounds = new UniRectangle(180f, 60f, 250f, 24f);
		boneNameInputControl.Text = "NewBone";
		law.Children.Add(boneNameInputControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Create";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			bool flag = true;
			List<Bone> list = hs.GetBoneList();
			string text = boneNameInputControl.Text;
			foreach (Bone item in list)
			{
				if (item.Name == text)
				{
					flag = false;
					break;
				}
			}
			if (text == "Root" || text == "NewBone")
			{
				flag = false;
			}
			if (flag && hs.CreateAndLoadBone(boneNameInputControl.Text, parentBoneName, ((Game)this).GraphicsDevice, ProjectPath(basePath, spritesSubPath), world, 1f) != null)
			{
				RefreshBoneList();
				RefreshAnimationList();
				law.Close();
			}
		};
		law.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Cancel";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			law.Close();
		};
		law.Children.Add(buttonControl2);
		return law;
	}

	private WindowControl DeleteAnimationWindow(string animationName)
	{
		WindowControl dbw = new WindowControl();
		dbw.Bounds = new UniRectangle(0f, 0f, 450f, 250f);
		dbw.Title = "Delete Animation";
		LabelControl labelControl = new LabelControl("Animation to Delete: " + animationName);
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		dbw.Children.Add(labelControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Delete";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			if (hs.RemoveAnimation(animationName))
			{
				RefreshAnimationList();
				animationList.SelectedItems.Clear();
				selectedAnimation = -1;
				dbw.Close();
			}
		};
		dbw.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Cancel";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			dbw.Close();
		};
		dbw.Children.Add(buttonControl2);
		return dbw;
	}

	private WindowControl CreateAnimationWindow()
	{
		WindowControl caw = new WindowControl();
		caw.Bounds = new UniRectangle(0f, 0f, 450f, 180f);
		caw.Title = "Create Animation";
		LabelControl labelControl = new LabelControl("Animation Name");
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		caw.Children.Add(labelControl);
		InputControl animationNameInputControl = new InputControl();
		animationNameInputControl.Bounds = new UniRectangle(180f, 30f, 250f, 24f);
		animationNameInputControl.Text = "NewAnimation";
		caw.Children.Add(animationNameInputControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Create";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			if (hs.AddEmptyAnimation(animationNameInputControl.Text))
			{
				RefreshBoneList();
				RefreshAnimationList();
				caw.Close();
			}
		};
		caw.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Cancel";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			caw.Close();
		};
		caw.Children.Add(buttonControl2);
		return caw;
	}

	private WindowControl LoadAnimationWindow()
	{
		WindowControl law = new WindowControl();
		law.Bounds = new UniRectangle(0f, 0f, 450f, 180f);
		law.Title = "Load Animation";
		LabelControl labelControl = new LabelControl("Base Path: " + basePath);
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		law.Children.Add(labelControl);
		LabelControl labelControl2 = new LabelControl("File Name");
		labelControl2.Bounds = new UniRectangle(10f, 60f, 100f, 24f);
		law.Children.Add(labelControl2);
		InputControl fileNameInputControl = new InputControl();
		fileNameInputControl.Bounds = new UniRectangle(180f, 60f, 250f, 24f);
		fileNameInputControl.Text = animationsFile;
		law.Children.Add(fileNameInputControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Load";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0f, 10f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			animationsFile = ValidatePath(fileNameInputControl.Text);
			if (LoadAnimation(basePath, animationsFile))
			{
				RefreshBoneList();
				RefreshAnimationList();
				law.Close();
			}
		};
		law.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Close";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			law.Close();
		};
		law.Children.Add(buttonControl2);
		return law;
	}

	private WindowControl AnimationEditWindow()
	{
		if (animationList.SelectedItems.Count <= 0)
		{
			return null;
		}
		BoneAnimation boneAnimation = hs.AnimationsList[animationList.Items[animationList.SelectedItems[0]]];
		List<KeyFrame> keyFrames = hs.GetKeyFrames(boneAnimation.Name);
		string oldName = boneAnimation.Name;
		aew = new WindowControl();
		aew.Bounds = new UniRectangle(0f, 0f, 650f, 420f);
		aew.Title = animationList.Items[animationList.SelectedItems[0]];
		LabelControl labelControl = new LabelControl("Name");
		labelControl.Bounds = new UniRectangle(230f, 30f, 100f, 24f);
		aew.Children.Add(labelControl);
		InputControl nameInputControl = new InputControl();
		nameInputControl.Bounds = new UniRectangle(350f, 30f, 200f, 24f);
		nameInputControl.Text = boneAnimation.Name;
		aew.Children.Add(nameInputControl);
		LabelControl labelControl2 = new LabelControl("Bones");
		labelControl2.Bounds = new UniRectangle(10f, 30f, 200f, 24f);
		aew.Children.Add(labelControl2);
		LabelControl labelControl3 = new LabelControl("KeyFrames");
		labelControl3.Bounds = new UniRectangle(140f, 30f, 200f, 24f);
		aew.Children.Add(labelControl3);
		ListControl animationBoneList = new ListControl();
		animationBoneList.Bounds = new UniRectangle(10f, 60f, 120f, 342f);
		animationBoneList.Slider.Bounds.Location.X.Offset -= 1f;
		animationBoneList.Slider.Bounds.Location.Y.Offset += 1f;
		animationBoneList.Slider.Bounds.Size.Y.Offset -= 2f;
		animationBoneList.SelectionMode = ListSelectionMode.Single;
		aew.Children.Add(animationBoneList);
		int bonesNumber = hs.GetBonesNumber();
		animationBoneList.Items.Clear();
		for (int i = 0; i < bonesNumber; i++)
		{
			animationBoneList.Items.Add(hs.GetBoneAt(i).Name);
		}
		keyFrameList = new ListControl();
		keyFrameList.Bounds = new UniRectangle(140f, 60f, 60f, 342f);
		keyFrameList.Slider.Bounds.Location.X.Offset -= 1f;
		keyFrameList.Slider.Bounds.Location.Y.Offset += 1f;
		keyFrameList.Slider.Bounds.Size.Y.Offset -= 2f;
		keyFrameList.SelectionMode = ListSelectionMode.Single;
		aew.Children.Add(keyFrameList);
		ButtonControl pasteBoneKFListButton = new ButtonControl();
		pasteBoneKFListButton.Text = "Paste Bones";
		pasteBoneKFListButton.Bounds = new UniRectangle(320f, 286f, 100f, 32f);
		pasteBoneKFListButton.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				Dictionary<string, KeyFrameInfo> dictionary = new Dictionary<string, KeyFrameInfo>(keyFrames[keyFrameList.SelectedItems[0]].KeyFrameInfo);
				foreach (string key2 in copiedBonesKeyFrameList.Keys)
				{
					dictionary[key2] = copiedBonesKeyFrameList[key2];
				}
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				keyFrame.KeyFrameInfo = dictionary;
				keyFrames[keyFrameList.SelectedItems[0]] = keyFrame;
			}
		};
		if (copiedBonesKeyFrameList != null)
		{
			pasteBoneKFListButton.Text = "Paste Bones " + copiedBonesKeyFrameList.Count;
		}
		pasteBoneKFListButton.Enabled = false;
		aew.Children.Add(pasteBoneKFListButton);
		bool selecting = false;
		ButtonControl copyBoneKFListButton = new ButtonControl();
		copyBoneKFListButton.Text = "Select Bones";
		copyBoneKFListButton.Bounds = new UniRectangle(210f, 286f, 100f, 32f);
		copyBoneKFListButton.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				if (selecting)
				{
					if (animationBoneList.SelectedItems.Count > 0)
					{
						copiedBonesKeyFrameList = new Dictionary<string, KeyFrameInfo>();
						for (int j = 0; j < animationBoneList.SelectedItems.Count; j++)
						{
							copiedBonesKeyFrameList.Add(animationBoneList.Items[animationBoneList.SelectedItems[j]], keyFrames[keyFrameList.SelectedItems[0]].KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[j]]]);
						}
						int item = animationBoneList.SelectedItems[0];
						animationBoneList.SelectedItems.Clear();
						animationBoneList.SelectedItems.Add(item);
						pasteBoneKFListButton.Text = "Paste Bones " + copiedBonesKeyFrameList.Count;
					}
					animationBoneList.SelectionMode = ListSelectionMode.Single;
					pasteBoneKFListButton.Enabled = true;
					copyBoneKFListButton.Text = "Select Bones";
					selecting = false;
				}
				else
				{
					animationBoneList.SelectionMode = ListSelectionMode.Multi;
					copyBoneKFListButton.Text = "Copy Bones";
					selecting = true;
				}
			}
		};
		copyBoneKFListButton.Enabled = false;
		aew.Children.Add(copyBoneKFListButton);
		LabelControl pasteBoneKFLabel = new LabelControl("");
		pasteBoneKFLabel.Bounds = new UniRectangle(430f, 328f, 100f, 32f);
		aew.Children.Add(pasteBoneKFLabel);
		if (copiedBoneNameKeyFrame != "")
		{
			pasteBoneKFLabel.Text = "<= Origin Bone: " + copiedBoneNameKeyFrame;
		}
		ButtonControl pasteBoneKFButton = new ButtonControl();
		pasteBoneKFButton.Text = "Paste BKF";
		pasteBoneKFButton.Bounds = new UniRectangle(320f, 328f, 100f, 32f);
		pasteBoneKFButton.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0 && animationBoneList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				string key = animationBoneList.Items[animationBoneList.SelectedItems[0]];
				keyFrame.KeyFrameInfo[key] = copiedBoneKeyFrame;
				keyFrames[keyFrameList.SelectedItems[0]] = keyFrame;
			}
		};
		pasteBoneKFButton.Enabled = false;
		aew.Children.Add(pasteBoneKFButton);
		ButtonControl copyBoneKFButton = new ButtonControl();
		copyBoneKFButton.Text = "Copy BKF";
		copyBoneKFButton.Bounds = new UniRectangle(210f, 328f, 100f, 32f);
		copyBoneKFButton.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0 && animationBoneList.SelectedItems.Count > 0)
			{
				copiedBoneNameKeyFrame = animationBoneList.Items[animationBoneList.SelectedItems[0]];
				copiedBoneKeyFrame = keyFrames[keyFrameList.SelectedItems[0]].KeyFrameInfo[copiedBoneNameKeyFrame];
				pasteBoneKFLabel.Text = "<= Origin Bone: " + copiedBoneNameKeyFrame;
				pasteBoneKFButton.Enabled = true;
			}
		};
		copyBoneKFButton.Enabled = false;
		aew.Children.Add(copyBoneKFButton);
		ButtonControl pasteButton = new ButtonControl();
		pasteButton.Text = "Paste KF";
		pasteButton.Bounds = new UniRectangle(320f, 370f, 100f, 32f);
		pasteButton.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				keyFrame.Time = copiedKeyFrame.Time;
				keyFrame.KeyFrameInfo = new Dictionary<string, KeyFrameInfo>(copiedKeyFrame.KeyFrameInfo);
				keyFrames[keyFrameList.SelectedItems[0]] = keyFrame;
			}
		};
		if (copiedKeyFrame == null)
		{
			pasteButton.Enabled = false;
		}
		aew.Children.Add(pasteButton);
		ButtonControl copyButton = new ButtonControl();
		copyButton.Text = "Copy KF";
		copyButton.Bounds = new UniRectangle(210f, 370f, 100f, 32f);
		copyButton.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				copiedKeyFrame = new KeyFrame();
				copiedKeyFrame.Time = keyFrames[keyFrameList.SelectedItems[0]].Time;
				copiedKeyFrame.KeyFrameInfo = new Dictionary<string, KeyFrameInfo>(keyFrames[keyFrameList.SelectedItems[0]].KeyFrameInfo);
				pasteButton.Enabled = true;
			}
		};
		copyButton.Enabled = false;
		aew.Children.Add(copyButton);
		keyFrameList.Items.Clear();
		string keyFrameName = "KF";
		int keyFrameNumber = 0;
		foreach (KeyFrame item2 in keyFrames)
		{
			_ = item2;
			keyFrameList.Items.Add(keyFrameName + keyFrameNumber);
			keyFrameNumber++;
		}
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "^";
		buttonControl.Bounds = new UniRectangle(205f, 60f, 20f, 20f);
		buttonControl.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				int num = keyFrameList.SelectedItems[0];
				if (boneAnimation.MoveKeyFrameUp(num))
				{
					keyFrames = hs.GetKeyFrames(boneAnimation.Name);
					keyFrameList.Items.Clear();
					keyFrameNumber = 0;
					foreach (KeyFrame item3 in keyFrames)
					{
						_ = item3;
						keyFrameList.Items.Add(keyFrameName + keyFrameNumber);
						keyFrameNumber++;
					}
					keyFrameList.SelectedItems[0] = num - 1;
				}
			}
		};
		aew.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "v";
		buttonControl2.Bounds = new UniRectangle(205f, 85f, 20f, 20f);
		buttonControl2.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				int num = keyFrameList.SelectedItems[0];
				if (boneAnimation.MoveKeyFrameDown(num))
				{
					keyFrames = hs.GetKeyFrames(boneAnimation.Name);
					keyFrameList.Items.Clear();
					keyFrameNumber = 0;
					foreach (KeyFrame item4 in keyFrames)
					{
						_ = item4;
						keyFrameList.Items.Add(keyFrameName + keyFrameNumber);
						keyFrameNumber++;
					}
					keyFrameList.SelectedItems[0] = num + 1;
				}
			}
		};
		aew.Children.Add(buttonControl2);
		ButtonControl buttonControl3 = new ButtonControl();
		buttonControl3.Text = "+";
		buttonControl3.Bounds = new UniRectangle(205f, 120f, 20f, 20f);
		buttonControl3.Pressed += delegate
		{
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			if (keyFrameList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				Dictionary<string, KeyFrameInfo> keyFrameInfo = new Dictionary<string, KeyFrameInfo>(keyFrame.KeyFrameInfo);
				KeyFrame keyFrame2 = new KeyFrame
				{
					Time = keyFrame.Time,
					KeyFrameInfo = keyFrameInfo
				};
				if (keyFrameList.SelectedItems[0] == keyFrames.Count - 1)
				{
					boneAnimation.AddKeyFrame(keyFrame2);
					num = keyFrames.Count - 1;
				}
				else
				{
					boneAnimation.AddKeyFrameAt(keyFrameList.SelectedItems[0] + 1, keyFrame2);
					num = keyFrameList.SelectedItems[0] + 1;
				}
			}
			else
			{
				KeyFrame keyFrame3 = new KeyFrame
				{
					Time = 10.0
				};
				Dictionary<string, KeyFrameInfo> dictionary = new Dictionary<string, KeyFrameInfo>();
				List<Bone> list = hs.GetBoneList();
				foreach (Bone item5 in list)
				{
					dictionary.Add(value: new KeyFrameInfo(item5.BaseAngle, item5.OriginalPosition), key: item5.Name);
				}
				keyFrame3.KeyFrameInfo = dictionary;
				boneAnimation.AddKeyFrame(keyFrame3);
				num = keyFrames.Count - 1;
			}
			keyFrames = hs.GetKeyFrames(boneAnimation.Name);
			keyFrameList.Items.Clear();
			keyFrameNumber = 0;
			foreach (KeyFrame item6 in keyFrames)
			{
				_ = item6;
				keyFrameList.Items.Add(keyFrameName + keyFrameNumber);
				keyFrameNumber++;
			}
			keyFrameList.SelectedItems.Clear();
			keyFrameList.SelectedItems.Add(num);
		};
		aew.Children.Add(buttonControl3);
		ButtonControl buttonControl4 = new ButtonControl();
		buttonControl4.Text = "-";
		buttonControl4.Bounds = new UniRectangle(205f, 145f, 20f, 20f);
		buttonControl4.Pressed += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				boneAnimation.RemoveKeyFrame(keyFrameList.SelectedItems[0]);
				keyFrameList.Items.Clear();
				keyFrameList.SelectedItems.Clear();
				keyFrameNumber = 0;
				foreach (KeyFrame item7 in keyFrames)
				{
					_ = item7;
					keyFrameList.Items.Add(keyFrameName + keyFrameNumber);
					keyFrameNumber++;
				}
			}
		};
		aew.Children.Add(buttonControl4);
		LabelControl labelControl4 = new LabelControl("Frames (60FPS)");
		labelControl4.Bounds = new UniRectangle(230f, 60f, 100f, 24f);
		aew.Children.Add(labelControl4);
		InputControl timeInputControl = new InputControl();
		timeInputControl.Bounds = new UniRectangle(350f, 60f, 100f, 24f);
		timeInputControl.Text = "";
		aew.Children.Add(timeInputControl);
		LabelControl labelControl5 = new LabelControl("Loop");
		labelControl5.Bounds = new UniRectangle(470f, 60f, 100f, 24f);
		aew.Children.Add(labelControl5);
		OptionControl loopOptionControl = new OptionControl();
		loopOptionControl.Bounds = new UniRectangle(600f, 60f, 24f, 24f);
		loopOptionControl.Selected = boneAnimation.Loop;
		aew.Children.Add(loopOptionControl);
		loopOptionControl.Changed += delegate
		{
			boneAnimation.Loop = loopOptionControl.Selected;
		};
		LabelControl labelControl6 = new LabelControl("Angle");
		labelControl6.Bounds = new UniRectangle(230f, 120f, 100f, 24f);
		aew.Children.Add(labelControl6);
		InputControl angleInputControl = new InputControl();
		angleInputControl.Bounds = new UniRectangle(350f, 120f, 100f, 24f);
		angleInputControl.Text = "";
		aew.Children.Add(angleInputControl);
		HorizontalSliderControl horSlider = new HorizontalSliderControl();
		horSlider.Bounds = new UniRectangle(230f, 150f, 400f, 12f);
		horSlider.ThumbSize = 0.05f;
		horSlider.ThumbPosition = 0f;
		horSlider.Moved += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0 && animationBoneList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				KeyFrameInfo value = keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]];
				ChildBone boneByName = hs.GetBoneByName(animationBoneList.Items[animationBoneList.SelectedItems[0]]);
				float num = boneByName.AngleOffset;
				float baseAngle = boneByName.BaseAngle;
				if (num == 0f)
				{
					num = (float)Math.PI;
				}
				angleInputControl.Text = Math.Round(MathHelper.ToDegrees(MathHelper.WrapAngle(horSlider.ThumbPosition * num * 2f - num + baseAngle))).ToString();
				if (float.TryParse(angleInputControl.Text, out var result))
				{
					result = MathHelper.ToRadians(result);
					value.Angle = MathHelper.WrapAngle(result);
					keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]] = value;
				}
			}
		};
		aew.Children.Add(horSlider);
		LabelControl labelControl7 = new LabelControl("Position X");
		labelControl7.Bounds = new UniRectangle(230f, 180f, 100f, 24f);
		aew.Children.Add(labelControl7);
		InputControl posXInputControl = new InputControl();
		posXInputControl.Bounds = new UniRectangle(330f, 180f, 50f, 24f);
		posXInputControl.Text = "";
		aew.Children.Add(posXInputControl);
		HorizontalSliderControl horPosXSlider = new HorizontalSliderControl();
		horPosXSlider.Bounds = new UniRectangle(labelControl7.Bounds.Left, 210f, posXInputControl.Bounds.Right - labelControl7.Bounds.Left, 12f);
		horPosXSlider.ThumbSize = 0.05f;
		horPosXSlider.ThumbPosition = 0f;
		horPosXSlider.Moved += delegate
		{
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			if (keyFrameList.SelectedItems.Count > 0 && animationBoneList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				KeyFrameInfo value = keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]];
				posXInputControl.Text = Math.Round(horPosXSlider.ThumbPosition * 100f - 50f).ToString();
				if (int.TryParse(posXInputControl.Text, out var result))
				{
					Vector2 position = default(Vector2);
					position = new Vector2((float)result, value.Position.Y);
					value.Position = position;
					keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]] = value;
				}
			}
		};
		aew.Children.Add(horPosXSlider);
		LabelControl labelControl8 = new LabelControl("Position Y");
		labelControl8.Bounds = new UniRectangle(390f, 180f, 100f, 24f);
		aew.Children.Add(labelControl8);
		InputControl posYInputControl = new InputControl();
		posYInputControl.Bounds = new UniRectangle(490f, 180f, 50f, 24f);
		posYInputControl.Text = "";
		aew.Children.Add(posYInputControl);
		HorizontalSliderControl horPosYSlider = new HorizontalSliderControl();
		horPosYSlider.Bounds = new UniRectangle(labelControl8.Bounds.Left, 210f, posYInputControl.Bounds.Right - labelControl8.Bounds.Left, 12f);
		horPosYSlider.ThumbSize = 0.05f;
		horPosYSlider.ThumbPosition = 0f;
		horPosYSlider.Moved += delegate
		{
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			if (keyFrameList.SelectedItems.Count > 0 && animationBoneList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				KeyFrameInfo value = keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]];
				posYInputControl.Text = Math.Round(horPosYSlider.ThumbPosition * 100f - 50f).ToString();
				if (int.TryParse(posYInputControl.Text, out var result))
				{
					Vector2 position = default(Vector2);
					position = new Vector2(value.Position.X, (float)result);
					value.Position = position;
					keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]] = value;
				}
			}
		};
		aew.Children.Add(horPosYSlider);
		LabelControl labelControl9 = new LabelControl("Bone Frame to Draw");
		labelControl9.Bounds = new UniRectangle(390f, 240f, 100f, 24f);
		aew.Children.Add(labelControl9);
		ListControl spriteFrameList = new ListControl();
		spriteFrameList.Bounds = new UniRectangle(570f, 240f, 60f, 60f);
		spriteFrameList.Slider.Bounds.Location.X.Offset -= 1f;
		spriteFrameList.Slider.Bounds.Location.Y.Offset += 1f;
		spriteFrameList.Slider.Bounds.Size.Y.Offset -= 2f;
		spriteFrameList.SelectionMode = ListSelectionMode.Single;
		aew.Children.Add(spriteFrameList);
		spriteFrameList.SelectionChanged += delegate
		{
			KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
			KeyFrameInfo value = keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]];
			int spriteFrame = 0;
			if (spriteFrameList.SelectedItems.Count > 0)
			{
				spriteFrame = spriteFrameList.SelectedItems[0];
			}
			value.SpriteFrame = spriteFrame;
			keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]] = value;
		};
		keyFrameList.SelectionChanged += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				selectedKeyFrame = keyFrameList.SelectedItems[0];
				KeyFrame keyFrame = keyFrames[selectedKeyFrame];
				timeInputControl.Text = (keyFrame.Time / 1000.0 * 60.0).ToString();
				copyButton.Enabled = true;
				copyBoneKFListButton.Enabled = true;
				if (copiedBonesKeyFrameList != null)
				{
					pasteBoneKFListButton.Enabled = true;
				}
				if (animationBoneList.SelectedItems.Count > 0)
				{
					copyBoneKFButton.Enabled = true;
					if (copiedBoneNameKeyFrame != "")
					{
						pasteBoneKFButton.Enabled = true;
					}
					KeyFrameInfo keyFrameInfo = keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]];
					angleInputControl.Text = Math.Round(MathHelper.ToDegrees(keyFrameInfo.Angle)).ToString();
					ChildBone boneByName = hs.GetBoneByName(animationBoneList.Items[animationBoneList.SelectedItems[0]]);
					float num = boneByName.AngleOffset;
					if (num == 0f)
					{
						num = (float)Math.PI;
					}
					horSlider.ThumbPosition = (MathHelper.WrapAngle(keyFrameInfo.Angle - boneByName.BaseAngle) + num) / (num * 2f);
					horPosXSlider.ThumbPosition = (keyFrameInfo.Position.X + 50f) / 100f;
					horPosYSlider.ThumbPosition = (keyFrameInfo.Position.Y + 50f) / 100f;
					posXInputControl.Text = keyFrameInfo.Position.X.ToString();
					posYInputControl.Text = keyFrameInfo.Position.Y.ToString();
					spriteFrameList.Items.Clear();
					spriteFrameList.SelectedItems.Clear();
					for (int j = 0; j < hs.GetBoneByName(animationBoneList.Items[animationBoneList.SelectedItems[0]]).SpriteFrames; j++)
					{
						spriteFrameList.Items.Add(j.ToString());
					}
					spriteFrameList.SelectedItems.Add(keyFrameInfo.SpriteFrame);
				}
			}
		};
		animationBoneList.SelectionChanged += delegate
		{
			if (keyFrameList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				timeInputControl.Text = (keyFrame.Time / 1000.0 * 60.0).ToString();
				if (animationBoneList.SelectedItems.Count > 0)
				{
					copyBoneKFButton.Enabled = true;
					if (copiedBoneNameKeyFrame != "")
					{
						pasteBoneKFButton.Enabled = true;
					}
					KeyFrameInfo keyFrameInfo = keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]];
					angleInputControl.Text = Math.Round(MathHelper.ToDegrees(keyFrameInfo.Angle)).ToString();
					ChildBone boneByName = hs.GetBoneByName(animationBoneList.Items[animationBoneList.SelectedItems[0]]);
					float num = boneByName.AngleOffset;
					if (num == 0f)
					{
						num = (float)Math.PI;
					}
					horSlider.ThumbPosition = (MathHelper.WrapAngle(keyFrameInfo.Angle - boneByName.BaseAngle) + num) / (num * 2f);
					horPosXSlider.ThumbPosition = (keyFrameInfo.Position.X + 50f) / 100f;
					horPosYSlider.ThumbPosition = (keyFrameInfo.Position.Y + 50f) / 100f;
					posXInputControl.Text = keyFrameInfo.Position.X.ToString();
					posYInputControl.Text = keyFrameInfo.Position.Y.ToString();
					spriteFrameList.Items.Clear();
					spriteFrameList.SelectedItems.Clear();
					for (int j = 0; j < hs.GetBoneByName(animationBoneList.Items[animationBoneList.SelectedItems[0]]).SpriteFrames; j++)
					{
						spriteFrameList.Items.Add(j.ToString());
					}
					spriteFrameList.SelectedItems.Add(keyFrameInfo.SpriteFrame);
				}
			}
		};
		ButtonControl buttonControl5 = new ButtonControl();
		buttonControl5.Text = "Update";
		buttonControl5.Bounds = new UniRectangle(new UniScalar(1f, -180f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl5.Pressed += delegate
		{
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			if (keyFrameList.SelectedItems.Count > 0)
			{
				KeyFrame keyFrame = keyFrames[keyFrameList.SelectedItems[0]];
				if (double.TryParse(timeInputControl.Text, out var result))
				{
					keyFrame.Time = result / 60.0 * 1000.0;
				}
				if (animationBoneList.SelectedItems.Count > 0)
				{
					KeyFrameInfo value = keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]];
					if (!float.TryParse(angleInputControl.Text, out var result2))
					{
						return;
					}
					result2 = MathHelper.ToRadians(result2);
					value.Angle = MathHelper.WrapAngle(result2);
					int spriteFrame = 0;
					if (spriteFrameList.SelectedItems.Count > 0)
					{
						spriteFrame = spriteFrameList.SelectedItems[0];
					}
					if (!float.TryParse(posXInputControl.Text, out var result3) || !float.TryParse(posYInputControl.Text, out var result4))
					{
						return;
					}
					value.Position = new Vector2(result3, result4);
					value.SpriteFrame = spriteFrame;
					keyFrame.KeyFrameInfo[animationBoneList.Items[animationBoneList.SelectedItems[0]]] = value;
					ChildBone boneByName = hs.GetBoneByName(animationBoneList.Items[animationBoneList.SelectedItems[0]]);
					float num = boneByName.AngleOffset;
					if (num == 0f)
					{
						num = (float)Math.PI;
					}
					horSlider.ThumbPosition = (MathHelper.WrapAngle(value.Angle - boneByName.BaseAngle) + num) / (num * 2f);
					horPosXSlider.ThumbPosition = (value.Position.X + 50f) / 100f;
					horPosYSlider.ThumbPosition = (value.Position.Y + 50f) / 100f;
				}
			}
			if (oldName != nameInputControl.Text)
			{
				hs.ChangeAnimationName(oldName, nameInputControl.Text);
				oldName = nameInputControl.Text;
				RefreshAnimationList();
			}
		};
		aew.Children.Add(buttonControl5);
		ButtonControl buttonControl6 = new ButtonControl();
		buttonControl6.Text = "Close";
		buttonControl6.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl6.Pressed += delegate
		{
			aewOpen = false;
			aew.Close();
		};
		aew.Children.Add(buttonControl6);
		aewOpen = true;
		return aew;
	}

	private WindowControl BoneEditWindow()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		if (boneList.SelectedItems.Count <= 0)
		{
			return null;
		}
		ChildBone theSelectedBone = hs.GetBoneAt(selectedBone);
		Vector2 oldOriginalPosition = theSelectedBone.OriginalPosition;
		bew = new WindowControl();
		bew.Bounds = new UniRectangle(0f, 0f, 450f, 420f);
		bew.Title = theSelectedBone.Name;
		LabelControl labelControl = new LabelControl("Name");
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		bew.Children.Add(labelControl);
		InputControl nameInputControl = new InputControl();
		nameInputControl.Bounds = new UniRectangle(110f, 30f, 100f, 24f);
		nameInputControl.Text = theSelectedBone.Name;
		bew.Children.Add(nameInputControl);
		LabelControl labelControl2 = new LabelControl("Frames");
		labelControl2.Bounds = new UniRectangle(250f, 30f, 100f, 24f);
		bew.Children.Add(labelControl2);
		InputControl framesInputControl = new InputControl();
		framesInputControl.Bounds = new UniRectangle(350f, 30f, 50f, 24f);
		framesInputControl.Text = theSelectedBone.SpriteFrames.ToString();
		bew.Children.Add(framesInputControl);
		LabelControl labelControl3 = new LabelControl("Angle");
		labelControl3.Bounds = new UniRectangle(10f, 60f, 100f, 24f);
		bew.Children.Add(labelControl3);
		InputControl angleInputControl = new InputControl();
		angleInputControl.Bounds = new UniRectangle(110f, 60f, 100f, 24f);
		angleInputControl.Text = Math.Round(MathHelper.ToDegrees(theSelectedBone.BaseAngle)).ToString();
		bew.Children.Add(angleInputControl);
		LabelControl labelControl4 = new LabelControl("Default Frame");
		labelControl4.Bounds = new UniRectangle(250f, 60f, 100f, 24f);
		bew.Children.Add(labelControl4);
		InputControl defaultFramesInputControl = new InputControl();
		defaultFramesInputControl.Bounds = new UniRectangle(350f, 60f, 50f, 24f);
		defaultFramesInputControl.Text = theSelectedBone.DefaultFrame.ToString();
		bew.Children.Add(defaultFramesInputControl);
		HorizontalSliderControl horSlider = new HorizontalSliderControl();
		horSlider.Bounds = new UniRectangle(10f, 90f, 400f, 12f);
		horSlider.ThumbSize = 0.05f;
		horSlider.ThumbPosition = MathHelper.ToDegrees(MathHelper.WrapAngle(theSelectedBone.BaseAngle) + (float)Math.PI) / 360f;
		horSlider.Moved += delegate
		{
			angleInputControl.Text = Math.Round(horSlider.ThumbPosition * 360f - 180f).ToString();
			if (float.TryParse(angleInputControl.Text, out var result))
			{
				result = MathHelper.ToRadians(result);
				theSelectedBone.SetData(result);
			}
		};
		bew.Children.Add(horSlider);
		LabelControl labelControl5 = new LabelControl("Position X");
		labelControl5.Bounds = new UniRectangle(10f, 120f, 100f, 24f);
		bew.Children.Add(labelControl5);
		InputControl posXInputControl = new InputControl();
		posXInputControl.Bounds = new UniRectangle(110f, 120f, 50f, 24f);
		InputControl inputControl = posXInputControl;
		float x = theSelectedBone.OriginalPosition.X;
		inputControl.Text = x.ToString();
		bew.Children.Add(posXInputControl);
		LabelControl labelControl6 = new LabelControl("Position Y");
		labelControl6.Bounds = new UniRectangle(170f, 120f, 100f, 24f);
		bew.Children.Add(labelControl6);
		InputControl posYInputControl = new InputControl();
		posYInputControl.Bounds = new UniRectangle(270f, 120f, 50f, 24f);
		InputControl inputControl2 = posYInputControl;
		float y = theSelectedBone.OriginalPosition.Y;
		inputControl2.Text = y.ToString();
		bew.Children.Add(posYInputControl);
		LabelControl labelControl7 = new LabelControl("Bone Length");
		labelControl7.Bounds = new UniRectangle(10f, 150f, 100f, 24f);
		bew.Children.Add(labelControl7);
		InputControl lengthInputControl = new InputControl();
		lengthInputControl.Bounds = new UniRectangle(110f, 150f, 50f, 24f);
		lengthInputControl.Text = theSelectedBone.Length.ToString();
		bew.Children.Add(lengthInputControl);
		LabelControl labelControl8 = new LabelControl("Ragdoll");
		labelControl8.Bounds = new UniRectangle(250f, 150f, 100f, 24f);
		bew.Children.Add(labelControl8);
		OptionControl ragdollOptionControl = new OptionControl();
		ragdollOptionControl.Bounds = new UniRectangle(350f, 150f, 24f, 24f);
		ragdollOptionControl.Selected = theSelectedBone.IsInRagdoll;
		bew.Children.Add(ragdollOptionControl);
		ragdollOptionControl.Changed += delegate
		{
			theSelectedBone.IsInRagdoll = ragdollOptionControl.Selected;
		};
		LabelControl labelControl9 = new LabelControl("Offset Angle");
		labelControl9.Bounds = new UniRectangle(10f, 180f, 100f, 24f);
		bew.Children.Add(labelControl9);
		InputControl offsetAngleInputControl = new InputControl();
		offsetAngleInputControl.Bounds = new UniRectangle(110f, 180f, 100f, 24f);
		offsetAngleInputControl.Text = Math.Round(MathHelper.ToDegrees(theSelectedBone.AngleOffset)).ToString();
		bew.Children.Add(offsetAngleInputControl);
		HorizontalSliderControl horOffsetSlider = new HorizontalSliderControl();
		horOffsetSlider.Bounds = new UniRectangle(10f, 210f, 400f, 12f);
		horOffsetSlider.ThumbSize = 0.05f;
		horOffsetSlider.ThumbPosition = MathHelper.ToDegrees(MathHelper.WrapAngle(theSelectedBone.AngleOffset)) / 180f;
		horOffsetSlider.Moved += delegate
		{
			offsetAngleInputControl.Text = Math.Round(horOffsetSlider.ThumbPosition * 180f).ToString();
			if (float.TryParse(angleInputControl.Text, out var result))
			{
				result = MathHelper.ToRadians(result);
				if (float.TryParse(offsetAngleInputControl.Text, out var result2))
				{
					result2 = MathHelper.ToRadians(result2);
					theSelectedBone.SetData(result, result2);
				}
			}
		};
		bew.Children.Add(horOffsetSlider);
		LabelControl labelControl10 = new LabelControl("Parent");
		labelControl10.Bounds = new UniRectangle(10f, 240f, 100f, 24f);
		bew.Children.Add(labelControl10);
		ListControl parentBoneList = new ListControl();
		parentBoneList.Bounds = new UniRectangle(110f, 240f, 150f, 100f);
		parentBoneList.Slider.Bounds.Location.X.Offset -= 1f;
		parentBoneList.Slider.Bounds.Location.Y.Offset += 1f;
		parentBoneList.Slider.Bounds.Size.Y.Offset -= 2f;
		parentBoneList.SelectionMode = ListSelectionMode.Single;
		parentBoneList.Items.Add("Root");
		int item = 0;
		int bonesNumber = hs.GetBonesNumber();
		for (int num = 0; num < bonesNumber; num++)
		{
			Bone boneAt = hs.GetBoneAt(num);
			List<Bone> parentsList = boneAt.GetParentsList();
			if (!parentsList.Contains(theSelectedBone) && num != boneList.SelectedItems[0])
			{
				parentBoneList.Items.Add(boneAt.Name);
				if (boneAt.Name == theSelectedBone.Parent.Name)
				{
					item = parentBoneList.Items.Count - 1;
				}
			}
		}
		parentBoneList.SelectedItems.Clear();
		parentBoneList.SelectedItems.Add(item);
		bew.Children.Add(parentBoneList);
		string textLabel = "Current Parents:";
		List<Bone> parentsList2 = theSelectedBone.GetParentsList();
		string parentLevel = "-";
		foreach (Bone item2 in parentsList2)
		{
			textLabel = textLabel + "\n" + parentLevel + item2.Name;
			parentLevel = "-" + parentLevel;
		}
		LabelControl currentParentLabel = new LabelControl(textLabel);
		currentParentLabel.Bounds = new UniRectangle(270f, 240f, 100f, 24f);
		bew.Children.Add(currentParentLabel);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "Update";
		buttonControl.Bounds = new UniRectangle(new UniScalar(1f, -180f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_037a: Unknown result type (might be due to invalid IL or missing references)
			//IL_037c: Unknown result type (might be due to invalid IL or missing references)
			//IL_039e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
			if (float.TryParse(posXInputControl.Text, out var result) && float.TryParse(posYInputControl.Text, out var result2))
			{
				Vector2 val = default(Vector2);
				val = new Vector2(result, result2);
				if (int.TryParse(framesInputControl.Text, out var result3))
				{
					if (result3 <= 0)
					{
						result3 = 1;
					}
					if (int.TryParse(defaultFramesInputControl.Text, out var result4) && float.TryParse(angleInputControl.Text, out var result5))
					{
						result5 = MathHelper.ToRadians(result5);
						if (float.TryParse(lengthInputControl.Text, out var result6) && float.TryParse(offsetAngleInputControl.Text, out var result7))
						{
							result7 = MathHelper.ToRadians(result7);
							Bone bone = theSelectedBone.Parent;
							_ = theSelectedBone.Parent;
							if (parentBoneList.SelectedItems.Count > 0)
							{
								string text = parentBoneList.Items[parentBoneList.SelectedItems[0]];
								if (text == "Root")
								{
									bone = hs;
								}
								else
								{
									bone = hs.GetBoneByName(text);
									if (bone == null)
									{
										bone = theSelectedBone.Parent;
									}
								}
							}
							if (nameInputControl.Text == "")
							{
								nameInputControl.Text = theSelectedBone.Name;
							}
							string text2 = nameInputControl.Text;
							List<Bone> list = hs.GetBoneList();
							foreach (Bone item3 in list)
							{
								if (item3.Name == text2)
								{
									text2 = theSelectedBone.Name;
									break;
								}
							}
							if (theSelectedBone.Name == "Root" || theSelectedBone.Name == "NewBone")
							{
								text2 = theSelectedBone.Name;
							}
							theSelectedBone.SetData(nameInputControl.Text, val, result5, result6, result7, result3, result4, bone);
							textLabel = "Current Parents:";
							List<Bone> parentsList3 = theSelectedBone.GetParentsList();
							parentLevel = "-";
							foreach (Bone item4 in parentsList3)
							{
								textLabel = textLabel + "\n" + parentLevel + item4.Name;
								parentLevel = "-" + parentLevel;
							}
							currentParentLabel.Text = textLabel;
							horSlider.ThumbPosition = MathHelper.ToDegrees(MathHelper.WrapAngle(theSelectedBone.BaseAngle) + (float)Math.PI) / 360f;
							horOffsetSlider.ThumbPosition = MathHelper.ToDegrees(MathHelper.WrapAngle(theSelectedBone.AngleOffset)) / 180f;
							offsetAngleInputControl.Text = Math.Round(MathHelper.ToDegrees(theSelectedBone.AngleOffset)).ToString();
							angleInputControl.Text = Math.Round(MathHelper.ToDegrees(theSelectedBone.BaseAngle)).ToString();
							if (val != oldOriginalPosition)
							{
								hs.UpdateBoneAnimationByOffset(theSelectedBone.Name, val - oldOriginalPosition);
								oldOriginalPosition = val;
							}
							RefreshBoneList();
						}
					}
				}
			}
		};
		bew.Children.Add(buttonControl);
		ButtonControl buttonControl2 = new ButtonControl();
		buttonControl2.Text = "Close";
		buttonControl2.Bounds = new UniRectangle(new UniScalar(1f, -90f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl2.Pressed += delegate
		{
			bewOpen = false;
			bew.Close();
		};
		bew.Children.Add(buttonControl2);
		bewOpen = true;
		return bew;
	}

	private void MessageBoxWindow(string message)
	{
		WindowControl mbw = new WindowControl();
		mbw.Bounds = new UniRectangle(0f, 0f, 600f, 250f);
		mbw.Title = "Info";
		LabelControl labelControl = new LabelControl(message);
		labelControl.Bounds = new UniRectangle(10f, 30f, 100f, 24f);
		mbw.Children.Add(labelControl);
		ButtonControl buttonControl = new ButtonControl();
		buttonControl.Text = "OK";
		buttonControl.Bounds = new UniRectangle(new UniScalar(0.5f, -40f), new UniScalar(1f, -42f), 80f, 32f);
		buttonControl.Pressed += delegate
		{
			mbw.Close();
		};
		mbw.Children.Add(buttonControl);
		gui.Screen.Desktop.Children.Insert(0, mbw);
	}

	private void RefreshBoneList()
	{
		int bonesNumber = hs.GetBonesNumber();
		boneList.Items.Clear();
		for (int i = 0; i < bonesNumber; i++)
		{
			boneList.Items.Add(hs.GetBoneAt(i).Name);
		}
	}

	private void RefreshAnimationList()
	{
		List<string> animationListNames = hs.GetAnimationListNames();
		animationList.Items.Clear();
		for (int i = 0; i < animationListNames.Count; i++)
		{
			string item = animationListNames[i];
			animationList.Items.Add(item);
		}
	}

	protected override void Update(GameTime gameTime)
	{
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if (((Game)this).IsActive)
		{
			if (boneList.SelectedItems.Count > 0)
			{
				selectedBone = boneList.SelectedItems[0];
			}
			if (animationList.SelectedItems.Count > 0)
			{
				selectedAnimation = animationList.SelectedItems[0];
			}
			dialogUpdaters.RemoveAll(update => !update());
			KeyboardState state = Keyboard.GetState();
			if (state.IsKeyDown((Keys)112) && oldKeyState.IsKeyUp((Keys)112))
			{
				debug = !debug;
			}
			KeyboardState state2 = Keyboard.GetState();
			if (state2.IsKeyDown((Keys)113) && oldKeyState.IsKeyUp((Keys)113))
			{
				editingAnimation = !editingAnimation;
			}
			KeyboardState state3 = Keyboard.GetState();
			if (state3.IsKeyDown((Keys)114) && oldKeyState.IsKeyUp((Keys)114))
			{
				drawHBoundingBox = !drawHBoundingBox;
			}
			KeyboardState state4 = Keyboard.GetState();
			if (state4.IsKeyDown((Keys)115) && oldKeyState.IsKeyUp((Keys)115))
			{
				drawVBoundingBox = !drawVBoundingBox;
			}
			KeyboardState state5 = Keyboard.GetState();
			if (state5.IsKeyDown((Keys)116) && oldKeyState.IsKeyUp((Keys)116))
			{
				hs.ReloadTexture(((Game)this).GraphicsDevice, ProjectPath(basePath, spritesSubPath));
				try
				{
					using Stream stream = File.OpenRead(ProjectPath(basePath, "background.png"));
					backgroundTexture = Texture2D.FromStream(((Game)this).GraphicsDevice, stream);
				}
				catch
				{
					backgroundTexture = new Texture2D(((Game)this).GraphicsDevice, 1, 1);
				}
			}
			KeyboardState state6 = Keyboard.GetState();
			if (state6.IsKeyDown((Keys)117) && oldKeyState.IsKeyUp((Keys)117))
			{
				drawBackground = !drawBackground;
			}
			if (state6.IsKeyDown(Keys.F7) && oldKeyState.IsKeyUp(Keys.F7))
			{
				SetUiScale(uiScale % MaxUiScale + 1);
			}
			if (aewOpen)
			{
				KeyboardState state7 = Keyboard.GetState();
				if (state7.IsKeyDown((Keys)33) && oldKeyState.IsKeyUp((Keys)33))
				{
					if (keyFrameList.SelectedItems.Count > 0)
					{
						int num = keyFrameList.SelectedItems[0];
						num--;
						if (num < 0)
						{
							num = keyFrameList.Items.Count - 1;
						}
						keyFrameList.SelectedItems[0] = num;
						selectedKeyFrame = num;
					}
					else if (keyFrameList.Items.Count > 0)
					{
						keyFrameList.SelectedItems.Add(0);
						selectedKeyFrame = 0;
					}
				}
				KeyboardState state8 = Keyboard.GetState();
				if (state8.IsKeyDown((Keys)34) && oldKeyState.IsKeyUp((Keys)34))
				{
					if (keyFrameList.SelectedItems.Count > 0)
					{
						int num2 = keyFrameList.SelectedItems[0];
						num2++;
						if (num2 >= keyFrameList.Items.Count)
						{
							num2 = 0;
						}
						keyFrameList.SelectedItems[0] = num2;
						selectedKeyFrame = num2;
					}
					else if (keyFrameList.Items.Count > 0)
					{
						keyFrameList.SelectedItems.Add(0);
						selectedKeyFrame = 0;
					}
				}
			}
			oldKeyState = Keyboard.GetState();
			view = Matrix.CreateRotationZ(0f) * Matrix.CreateTranslation(new Vector3(-cameraPosition, 0f));
			hs.Angle = (float)Math.PI;
			hs.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
		}
		base.Update(gameTime);
	}

	private void SaveData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		XmlWriterSettings val = new XmlWriterSettings();
		val.Indent = true;
		try
		{
			XmlWriter val2 = XmlWriter.Create("recentFiles.xml", val);
			try
			{
				IntermediateSerializer.Serialize<List<string>>(val2, pastProjectsPath, (string)null);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			MessageBoxWindow("Error while saving\n" + ex.Message);
		}
	}

	private bool LoadData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		new XmlReaderSettings();
		try
		{
			XmlReader val = XmlReader.Create("recentFiles.xml");
			try
			{
				pastProjectsPath = IntermediateSerializer.Deserialize<List<string>>(val, (string)null);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (ArgumentNullException)
		{
			return false;
		}
		catch (FileNotFoundException)
		{
			return false;
		}
		catch (UriFormatException)
		{
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			return false;
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	private void SaveDataOld()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		BoneAnimationXMLReadHelper boneAnimationXMLReadHelper = new BoneAnimationXMLReadHelper
		{
			name = "ANIM",
			loop = false
		};
		KeyFrameAnimationXMLReaderHelper keyFrameAnimationXMLReaderHelper = new KeyFrameAnimationXMLReaderHelper
		{
			time = 100
		};
		Dictionary<string, float[]> dictionary = new Dictionary<string, float[]>();
		int bonesNumber = hs.GetBonesNumber();
		for (int i = 0; i < bonesNumber; i++)
		{
			Bone boneAt = hs.GetBoneAt(i);
			float[] value = new float[3]
			{
				boneAt.Position.X,
				boneAt.Position.Y,
				boneAt.Angle
			};
			dictionary.Add(boneAt.Name, value);
			keyFrameAnimationXMLReaderHelper.values = dictionary;
		}
		XmlWriterSettings val = new XmlWriterSettings();
		val.Indent = true;
		XmlWriter val2 = XmlWriter.Create("test.xml", val);
		try
		{
			IntermediateSerializer.Serialize<KeyFrameAnimationXMLReaderHelper>(val2, keyFrameAnimationXMLReaderHelper, (string)null);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	protected override void Draw(GameTime gameTime)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0724: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0733: Unknown result type (might be due to invalid IL or missing references)
		//IL_0785: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_083d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_086b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0875: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_0690: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0946: Unknown result type (might be due to invalid IL or missing references)
		//IL_094b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b94: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b99: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c70: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cbd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cdb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cfc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d09: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d30: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d42: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d69: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d83: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d88: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d92: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d97: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0db6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dcf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dde: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df5: Unknown result type (might be due to invalid IL or missing references)
		//IL_11ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_11de: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_11f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_11f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_121a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1224: Unknown result type (might be due to invalid IL or missing references)
		//IL_122e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1233: Unknown result type (might be due to invalid IL or missing references)
		//IL_1240: Unknown result type (might be due to invalid IL or missing references)
		//IL_1245: Unknown result type (might be due to invalid IL or missing references)
		//IL_127d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1287: Unknown result type (might be due to invalid IL or missing references)
		//IL_1291: Unknown result type (might be due to invalid IL or missing references)
		//IL_1296: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_12cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_12d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_12e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_12e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1321: Unknown result type (might be due to invalid IL or missing references)
		//IL_133a: Unknown result type (might be due to invalid IL or missing references)
		//IL_133f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1349: Unknown result type (might be due to invalid IL or missing references)
		//IL_134e: Unknown result type (might be due to invalid IL or missing references)
		//IL_135b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1360: Unknown result type (might be due to invalid IL or missing references)
		//IL_136d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1387: Unknown result type (might be due to invalid IL or missing references)
		//IL_138c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1396: Unknown result type (might be due to invalid IL or missing references)
		//IL_139b: Unknown result type (might be due to invalid IL or missing references)
		//IL_13a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_13cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_13e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_13fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1408: Unknown result type (might be due to invalid IL or missing references)
		//IL_140d: Unknown result type (might be due to invalid IL or missing references)
		//IL_141a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1433: Unknown result type (might be due to invalid IL or missing references)
		//IL_1438: Unknown result type (might be due to invalid IL or missing references)
		//IL_1442: Unknown result type (might be due to invalid IL or missing references)
		//IL_1447: Unknown result type (might be due to invalid IL or missing references)
		//IL_1454: Unknown result type (might be due to invalid IL or missing references)
		//IL_1459: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09df: Unknown result type (might be due to invalid IL or missing references)
		//IL_09eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a46: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a50: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aaa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aaf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ee9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ef5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f11: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f32: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f39: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f50: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f69: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f76: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fcf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fe1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fe6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1000: Unknown result type (might be due to invalid IL or missing references)
		//IL_1018: Unknown result type (might be due to invalid IL or missing references)
		//IL_101d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1022: Unknown result type (might be due to invalid IL or missing references)
		//IL_153b: Unknown result type (might be due to invalid IL or missing references)
		//IL_154d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1552: Unknown result type (might be due to invalid IL or missing references)
		//IL_1557: Unknown result type (might be due to invalid IL or missing references)
		//IL_1559: Unknown result type (might be due to invalid IL or missing references)
		//IL_156b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1570: Unknown result type (might be due to invalid IL or missing references)
		//IL_1575: Unknown result type (might be due to invalid IL or missing references)
		//IL_1577: Unknown result type (might be due to invalid IL or missing references)
		//IL_157a: Unknown result type (might be due to invalid IL or missing references)
		//IL_157f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1584: Unknown result type (might be due to invalid IL or missing references)
		//IL_1596: Unknown result type (might be due to invalid IL or missing references)
		//IL_159d: Unknown result type (might be due to invalid IL or missing references)
		//IL_15a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_15af: Unknown result type (might be due to invalid IL or missing references)
		//IL_15b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_15cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_15da: Unknown result type (might be due to invalid IL or missing references)
		//IL_15df: Unknown result type (might be due to invalid IL or missing references)
		//IL_1601: Unknown result type (might be due to invalid IL or missing references)
		//IL_1608: Unknown result type (might be due to invalid IL or missing references)
		//IL_160d: Unknown result type (might be due to invalid IL or missing references)
		//IL_161a: Unknown result type (might be due to invalid IL or missing references)
		//IL_161f: Unknown result type (might be due to invalid IL or missing references)
		//IL_162c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1633: Unknown result type (might be due to invalid IL or missing references)
		//IL_1638: Unknown result type (might be due to invalid IL or missing references)
		//IL_1645: Unknown result type (might be due to invalid IL or missing references)
		//IL_164a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1664: Unknown result type (might be due to invalid IL or missing references)
		//IL_167c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1681: Unknown result type (might be due to invalid IL or missing references)
		//IL_1686: Unknown result type (might be due to invalid IL or missing references)
		//IL_1046: Unknown result type (might be due to invalid IL or missing references)
		//IL_105f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1064: Unknown result type (might be due to invalid IL or missing references)
		//IL_106e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1073: Unknown result type (might be due to invalid IL or missing references)
		//IL_1080: Unknown result type (might be due to invalid IL or missing references)
		//IL_1085: Unknown result type (might be due to invalid IL or missing references)
		//IL_1092: Unknown result type (might be due to invalid IL or missing references)
		//IL_10aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_10af: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_10be: Unknown result type (might be due to invalid IL or missing references)
		//IL_10cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_110c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1111: Unknown result type (might be due to invalid IL or missing references)
		//IL_111b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1120: Unknown result type (might be due to invalid IL or missing references)
		//IL_112d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1132: Unknown result type (might be due to invalid IL or missing references)
		//IL_113f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1158: Unknown result type (might be due to invalid IL or missing references)
		//IL_115d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1167: Unknown result type (might be due to invalid IL or missing references)
		//IL_116c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1179: Unknown result type (might be due to invalid IL or missing references)
		//IL_117e: Unknown result type (might be due to invalid IL or missing references)
		//IL_16b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_16cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_16da: Unknown result type (might be due to invalid IL or missing references)
		//IL_16df: Unknown result type (might be due to invalid IL or missing references)
		//IL_16ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_16f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_16fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1718: Unknown result type (might be due to invalid IL or missing references)
		//IL_171d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1727: Unknown result type (might be due to invalid IL or missing references)
		//IL_172c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1739: Unknown result type (might be due to invalid IL or missing references)
		//IL_173e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1760: Unknown result type (might be due to invalid IL or missing references)
		//IL_1778: Unknown result type (might be due to invalid IL or missing references)
		//IL_177d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1787: Unknown result type (might be due to invalid IL or missing references)
		//IL_178c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1799: Unknown result type (might be due to invalid IL or missing references)
		//IL_179e: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_17c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_17d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_17d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_17e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ea: Unknown result type (might be due to invalid IL or missing references)
		Viewport viewport = LogicalViewport;
		int width = viewport.Width;
		Viewport viewport2 = LogicalViewport;
		int height = viewport2.Height;
		bool flag = false;
		Rectangle val = default(Rectangle);
		val = new Rectangle(0, 0, backgroundTexture.Width, backgroundTexture.Height);
		Vector2 val2 = default(Vector2);
		val2 = new Vector2((float)(backgroundTexture.Width / 2), (float)(backgroundTexture.Height / 2));
		basicEffect.View = view;
		basicEffectRT.View = view;
		basicEffect.TextureEnabled = true;
		basicEffect.VertexColorEnabled = true;
		graphics.GraphicsDevice.SetRenderTarget(mainRenderTarget);
		graphics.GraphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin((SpriteSortMode)1, (BlendState)null, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)(object)basicEffect);
		if (drawBackground)
		{
			spriteBatch.Draw(backgroundTexture, val, (Rectangle?)null, Color.White, 0f, val2, (SpriteEffects)0, 0f);
		}
		if (editingAnimation)
		{
			hs.Draw(spriteBatch);
		}
		else
		{
			hs.DrawInitialSkeleton(spriteBatch, (float)Math.PI);
		}
		spriteBatch.End();
		basicEffectRT.TextureEnabled = false;
		basicEffectRT.VertexColorEnabled = false;
		((Effect)basicEffectRT).CurrentTechnique.Passes[0].Apply();
		if (debug)
		{
			if (editingAnimation)
			{
				hs.DrawDebug(spriteBatch);
			}
			else
			{
				hs.DrawDebugInitialSkeleton(spriteBatch, (float)Math.PI);
			}
		}
		graphics.GraphicsDevice.SetRenderTarget(animRenderTarget);
		graphics.GraphicsDevice.Clear(Color.Transparent);
		if (selectedAnimation != -1)
		{
			spriteBatch.Begin((SpriteSortMode)1, (BlendState)null, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)(object)basicEffect);
			BoneAnimation boneAnimation = hs.AnimationsList[animationList.Items[animationList.SelectedItems[0]]];
			List<KeyFrame> keyFrames = hs.GetKeyFrames(boneAnimation.Name);
			if (selectedKeyFrame != -1 && keyFrames.Count > selectedKeyFrame)
			{
				flag = true;
				if (drawBackground)
				{
					spriteBatch.Draw(backgroundTexture, val, (Rectangle?)null, Color.White, 0f, val2, (SpriteEffects)0, 0f);
				}
				hs.DrawAnimationAtFrame(Vector2.Zero, (float)Math.PI, boneAnimation.Name, selectedKeyFrame, spriteBatch);
			}
			spriteBatch.End();
			if (debug)
			{
				basicEffectRT.TextureEnabled = false;
				basicEffectRT.VertexColorEnabled = false;
				((Effect)basicEffectRT).CurrentTechnique.Passes[0].Apply();
				if (selectedKeyFrame != -1 && keyFrames.Count > selectedKeyFrame)
				{
					hs.DrawDebugAnimationAtFrame(Vector2.Zero, (float)Math.PI, boneAnimation.Name, selectedKeyFrame, spriteBatch);
				}
			}
		}
		// With a UI zoom the scene (and the GUI drawn after it) goes to a
		// low-resolution target that PresentScene scales up to the window.
		graphics.GraphicsDevice.SetRenderTarget(sceneTarget);
		((Game)this).GraphicsDevice.Clear(Color.CornflowerBlue);
		spriteBatch.Begin((SpriteSortMode)1, (BlendState)null, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)(object)basicEffect);
		string text = ((!editingAnimation) ? "Editing Skeleton" : "Editing Animations");
		spriteBatch.DrawString(spriteFontBold, text, new Vector2((float)(-width / 2 + 20), (float)(-height / 2 + 20)), Color.White, 0f, Vector2.Zero, 1f, (SpriteEffects)0, 0f);
		string text2 = "F1 to toggle debug bone view\nF2 to switch skeleton/animation edition\nF3 to toggle horizontal bounding box lines\nF4 to toggle vertical bounding box lines\nF5 to reload the sprites\nF6 to switch background on and off, F7 to zoom (" + uiScale + "x)";
		text2 = text2 + "\nProject Path: " + projectPath;
		text2 = text2 + "\nBase Path: " + basePath;
		string text3 = text2;
		text2 = text3 + "\nSkeleton File: " + skeletonFile;
		string text4 = text2;
		text2 = text4 + "\nSprites Subfolder: " + spritesSubPath;
		string text5 = text2;
		text2 = text5 + "\nAnimations File: " + animationsFile;
		spriteBatch.DrawString(spriteFont, text2, new Vector2((float)(-width / 2 + 20), (float)(-height / 2 + 60)), Color.White, 0f, Vector2.Zero, 1f, (SpriteEffects)0, 0f);
		// The bone info goes below the help/project text, whatever its height.
		float boneInfoY = -height / 2 + 60 + spriteFont.MeasureString(text2).Y + spriteFont.LineSpacing;
		hs.GetBonesNumber();
		new Vector2(200f, (float)(-height / 2 + 20));
		Color white = Color.White;
		Bone boneAt = hs.GetBoneAt(selectedBone);
		if (boneAt != null)
		{
			string text6 = ((boneAt.Parent != null) ? ("Selected Bone: " + boneAt.Name + "\nParent Bone: " + boneAt.Parent.Name) : ("Selected Bone: " + boneAt.Name));
			text6 = text6 + "\nBone Length: " + boneAt.Length;
			text6 = text6 + "\nBone Initial Angle: " + MathHelper.ToDegrees(boneAt.BaseAngle);
			text6 = text6 + "\nBone Angle Offset Limits: " + MathHelper.ToDegrees(boneAt.AngleOffset);
			text6 = text6 + "\nBone Initial Position: " + boneAt.OriginalPosition.X + ", " + boneAt.OriginalPosition.Y;
			spriteBatch.DrawString(spriteFont, text6, new Vector2((float)(-width / 2 + 20), boneInfoY), Color.White, 0f, Vector2.Zero, 1f, (SpriteEffects)0, 0f);
		}
		spriteBatch.End();
		spriteBatch.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, (Effect)(object)basicEffect);
		Vector2 val3 = new Vector2((float)width, (float)(-height)) / 2f;
		val3.X -= boundingBox.X * 2f + 130f;
		val3.Y += boundingBox.Y * 1.5f;
		spriteBatch.Draw((Texture2D)(object)mainRenderTarget, val3, (Rectangle?)null, Color.White, 0f, new Vector2((float)((Texture2D)mainRenderTarget).Width, (float)((Texture2D)mainRenderTarget).Height) / 2f, 2f, (SpriteEffects)0, 0f);
		Vector2 val4 = new Vector2((float)width, (float)height) / 2f;
		if (flag)
		{
			val4.X -= boundingBox.X * 2f + 130f;
			val4.Y -= boundingBox.Y * 1.5f;
			spriteBatch.Draw((Texture2D)(object)animRenderTarget, val4, (Rectangle?)null, Color.White, 0f, new Vector2((float)((Texture2D)animRenderTarget).Width, (float)((Texture2D)animRenderTarget).Height) / 2f, 2f, (SpriteEffects)0, 0f);
		}
		spriteBatch.End();
		spriteBatch.Begin((SpriteSortMode)1, (BlendState)null, (SamplerState)null, (DepthStencilState)null, (RasterizerState)null, (Effect)(object)basicEffect);
		if (drawBackground)
		{
			spriteBatch.Draw(backgroundTexture, val, (Rectangle?)null, Color.White, 0f, val2, (SpriteEffects)0, 0f);
		}
		if (editingAnimation)
		{
			hs.Draw(spriteBatch);
		}
		else
		{
			hs.DrawInitialSkeleton(spriteBatch, (float)Math.PI);
		}
		Vector2 val5 = default(Vector2);
		val5 = new Vector2((float)(-width / 2) + boundingBox.X * 1f, (float)(height / 2) - boundingBox.Y * 1f);
		white = Color.White;
		if (selectedAnimation != -1)
		{
			BoneAnimation boneAnimation2 = hs.AnimationsList[animationList.Items[animationList.SelectedItems[0]]];
			List<KeyFrame> keyFrames2 = hs.GetKeyFrames(boneAnimation2.Name);
			for (int i = 0; i < keyFrames2.Count; i++)
			{
				white = ((i != selectedKeyFrame) ? Color.White : Color.Red);
				spriteBatch.DrawString(spriteFont, "KF " + i, val5 - new Vector2(0f, 100f), white, 0f, Vector2.Zero, 1f, (SpriteEffects)0, 0f);
				spriteBatch.DrawString(spriteFont, Math.Round(keyFrames2[i].Time, 2).ToString(), val5 - new Vector2(0f, 80f), white, 0f, Vector2.Zero, 1f, (SpriteEffects)0, 0f);
				hs.DrawAnimationAtFrame(val5, (float)Math.PI, boneAnimation2.Name, i, spriteBatch);
				val5 += new Vector2(boundingBox.X * 2.5f, 0f);
			}
		}
		spriteBatch.End();
		basicEffect.TextureEnabled = false;
		basicEffect.VertexColorEnabled = false;
		((Effect)basicEffect).CurrentTechnique.Passes[0].Apply();
		if (debug)
		{
			if (editingAnimation)
			{
				hs.DrawDebug(spriteBatch);
			}
			else
			{
				hs.DrawDebugInitialSkeleton(spriteBatch, (float)Math.PI);
			}
		}
		if (drawHBoundingBox)
		{
			VertexPositionColor[] array = (VertexPositionColor[])(object)new VertexPositionColor[2];
			array[0].Position = new Vector3(new Vector2(0f - boundingBox.X, boundingBox.Y) / 2f, 0f);
			array[0].Color = Color.Black;
			array[1].Position = new Vector3(new Vector2(boundingBox.X, boundingBox.Y) / 2f, 0f);
			array[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
			array[0].Position = new Vector3(new Vector2(0f - boundingBox.X, 0f - boundingBox.Y) / 2f, 0f);
			array[0].Color = Color.Black;
			array[1].Position = new Vector3(new Vector2(boundingBox.X, 0f - boundingBox.Y) / 2f, 0f);
			array[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
			array[0].Position = new Vector3(val3 + new Vector2(0f - boundingBox.X, boundingBox.Y), 0f);
			array[0].Color = Color.Black;
			array[1].Position = new Vector3(val3 + new Vector2(boundingBox.X, boundingBox.Y), 0f);
			array[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
			array[0].Position = new Vector3(val3 + new Vector2(0f - boundingBox.X, 0f - boundingBox.Y), 0f);
			array[0].Color = Color.Black;
			array[1].Position = new Vector3(val3 + new Vector2(boundingBox.X, 0f - boundingBox.Y), 0f);
			array[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
			if (selectedAnimation != -1)
			{
				BoneAnimation boneAnimation3 = hs.AnimationsList[animationList.Items[animationList.SelectedItems[0]]];
				List<KeyFrame> keyFrames3 = hs.GetKeyFrames(boneAnimation3.Name);
				Vector2 val6 = default(Vector2);
				val6 = new Vector2((float)(-width / 2) + boundingBox.X * 1f, (float)(height / 2) - boundingBox.Y * 1f);
				Vector2 val7 = default(Vector2);
				for (int j = 0; j < keyFrames3.Count; j++)
				{
					val7 = new Vector2(val6.X - boundingBox.X / 2f, val6.Y - boundingBox.Y / 2f);
					Vector2 val8 = val7 + new Vector2(boundingBox.X, 0f);
					Vector2 val9 = val7 + new Vector2(0f, boundingBox.Y);
					Vector2 val10 = val7 + boundingBox;
					array = (VertexPositionColor[])(object)new VertexPositionColor[2];
					array[0].Position = new Vector3(val9, 0f);
					array[0].Color = Color.Black;
					array[1].Position = new Vector3(val10, 0f);
					array[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
					array[0].Position = new Vector3(val7, 0f);
					array[0].Color = Color.Black;
					array[1].Position = new Vector3(val8, 0f);
					array[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
					val6 += new Vector2(boundingBox.X * 2.5f, 0f);
				}
				if (flag)
				{
					array[0].Position = new Vector3(val4 + new Vector2(0f - boundingBox.X, boundingBox.Y), 0f);
					array[0].Color = Color.Black;
					array[1].Position = new Vector3(val4 + new Vector2(boundingBox.X, boundingBox.Y), 0f);
					array[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
					array[0].Position = new Vector3(val4 + new Vector2(0f - boundingBox.X, 0f - boundingBox.Y), 0f);
					array[0].Color = Color.Black;
					array[1].Position = new Vector3(val4 + new Vector2(boundingBox.X, 0f - boundingBox.Y), 0f);
					array[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array, 0, 1);
				}
			}
		}
		if (drawVBoundingBox)
		{
			VertexPositionColor[] array2 = (VertexPositionColor[])(object)new VertexPositionColor[2];
			array2[0].Position = new Vector3(new Vector2(0f - boundingBox.X, boundingBox.Y) / 2f, 0f);
			array2[0].Color = Color.Black;
			array2[1].Position = new Vector3(new Vector2(0f - boundingBox.X, 0f - boundingBox.Y) / 2f, 0f);
			array2[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
			array2[0].Position = new Vector3(new Vector2(boundingBox.X, boundingBox.Y) / 2f, 0f);
			array2[0].Color = Color.Black;
			array2[1].Position = new Vector3(new Vector2(boundingBox.X, 0f - boundingBox.Y) / 2f, 0f);
			array2[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
			array2 = (VertexPositionColor[])(object)new VertexPositionColor[2];
			array2[0].Position = new Vector3(val3 + new Vector2(0f - boundingBox.X, boundingBox.Y), 0f);
			array2[0].Color = Color.Black;
			array2[1].Position = new Vector3(val3 + new Vector2(0f - boundingBox.X, 0f - boundingBox.Y), 0f);
			array2[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
			array2[0].Position = new Vector3(val3 + new Vector2(boundingBox.X, boundingBox.Y), 0f);
			array2[0].Color = Color.Black;
			array2[1].Position = new Vector3(val3 + new Vector2(boundingBox.X, 0f - boundingBox.Y), 0f);
			array2[1].Color = Color.Black;
			((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
			if (selectedAnimation != -1)
			{
				BoneAnimation boneAnimation4 = hs.AnimationsList[animationList.Items[animationList.SelectedItems[0]]];
				List<KeyFrame> keyFrames4 = hs.GetKeyFrames(boneAnimation4.Name);
				Vector2 val11 = default(Vector2);
				val11 = new Vector2((float)(-width / 2) + boundingBox.X * 1f, (float)(height / 2) - boundingBox.Y * 1f);
				Vector2 val12 = default(Vector2);
				for (int k = 0; k < keyFrames4.Count; k++)
				{
					val12 = new Vector2(val11.X - boundingBox.X / 2f, val11.Y - boundingBox.Y / 2f);
					Vector2 val13 = val12 + new Vector2(boundingBox.X, 0f);
					Vector2 val14 = val12 + new Vector2(0f, boundingBox.Y);
					Vector2 val15 = val12 + boundingBox;
					array2 = (VertexPositionColor[])(object)new VertexPositionColor[2];
					array2[0].Position = new Vector3(val14, 0f);
					array2[0].Color = Color.Black;
					array2[1].Position = new Vector3(val12, 0f);
					array2[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
					array2[0].Position = new Vector3(val15, 0f);
					array2[0].Color = Color.Black;
					array2[1].Position = new Vector3(val13, 0f);
					array2[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
					val11 += new Vector2(boundingBox.X * 2.5f, 0f);
				}
				if (flag)
				{
					array2 = (VertexPositionColor[])(object)new VertexPositionColor[2];
					array2[0].Position = new Vector3(val4 + new Vector2(0f - boundingBox.X, boundingBox.Y), 0f);
					array2[0].Color = Color.Black;
					array2[1].Position = new Vector3(val4 + new Vector2(0f - boundingBox.X, 0f - boundingBox.Y), 0f);
					array2[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
					array2[0].Position = new Vector3(val4 + new Vector2(boundingBox.X, boundingBox.Y), 0f);
					array2[0].Color = Color.Black;
					array2[1].Position = new Vector3(val4 + new Vector2(boundingBox.X, 0f - boundingBox.Y), 0f);
					array2[1].Color = Color.Black;
					((GraphicsResource)spriteBatch).GraphicsDevice.DrawUserPrimitives<VertexPositionColor>((PrimitiveType)2, array2, 0, 1);
				}
			}
		}
		base.Draw(gameTime);
		PresentScene();
		DebugScreenshot.AfterDraw(((Game)this).GraphicsDevice);
	}

	public string ConvertKeyToChar(Keys key, bool shift)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected I4, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected I4, but got Unknown
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected I4, but got Unknown
		switch ((int)key - 9)
		{
		default:
			switch ((int)key - 186)
			{
			case 2:
				if (!shift)
				{
					return ",";
				}
				return "<";
			case 4:
				if (!shift)
				{
					return ".";
				}
				return ">";
			case 3:
				if (!shift)
				{
					return "-";
				}
				return "_";
			case 1:
				if (!shift)
				{
					return "=";
				}
				return "+";
			case 5:
				if (!shift)
				{
					return "/";
				}
				return "?";
			case 0:
				if (!shift)
				{
					return ";";
				}
				return ":";
			case 6:
				if (!shift)
				{
					return "`";
				}
				return "~";
			}
			switch ((int)key - 219)
			{
			case 0:
				if (!shift)
				{
					return "[";
				}
				return "{";
			case 2:
				if (!shift)
				{
					return "]";
				}
				return "}";
			case 3:
				if (!shift)
				{
					return "'";
				}
				return "\"";
			case 1:
				if (!shift)
				{
					return "\\";
				}
				return "|";
			}
			break;
		case 23:
			return " ";
		case 4:
			return "\n";
		case 0:
			return "\t";
		case 39:
			if (!shift)
			{
				return "0";
			}
			return ")";
		case 40:
			if (!shift)
			{
				return "1";
			}
			return "!";
		case 41:
			if (!shift)
			{
				return "2";
			}
			return "@";
		case 42:
			if (!shift)
			{
				return "3";
			}
			return "#";
		case 43:
			if (!shift)
			{
				return "4";
			}
			return "$";
		case 44:
			if (!shift)
			{
				return "5";
			}
			return "%";
		case 45:
			if (!shift)
			{
				return "6";
			}
			return "^";
		case 46:
			if (!shift)
			{
				return "7";
			}
			return "&";
		case 47:
			if (!shift)
			{
				return "8";
			}
			return "*";
		case 48:
			if (!shift)
			{
				return "9";
			}
			return "(";
		case 87:
			return "0";
		case 88:
			return "1";
		case 89:
			return "2";
		case 90:
			return "3";
		case 91:
			return "4";
		case 92:
			return "5";
		case 93:
			return "6";
		case 94:
			return "7";
		case 95:
			return "8";
		case 96:
			return "9";
		case 98:
			return "+";
		case 100:
			return "-";
		case 97:
			return "*";
		case 102:
			return "/";
		case 101:
			return ".";
		case 56:
			if (!shift)
			{
				return "a";
			}
			return "A";
		case 57:
			if (!shift)
			{
				return "b";
			}
			return "B";
		case 58:
			if (!shift)
			{
				return "c";
			}
			return "C";
		case 59:
			if (!shift)
			{
				return "d";
			}
			return "D";
		case 60:
			if (!shift)
			{
				return "e";
			}
			return "E";
		case 61:
			if (!shift)
			{
				return "f";
			}
			return "F";
		case 62:
			if (!shift)
			{
				return "g";
			}
			return "G";
		case 63:
			if (!shift)
			{
				return "h";
			}
			return "H";
		case 64:
			if (!shift)
			{
				return "i";
			}
			return "I";
		case 65:
			if (!shift)
			{
				return "j";
			}
			return "J";
		case 66:
			if (!shift)
			{
				return "k";
			}
			return "K";
		case 67:
			if (!shift)
			{
				return "l";
			}
			return "L";
		case 68:
			if (!shift)
			{
				return "m";
			}
			return "M";
		case 69:
			if (!shift)
			{
				return "n";
			}
			return "N";
		case 70:
			if (!shift)
			{
				return "o";
			}
			return "O";
		case 71:
			if (!shift)
			{
				return "p";
			}
			return "P";
		case 72:
			if (!shift)
			{
				return "q";
			}
			return "Q";
		case 73:
			if (!shift)
			{
				return "r";
			}
			return "R";
		case 74:
			if (!shift)
			{
				return "s";
			}
			return "S";
		case 75:
			if (!shift)
			{
				return "t";
			}
			return "T";
		case 76:
			if (!shift)
			{
				return "u";
			}
			return "U";
		case 77:
			if (!shift)
			{
				return "v";
			}
			return "V";
		case 78:
			if (!shift)
			{
				return "w";
			}
			return "W";
		case 79:
			if (!shift)
			{
				return "x";
			}
			return "X";
		case 80:
			if (!shift)
			{
				return "y";
			}
			return "Y";
		case 81:
			if (!shift)
			{
				return "z";
			}
			return "Z";
		case 1:
		case 2:
		case 3:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 99:
			break;
		}
		return string.Empty;
	}
}
