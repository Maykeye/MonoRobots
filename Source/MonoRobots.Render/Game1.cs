using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoRobots.Engine;
namespace MonoRobots.Render;

public class Game1 : Game
{
    internal GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private readonly GameMap gameMap;

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
        var robots = gameMap.Size.X * gameMap.Size.Y / 32;
        gameMap.Reset(robots);
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        groundTexture = Texture2D.FromFile(GraphicsDevice, "Assets/Ground.png");
        robotTexture = Texture2D.FromFile(GraphicsDevice, "Assets/Robot.png");
        garboTexture = Texture2D.FromFile(GraphicsDevice, "Assets/Garbage.png");
        playerTexture = Texture2D.FromFile(GraphicsDevice, "Assets/Player.png");
    }

    private KeyboardState curKbd;
    private KeyboardState lastKbd;

    private bool IsPressed(Keys key)
    {
        if (lastKbd.IsKeyDown(key))
            return false;
        return curKbd.IsKeyDown(key);
    }

    protected override void Update(GameTime gameTime)
    {
        curKbd = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || IsPressed(Keys.Escape))
            Exit();

        // Movement
        PlayerTurn turn = null;
        // TODO: cfg?
        if (IsPressed(Keys.Down)) turn = new PlayerTurn.Move(new Point(0, 1));
        if (IsPressed(Keys.Up)) turn = new PlayerTurn.Move(new Point(0, -1));
        if (IsPressed(Keys.Left)) turn = new PlayerTurn.Move(new Point(-1, 0));
        if (IsPressed(Keys.Right)) turn = new PlayerTurn.Move(new Point(1, 0));


        if (IsPressed(Keys.NumPad8)) turn = new PlayerTurn.Move(new Point(0, -1));
        if (IsPressed(Keys.NumPad2)) turn = new PlayerTurn.Move(new Point(0, 1));
        if (IsPressed(Keys.NumPad4)) turn = new PlayerTurn.Move(new Point(-1, 0));
        if (IsPressed(Keys.NumPad6)) turn = new PlayerTurn.Move(new Point(1, 0));

        if (IsPressed(Keys.NumPad1)) turn = new PlayerTurn.Move(new Point(-1, 1));
        if (IsPressed(Keys.NumPad3)) turn = new PlayerTurn.Move(new Point(1, 1));
        if (IsPressed(Keys.NumPad7)) turn = new PlayerTurn.Move(new Point(-1, -1));
        if (IsPressed(Keys.NumPad9)) turn = new PlayerTurn.Move(new Point(1, -1));

        if (IsPressed(Keys.Space)) turn = new PlayerTurn.Teleport();
        if (IsPressed(Keys.OemPeriod)) turn = new PlayerTurn.DoNothing();
        if (IsPressed(Keys.NumPad5)) turn = new PlayerTurn.DoNothing();
        if (IsPressed(Keys.F12))
        {
            ResetMap();
            turn = null;
        }
        if (turn is not null)
        {
            gameMap.Step(turn);
        }


        // TODO: Add your update logic here

        base.Update(gameTime);

        lastKbd = curKbd;
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

    private void DrawEnts(CellKind kind, Texture2D texture)
    {
        var tileSz = ViewportTileSize;
        foreach (var robot in gameMap.FindKind(kind))
        {
            var tilePos = ViewportTilePos(robot.Position.X, robot.Position.Y);
            var tileRect = new Rectangle((int)tilePos.X, (int)tilePos.Y, (int)tileSz.X, (int)tileSz.Y);
            spriteBatch.Draw(texture, tileRect, Color.White);
        }
    }

    private void DrawPlayer()
    {
        var tileSz = ViewportTileSize;
        var ply = gameMap.GetPlayer();
        var tilePos = ViewportTilePos(ply.Position.X, ply.Position.Y);
        var tileRect = new Rectangle((int)tilePos.X, (int)tilePos.Y, (int)tileSz.X, (int)tileSz.Y);

        if (ply.Alive)
        {
            spriteBatch.Draw(playerTexture, tileRect, Color.White);
        }
        else
        {
            spriteBatch.Draw(
                playerTexture,
                tileRect,
                null,
                Color.Red,
                0f,
                Vector2.Zero,
                SpriteEffects.FlipVertically,
                0f
            );
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // In viewport pixels
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawGround();
        DrawEnts(CellKind.Garbage, garboTexture);
        DrawEnts(CellKind.Robot, robotTexture);
        DrawPlayer();
        spriteBatch.End();

        base.Draw(gameTime);
    }
}
