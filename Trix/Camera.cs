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

        private Vector3 position = new Vector3(0, -100, 0);
        private Vector3 angle = new Vector3();
        private float moveSpeed = 25f;
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

        public Camera(GraphicsDevice device)
        {
            float ratio = (float)device.Viewport.Width / (float)device.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, ratio, 0.1f, 10000);
        }

        public void Update(Game game, GameTime gameTime, KeyboardState keyboard, MouseState mouse)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            int centerX = game.Window.ClientBounds.Width / 2;
            int centerY = game.Window.ClientBounds.Height / 2;
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
