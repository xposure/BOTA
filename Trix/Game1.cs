using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Trix.Map;

namespace Trix
{
    //https://www.giawa.com
    //http://devblog.andyc.org/2011/06/ - networking post and resources

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
        SpriteFont arialFont;
        //Matrix worldMatrix;
        //Matrix viewMatrix;
        //Matrix projectionMatrix;
        //float zoom = 25;
        MouseState mouseState;
        KeyboardState keyboardState;
        Camera camera;
        private World world;
        bool wireFrameEnabled = false;

        private VoxelVolume selection;
        public static int verticesRendered = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 768;
            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
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

            //_chunkManager.Initialize();
            var generator = new DefaultWorldGenerator();
            world = new World(GraphicsDevice, 192, 128);
            world.Generate(generator);

            arialFont = Content.Load<SpriteFont>("fonts/arial");

            selection = new VoxelVolume(this.GraphicsDevice, new Dimensions(new int[] { world.Size, 1, world.Size }));
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
            else if (isRunning && this.Window != null)
            {
                if (newKeyboardState.IsKeyUp(Keys.F) && keyboardState.IsKeyDown(Keys.F))
                    wireFrameEnabled = !wireFrameEnabled;
                keyboardState = newKeyboardState;

                if (newKeyboardState.IsKeyDown(Keys.L))
                {
                    if (basicEffect.LightingEnabled)
                    {
                        basicEffect.DirectionalLight0.Enabled = true; // enable each light individually
                        if (basicEffect.DirectionalLight0.Enabled)
                        {
                            light += (float)gameTime.ElapsedGameTime.TotalSeconds;
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

        private double lastFrameTime = 0.0;
        private List<string> debugText = new List<string>(32);

        public void AddDebugText(string text)
        {
            debugText.Add(text);
        }

        public void AddDebugText(string text, params object[] args)
        {
            AddDebugText(string.Format(text, args));
        }

        protected override void Draw(GameTime gameTime)
        {
            verticesRendered = 0;

            lastFrameTime = (gameTime.ElapsedGameTime.TotalSeconds * 0.0005 + lastFrameTime * 0.9995);
            var fps = (int)(1.0 / lastFrameTime);

            debugText.Clear();

            AddDebugText("FPS: " + fps);
            AddDebugText("Position: " + camera.TargetPosition.ToString());
            AddDebugText("Direction: " + camera.Direction.ToString());
            AddDebugText("Zoom: " + camera.Zoom);

            basicEffect.Projection = camera.Projection;
            wireFrame.Projection = camera.Projection;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (wireFrameEnabled)
            {
                var rast = new RasterizerState();
                rast.FillMode = FillMode.WireFrame;
                rast.CullMode = CullMode.None;
                GraphicsDevice.RasterizerState = rast;
            }
            else
                GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            this.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //var culled = _chunkManager.Draw(gameTime, basicEffect, wireFrameEnabled ? wireFrame : null, camera);
            world.Render(basicEffect, camera);
            //System.Diagnostics.Trace.WriteLine(culled);


            AddDebugText("Vertices: " + verticesRendered);

            selection.Clear();

            var dir = camera.Direction;
            var start = camera.Position;
            var end = camera.Position + dir * 100;
            var foundCell = false;
            var cellPosition = Vector3.Zero;
            foreach (var p in GridRayTracer.Trace(start, end))
            {
                var x = (int)p.X;
                var y = (int)p.Y;
                var z = (int)p.Z;

                var cell = world[x, z, y];
                if (!cell.Meta.IsEmpty)
                {
                    selection[x, 0, y] = 0x00ffff;
                    foundCell = true;
                    cellPosition = p;
                    Rendering.SurfaceExtractor.ExtractMesh(selection, disableAO: true);
                    break;
                }
            }

            AddDebugText("Ray {{ start: {0}, end: {1}, hit: {2} }}", start, end, cellPosition);

            spriteBatch.Begin();
            for (var i = 0; i < debugText.Count; i++)
                spriteBatch.DrawString(arialFont, debugText[i], new Vector2(0, i * arialFont.LineSpacing), Color.White);
            spriteBatch.End();

            if (foundCell)
            {
                basicEffect.Alpha = 0.5f;
                basicEffect.World = Matrix.CreateTranslation(new Vector3(0, cellPosition.Y, 0));

                this.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    selection.opaqueMesh.Draw();
                }
            }

            base.Draw(gameTime);
        }
    }
}
