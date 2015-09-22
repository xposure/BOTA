using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Trix
{
    //https://www.giawa.com


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
        BasicEffect wireFrame;
        //Matrix worldMatrix;
        //Matrix viewMatrix;
        //Matrix projectionMatrix;
        //float zoom = 25;
        MouseState mouseState;
        KeyboardState keyboardState;
        Camera camera;
        bool wireFrameEnabled = false;

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

            _chunkManager = new ChunkManager(this.GraphicsDevice);

            //var data = new uint[3 * 3 *3];
            //for(var i =0;i < data.Length;i++){
            //    if((i % 4) == 0)
            //        data[i] = 0xffffff;
            //}

            //_volume = new Volume(_chunkManager, 0, 0, 0, data, new Dimensions(new int[] { 3, 3, 3 }));

            camera = new Camera(this.GraphicsDevice);
         
            //float tilt = MathHelper.ToRadians(0);  // 0 degree angle
            //// Use the world matrix to tilt the cube along x and y axes.
            //worldMatrix = Matrix.CreateRotationX(tilt) * Matrix.CreateRotationY(tilt);
            //viewMatrix = Matrix.CreateLookAt(new Vector3(zoom, zoom, zoom), Vector3.Zero, Vector3.Up);

            //projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            //    MathHelper.ToRadians(45),  // 45 degree angle
            //    (float)GraphicsDevice.Viewport.Width /
            //    (float)GraphicsDevice.Viewport.Height,
            //    1.0f, 10000.0f);


            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.Identity;
            basicEffect.Projection = camera.Projection;

            // primitive color
            basicEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            basicEffect.SpecularPower = 5.0f;
            basicEffect.Alpha = 1.0f;
            basicEffect.VertexColorEnabled = true;
            //basicEffect.FogEnabled = true;
            //basicEffect.FogColor = new Vector3(0, 0, 0);
            //basicEffect.FogStart = 10;
            //basicEffect.FogEnd = 100;
            wireFrame = new BasicEffect(graphics.GraphicsDevice);

            wireFrame.World = Matrix.Identity;
            wireFrame.View = Matrix.Identity;
            wireFrame.Projection = camera.Projection;

            // primitive color
            wireFrame.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            wireFrame.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            wireFrame.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
            wireFrame.SpecularPower = 5.0f;
            wireFrame.Alpha = 1.0f;
            wireFrame.VertexColorEnabled = false;
            wireFrame.LightingEnabled = true;

            basicEffect.LightingEnabled = true;
            if (basicEffect.LightingEnabled)
            {
                basicEffect.DirectionalLight0.Enabled = true; // enable each light individually
                if (basicEffect.DirectionalLight0.Enabled)
                {
                    // x direction
                    basicEffect.AmbientLightColor = new Vector3(1, 1, 1);
                    basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f); // range is 0 to 1
                    basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.25f, -1, -0.5f));
                    // points from the light to the origin of the scene
                    basicEffect.DirectionalLight0.SpecularColor = Vector3.Zero;
                    //basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
                }

                //basicEffect.DirectionalLight1.Enabled = true;
                //if (basicEffect.DirectionalLight1.Enabled)
                //{
                //    // y direction
                //    basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0, 0.75f, 0);
                //    basicEffect.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0, -1, 0));
                //    basicEffect.DirectionalLight1.SpecularColor = Vector3.One;
                //}

                //basicEffect.DirectionalLight2.Enabled = true;
                //if (basicEffect.DirectionalLight2.Enabled)
                //{
                //    // z direction
                //    basicEffect.DirectionalLight2.DiffuseColor = new Vector3(0, 0, 0.5f);
                //    basicEffect.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(0, 0, -1));
                //    basicEffect.DirectionalLight2.SpecularColor = Vector3.One;
                //}
            }

            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();

            _chunkManager.Initialize();
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

        bool isRunning = true;
        float light;

        protected override void OnExiting(object sender, System.EventArgs args)
        {
            isRunning = false;
            base.OnExiting(sender, args);
        }

        protected override void Update(GameTime gameTime)
        {
            var newKeyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || newKeyboardState.IsKeyDown(Keys.Escape))
                Exit();
            else if(isRunning && this.Window != null)
            {
                if (newKeyboardState.IsKeyUp(Keys.F) && keyboardState.IsKeyDown(Keys.F))
                    wireFrameEnabled = !wireFrameEnabled;
                keyboardState = newKeyboardState;

                if (newKeyboardState.IsKeyDown(Keys.L)) {
                    if (basicEffect.LightingEnabled)
                    {
                        basicEffect.DirectionalLight0.Enabled = true; // enable each light individually
                        if (basicEffect.DirectionalLight0.Enabled)
                        {
                            light +=  (float)gameTime.ElapsedGameTime.TotalSeconds;
                            if (light > 1f)
                                light = 0f;

                            basicEffect.AmbientLightColor = new Vector3(1, 1, 1);
                            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f); // range is 0 to 1
                            basicEffect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.25f, -1, -0.5f));
                            // points from the light to the origin of the scene
                            basicEffect.DirectionalLight0.SpecularColor = Vector3.Zero;
                            //basicEffect.DirectionalLight0.SpecularColor = Vector3.One;
                        }
                    }
                }
                // TODO: Add your update logic here

                var newMouseState = Mouse.GetState();

                camera.Update(this, gameTime, newKeyboardState, newMouseState);

                basicEffect.View = camera.View;
                wireFrame.View = camera.View;

                //if (mouseState.ScrollWheelValue != newMouseState.ScrollWheelValue)
                //{
                //    var cameraPosition = new Vector3(0, 0, 0);
                //    //var cameraPosition = new Vector3(gridSize * 8, 0, gridSize * 8);
                //    zoom = MathHelper.Clamp(zoom + (mouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) / 50, 10, 9000);
                //    viewMatrix = Matrix.CreateLookAt(new Vector3(0.333f, 0.333f, 0.333f) * zoom + cameraPosition, cameraPosition, Vector3.Up);
                //    basicEffect.View = viewMatrix;
                //    wireFrame.View = viewMatrix;
                //}
                mouseState = newMouseState;
            }
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
                //for (var x = 0; x < gridSize; x++)
                //    for (var z = 0; z < gridSize; z++)
                //        vertexCount += grid[x, z].opaqueMesh.PrimitiveCount;

                System.Diagnostics.Trace.WriteLine(1 / gameTime.ElapsedGameTime.TotalSeconds + ":" + vertexCount);
            }

            var culled = _chunkManager.Draw(gameTime, basicEffect, wireFrameEnabled ? wireFrame : null, camera);
            System.Diagnostics.Trace.WriteLine(culled);
            base.Draw(gameTime);
        }
    }
}
