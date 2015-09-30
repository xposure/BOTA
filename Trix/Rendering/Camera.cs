using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Trix
{
    public class Camera
    {
        private Matrix projection = Matrix.Identity;
        private Matrix view = Matrix.Identity;

        private Vector3 position = new Vector3(100, -10, 100);
        private Vector3 angle = new Vector3();
        private float moveSpeed = 15f;
        private float turnSpeed = 25f;
        private BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);

        public BoundingFrustum Frustum { get { return frustum; } }

        public Matrix Projection
        {
            get { return projection; }
        }

        public Matrix View
        {
            get { return view; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public float Zoom
        {
            get { return zoomDistance; }
        }

        public int Depth
        {
            get { return (int)position.Y; }
            set { position.Y = MathHelper.Clamp(value, 1, 128); }
        }

        public Camera(GraphicsDevice device)
        {
            float ratio = (float)device.Viewport.Width / (float)device.Viewport.Height;
            projection = Matrix.CreateOrthographic(device.Viewport.Width / 8, device.Viewport.Height / 8, 0, 1000);
            //projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, ratio, 0.1f, 10000);

            var mouse = Mouse.GetState();
            mousePosition = new Vector2(mouse.Position.X, mouse.Position.Y);
            mouseWheel = mouse.ScrollWheelValue;
            Depth = 60;
            zoomDistance = 15;
        }

        public void Update(Game game, GameTime gameTime, KeyboardState keyboard, MouseState mouse)
        {
            if (game != null && game.IsActive)
            {
                //fpsView(game, gameTime, keyboard, mouse);
                overheadView(game, gameTime, keyboard, mouse);
            }
            mousePosition = new Vector2(mouse.Position.X, mouse.Position.Y);
            mouseWheel = mouse.ScrollWheelValue;
        }

        int mouseWheel;
        float avatarYaw = (float)Math.PI / 4f;
        float zoomDistance = 25;
        Vector2 mousePosition;
        Vector3 thirdPersonReference = new Vector3(0, 1, -1);

        private void overheadView(Game game, GameTime gameTime, KeyboardState keyboard, MouseState mouse)
        {
            game.IsMouseVisible = true;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var moveVector = Vector3.Zero;
            var newMousePosition = new Vector2(mouse.Position.X, mouse.Position.Y);
            var mouseDelta = mousePosition - newMousePosition;

            var newScrollWheel = mouse.ScrollWheelValue;
            var mouseScrollDelta = mouseWheel - newScrollWheel;
            //swivel around point
            if (mouse.RightButton == ButtonState.Pressed)
            {
                if (mouseDelta.X != 0)
                    avatarYaw += mouseDelta.X * dt;

                if (mouseDelta.Y != 0)
                    thirdPersonReference.Y = MathHelper.Clamp(thirdPersonReference.Y - mouseDelta.Y * dt, 1, 2);
            }
            else if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (keyboard.IsKeyDown(Keys.LeftShift))
                {
                    if (mouseDelta.X != 0)
                        avatarYaw += mouseDelta.X * dt;

                    if (mouseDelta.Y != 0)
                        thirdPersonReference.Y = MathHelper.Clamp(thirdPersonReference.Y - mouseDelta.Y * dt, 1, 2);
                }
                else
                {
                    moveVector.X -= mouseDelta.X / zoomDistance;
                    moveVector.Z -= mouseDelta.Y / zoomDistance;
                }
            }
            else if (mouseScrollDelta != 0)
            {
                if (keyboard.IsKeyDown(Keys.LeftShift))
                {
                    if (mouseScrollDelta != 0)
                        zoomDistance = MathHelper.Clamp(zoomDistance - dt * mouseScrollDelta, 10, 100);
                }
                else
                {
                    if (mouseScrollDelta > 0)
                        position.Y--;
                    else
                        position.Y++;
                }
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.D))
                    moveVector.X -= moveSpeed * dt;
                if (keyboard.IsKeyDown(Keys.A))
                    moveVector.X += moveSpeed * dt;
                if (keyboard.IsKeyDown(Keys.W))
                    moveVector.Z += moveSpeed * dt;
                if (keyboard.IsKeyDown(Keys.S))
                    moveVector.Z -= moveSpeed * dt;
            }

            projection = Matrix.CreateOrthographic(game.GraphicsDevice.Viewport.Width / zoomDistance, game.GraphicsDevice.Viewport.Height / zoomDistance, -100, 100);

            Matrix rotationMatrix = Matrix.CreateRotationY(avatarYaw);

            if (moveVector.LengthSquared() != 0)
                position += Vector3.Transform(moveVector, rotationMatrix);

            // Create a vector pointing the direction the camera is facing.
            Vector3 transformedReference =
                Vector3.Transform(Vector3.Normalize(thirdPersonReference), rotationMatrix);

            transformedReference *= zoomDistance;

            // Calculate the position the camera is looking from.
            Vector3 cameraPosition = transformedReference + position;

            view = Matrix.CreateLookAt(cameraPosition, position, new Vector3(0.0f, 1.0f, 0.0f));

            frustum.Matrix = View * Projection;
        }

        private void fpsView(Game game, GameTime gameTime, KeyboardState keyboard, MouseState mouse)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            game.IsMouseVisible = false;

            int centerX = game.Window.ClientBounds.Width / 2;
            int centerY = game.Window.ClientBounds.Height / 2;
            if (game.IsActive)
                Mouse.SetPosition(centerX, centerY);

            angle.X += MathHelper.ToRadians((mouse.Y - centerY) * turnSpeed * 0.01f); // pitch
            angle.Y += MathHelper.ToRadians((mouse.X - centerX) * turnSpeed * 0.01f); // yaw

            Vector3 forward = Vector3.Normalize(new Vector3((float)Math.Sin(-angle.Y), (float)Math.Sin(angle.X), (float)Math.Cos(-angle.Y)));
            Vector3 left = Vector3.Normalize(new Vector3((float)Math.Cos(angle.Y), 0f, (float)Math.Sin(angle.Y)));

            var adjustedMoveSpeed = moveSpeed;
            if (keyboard.IsKeyDown(Keys.LeftShift))
                adjustedMoveSpeed += 100;

            if (keyboard.IsKeyDown(Keys.S))
                position -= forward * adjustedMoveSpeed * delta;

            if (keyboard.IsKeyDown(Keys.W))
                position += forward * adjustedMoveSpeed * delta;

            if (keyboard.IsKeyDown(Keys.D))
                position -= left * adjustedMoveSpeed * delta;

            if (keyboard.IsKeyDown(Keys.A))
                position += left * adjustedMoveSpeed * delta;

            if (keyboard.IsKeyDown(Keys.Space))
                position += Vector3.Down * adjustedMoveSpeed * delta;

            if (keyboard.IsKeyDown(Keys.Z))
                position += Vector3.Up * adjustedMoveSpeed * delta;

            view = Matrix.Identity;
            view *= Matrix.CreateTranslation(position);
            view *= Matrix.CreateRotationZ(angle.Z);
            view *= Matrix.CreateRotationY(angle.Y);
            view *= Matrix.CreateRotationX(angle.X);

            frustum.Matrix = View * Projection;
        }
    }
}
