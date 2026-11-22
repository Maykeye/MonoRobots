using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoRobots;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private GameMap gameMap;

    Texture2D groundTexture;
    Texture2D robotTexture;
    Texture2D garboTexture;
    Texture2D playerTexture;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        gameMap = new GameMap();
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        ResetMap();
    }

    void ResetMap()
    {
        var robots = gameMap.Size.X * gameMap.Size.Y / 2;
        gameMap.Reset(robots);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        groundTexture = Texture2D.FromFile(GraphicsDevice, "Assets/Ground.png");
        // TODO: use this.Content to load your game content here (robo, garbo, ply)

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }


    /// <summary>
    /// Return viewport a size of a single tile in pixels for the viewport assuming VP completely covered in tiles.
    /// </summary>
    private Vector2 ViewportTileSize => new(GraphicsDevice.Viewport.Width / gameMap.Size.X, GraphicsDevice.Viewport.Height / gameMap.Size.Y);

    /// <summary>
    /// Return position a single tile in pixels for the viewport assuming VP completely covered in tiles.
    /// </summary>
    private Vector2 ViewportTilePos(int x, int y)
    {
        return new(ViewportTileSize.X * x, ViewportTileSize.Y * y);
    }

    /// <summary>
    /// Draw ground
    /// </summary>
    private void DrawGround()
    {
        var tileSz = ViewportTileSize;

        // Draw ground
        for (int y = 0; y < gameMap.Size.Y; y++)
        {
            for (int x = 0; x < gameMap.Size.X; x++)
            {
                var tilePos = ViewportTilePos(x, y);
                var tileRect = new Rectangle((int)tilePos.X, (int)tilePos.Y, (int)tileSz.X, (int)tileSz.Y);
                spriteBatch.Draw(groundTexture, tileRect, Color.White);
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // In viewport pixels

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawGround();
        spriteBatch.End();

        base.Draw(gameTime);
    }
}
