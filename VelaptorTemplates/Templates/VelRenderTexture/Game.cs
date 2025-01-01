using Velaptor.Input;

namespace VelRenderTexture;

using System.Numerics;
using Velaptor;
using Velaptor.Batching;
using Velaptor.Content;
using Velaptor.ExtensionMethods;
using Velaptor.Factories;
using Velaptor.Graphics.Renderers;
using Velaptor.UI;

/// <summary>
/// The main game class.
/// </summary>
public class Game : Window
{
	private const float MaxVel = 350;
	private readonly IBatcher batcher;
	private readonly ITextureRenderer textureRenderer;
	private readonly ILoader<ITexture> textureLoader;
	private readonly IAppInput<KeyboardState> keyboard;
	private ITexture? logo;
	private KeyboardState prevKeyState;
	private Vector2 position;
	private Vector2 velocity = new(0, 0);
	private const float VelocityX = 50f;
	private const float VelocityY = 50f;
	private readonly Vector2 minVelocity = new (-MaxVel, -MaxVel);
	private readonly Vector2 maxVelocity = new (MaxVel, MaxVel);
	private bool leftKeyDown;
	private bool rightKeyDown;
	private bool upKeyDown;
	private bool downKeyDown;

	/// <summary>
	/// Create useful content loaders, renderers, and input detectors for the game here.
	/// </summary>
	public Game()
	{
		// Used for rendering textures in a batch.  This is required.
		this.batcher = RendererFactory.CreateBatcher();

		// Used for rendering textures.
		this.textureRenderer = RendererFactory.CreateTextureRenderer();

		// Used for loading textures.
		this.textureLoader = ContentLoaderFactory.CreateTextureLoader();

		// Used for detecting keyboard input.
		this.keyboard = HardwareFactory.GetKeyboard();
	}

	/// <summary>
	/// Load content here.
	/// </summary>
	protected override void OnLoad()
	{
		// This loads the 'velaptor-logo' texture from the 'Content/Graphics' directory located in the project.
		this.logo = this.textureLoader.Load("velaptor-logo");

		// Set the starting position of the logo to the center of the window.
		this.position = new Vector2(Width / 2f, Height / 2f);

		base.OnLoad();
	}

	/// <summary>
	/// Add game logic here.
	/// </summary>
	/// <param name="frameTime">The amount of time that passed for the current game loop frame.</param>
	protected override void OnUpdate(FrameTime frameTime)
	{
		var currentKeyState = this.keyboard.GetState();

		// Record the state of the arrow keys.
		this.leftKeyDown = currentKeyState.IsKeyDown(KeyCode.Left);
		this.rightKeyDown = currentKeyState.IsKeyDown(KeyCode.Right);
		this.upKeyDown = currentKeyState.IsKeyDown(KeyCode.Up);
		this.downKeyDown = currentKeyState.IsKeyDown(KeyCode.Down);

		// Check if movement is or is not happening.
		var isNotMovingHorizontally = currentKeyState.IsKeyUp(KeyCode.Right) && currentKeyState.IsKeyUp(KeyCode.Left);
		var isNotMovingVertically = currentKeyState.IsKeyUp(KeyCode.Up) && currentKeyState.IsKeyUp(KeyCode.Down);

		// Increase velocity in each direction based on which keys are pressed.
		this.velocity.X -= this.leftKeyDown ? VelocityX : 0;
		this.velocity.X += this.rightKeyDown ? VelocityX : 0;
		this.velocity.Y -= this.upKeyDown ? VelocityY : 0;
		this.velocity.Y += this.downKeyDown ? VelocityY : 0;

		// Stop moving if the left or right key is no longer being pressed.
		this.velocity.X = isNotMovingHorizontally ? 0 : this.velocity.X;
		this.velocity.Y = isNotMovingVertically ? 0 : this.velocity.Y;

		// Limit the maximum velocity in any direction.
		this.velocity = Vector2.Clamp(this.velocity, this.minVelocity, this.maxVelocity);

		// Calculate the distance to move the logo.
		var deltaVel = this.velocity * (float)frameTime.ElapsedTime.TotalSeconds;

		// Move the logo.
		this.position += deltaVel;

		// Save the current key state for the next frame.
		this.prevKeyState = currentKeyState;

		base.OnUpdate(frameTime);
	}

	/// <summary>
	/// Render graphics here.
	/// </summary>
	/// <param name="frameTime">The amount of time that passed for the current game loop frame.</param>
	protected override void OnDraw(FrameTime frameTime)
	{
		if (this.logo is null)
		{
			throw new InvalidOperationException("The logo texture is not loaded.");
		}

		// This must be called first before rendering anything.
		this.batcher.Begin();

		// Render the logo texture in the center of the window.
		this.textureRenderer.Render(this.logo, this.position);

		// This must be called for the batch of render calls to be rendered.
		this.batcher.End();

		base.OnDraw(frameTime);
	}
}
