using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Trix
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ChunkManager _chunkManager;
        //Volume _volume;
        BasicEffect basicEffect;
        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        float zoom = 25;
        MouseState mouseState;
        KeyboardState keyboardState;
        bool wireFrame = false;

        private const int gridSize = 64;
        Volume[,] grid = new Volume[gridSize, gridSize];

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            _chunkManager = new ChunkManager();

            //var data = new uint[3 * 3 *3];
            //for(var i =0;i < data.Length;i++){
            //    if((i % 4) == 0)
            //        data[i] = 0xffffff;
            //}

            //_volume = new Volume(_chunkManager, 0, 0, 0, data, new Dimensions(new int[] { 3, 3, 3 }));

            var terrainTimer = new Stopwatch();
            var surfaceTimer = new Stopwatch();

            var CHUNK_SIZE = 16;
            var sealevel = 8;
            var noise = NoiseType.Perlin;
            for (var x = 0; x < gridSize; x++)
            {
                for (var z = 0; z < gridSize; z++)
                {
                    terrainTimer.Start();
                    grid[x, z] = SurfaceExtractor.makeVoxels(_chunkManager, x * CHUNK_SIZE, 0, z * CHUNK_SIZE,
                         new int[] { 0, 0, 0 },
                         new int[] { CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE },
                            Generators.GenerateHeight(x, 0, z, CHUNK_SIZE, CHUNK_SIZE, noise, sealevel)
                         );
                    terrainTimer.Stop();

                    grid[x, z]._device = this.GraphicsDevice;

                    surfaceTimer.Start();
                    var count = SurfaceExtractor.GenerateMesh2(this.GraphicsDevice, null, grid[x, z], centered: true);
                    surfaceTimer.Stop();
                }
            }

            System.Diagnostics.Trace.WriteLine("Terrain: " + terrainTimer.Elapsed.ToString());
            System.Diagnostics.Trace.WriteLine("Surface: " + surfaceTimer.Elapsed.ToString());

            float tilt = MathHelper.ToRadians(0);  // 0 degree angle
            // Use the world matrix to tilt the cube along x and y axes.
            worldMatrix = Matrix.CreateRotationX(tilt) * Matrix.CreateRotationY(tilt);
            viewMatrix = Matrix.CreateLookAt(new Vector3(zoom, zoom, zoom), Vector3.Zero, Vector3.Up);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45),  // 45 degree angle
                (float)GraphicsDevice.Viewport.Width /
                (float)GraphicsDevice.Viewport.Height,
                1.0f, 10000.0f);

            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;

            // primitive color
            basicEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            basicEffect.SpecularPower = 5.0f;
            basicEffect.Alpha = 1.0f;
            basicEffect.VertexColorEnabled = true;

            //basicEffect.LightingEnabled = true;
            //if (basicEffect.LightingEnabled)
            //{
            //    basicEffect.DirectionalLight0.Enabled = true; // enable each light individually
            //    if (basicEffect.DirectionalLight0.Enabled)
            //    {
            //        // x direction
            //        basicEffect.DirectionalLight0.DiffuseColor = new Vector3(1, 0, 0); // range is 0 to 1
            //        basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1, 0, 0));
            //        // points from the light to the origin of the scene
            //        basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
            //    }

            //    basicEffect.DirectionalLight1.Enabled = true;
            //    if (basicEffect.DirectionalLight1.Enabled)
            //    {
            //        // y direction
            //        basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.75f, 0);
            //        basicEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0, -1, 0));
            //        basicEffect.DirectionalLight1.SpecularColor = Vector3.One;
            //    }

            //    basicEffect.DirectionalLight2.Enabled = true;
            //    if (basicEffect.DirectionalLight2.Enabled)
            //    {
            //        // z direction
            //        basicEffect.DirectionalLight2.DiffuseColor = new Vector3(0, 0, 0.5f);
            //        basicEffect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, 0, -1));
            //        basicEffect.DirectionalLight2.SpecularColor = Vector3.One;
            //    }
            //}

            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var newKeyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || newKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (newKeyboardState.IsKeyUp(Keys.F) && keyboardState.IsKeyDown(Keys.F))
                wireFrame = !wireFrame;
            keyboardState = newKeyboardState;

            // TODO: Add your update logic here

            var newMouseState = Mouse.GetState();
            if (mouseState.ScrollWheelValue != newMouseState.ScrollWheelValue)
            {
                var cameraPosition = new Vector3(0, 0, 0);
                //var cameraPosition = new Vector3(gridSize * 8, 0, gridSize * 8);
                zoom = MathHelper.Clamp(zoom + (mouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) / 50, 10, 9000);
                viewMatrix = Matrix.CreateLookAt(new Vector3(0.333f, 0.333f, 0.333f) * zoom + cameraPosition, cameraPosition, Vector3.Up);
                basicEffect.View = viewMatrix;
            }
            mouseState = newMouseState;

            base.Update(gameTime);
        }

        private int lastSecond = 0;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            var currentSecond = (int)gameTime.TotalGameTime.TotalSeconds;
            if (lastSecond != currentSecond)
            {
                lastSecond = currentSecond;
                var vertexCount = 0;
                for (var x = 0; x < gridSize; x++)
                    for (var z = 0; z < gridSize; z++)
                        vertexCount += grid[x, z].opaqueMesh.PrimitiveCount;

                System.Diagnostics.Trace.WriteLine(1 / gameTime.ElapsedGameTime.TotalSeconds + ":" + vertexCount);
            }

            var rast = new RasterizerState();
            rast.FillMode = wireFrame ? FillMode.WireFrame : FillMode.Solid;
            GraphicsDevice.RasterizerState = rast;

            // TODO: Add your drawing code here
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                for (var x = 0; x < gridSize; x++)
                {
                    for (var z = 0; z < gridSize; z++)
                    {
                        //basicEffect.World = worldMatrix * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds / 2);
                        basicEffect.World = worldMatrix * Matrix.CreateTranslation(new Vector3(x * 16 - (gridSize * 16 / 2), 0, z * 16 - (gridSize * 16 / 2))) * Matrix.CreateRotationY((float)gameTime.TotalGameTime.TotalSeconds / 2);
                        pass.Apply();
                        grid[x, z].opaqueMesh.Draw();
                    }
                }
            }

            base.Draw(gameTime);
        }
    }
}
