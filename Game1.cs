using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace WallBreaker
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D ballTexture;
        Texture2D lineTexture;
        Texture2D wallTexture;
        Texture2D screenTexture;
        Vector2 ballPosition;
        Vector2 linePosition;
        Vector2[] wallPositions;
        float ballYSpeed;
        float ballXSpeed;
        float ballSpeed;
        float lineSpeed;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            ballPosition =
                new Vector2(
                    _graphics.PreferredBackBufferWidth / 2,
                    _graphics.PreferredBackBufferHeight / 2);
            linePosition =
                new Vector2(
                    _graphics.PreferredBackBufferWidth / 2,
                    _graphics.PreferredBackBufferHeight - 50);

            wallPositions = new Vector2[4] {
                new(_graphics.PreferredBackBufferWidth/8, 100),
                new(3*_graphics.PreferredBackBufferWidth/8, 100),
                new(5*_graphics.PreferredBackBufferWidth/8, 100),
                new(7*_graphics.PreferredBackBufferWidth/8, 100) };

            ballYSpeed = 600f;
            ballXSpeed = 0f;
            ballSpeed = 600f;
            lineSpeed = 250f;
            screenTexture = null;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ballTexture = Content.Load<Texture2D>("ball");
            lineTexture = Content.Load<Texture2D>("line");
            wallTexture = Content.Load<Texture2D>("wall");
        }

        protected override void Update(GameTime gameTime)
        {
            if (wallPositions.Length == 0)
            {
                screenTexture = Content.Load<Texture2D>("win");
                ballXSpeed = 0f;
                ballYSpeed = 0f;
            }
            else if (ballPosition.Y + ballTexture.Height / 2 > linePosition.Y + lineTexture.Height / 2)
            {
                screenTexture = Content.Load<Texture2D>("lose");
                ballXSpeed = 0f;
                ballYSpeed = 0f;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                Initialize();

            // Update Logic
            var kstate = Keyboard.GetState();

            // Line
            if (kstate.IsKeyDown(Keys.A) || kstate.IsKeyDown(Keys.Left))
            {
                linePosition.X -= lineSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (kstate.IsKeyDown(Keys.D) || kstate.IsKeyDown(Keys.Right))
            {
                linePosition.X += lineSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            linePosition = RespectBorder(linePosition, lineTexture);


            // Ball
            CheckWallHit(ballPosition);
            ballPosition = RespectBorder(ballPosition, ballTexture);

            // collision check with line
            if (ballPosition.X <= linePosition.X + lineTexture.Width / 2 + ballTexture.Width / 2
                &&
                ballPosition.X >= linePosition.X - lineTexture.Width / 2 - ballTexture.Width / 2
                &&
                ballPosition.Y + ballTexture.Height / 2 >= linePosition.Y - lineTexture.Height / 2)
            {
                var ditanceFromCenter = (linePosition.X - ballPosition.X) / lineTexture.Width;

                ballXSpeed = ballSpeed * -ditanceFromCenter;
                ballYSpeed = ballSpeed - Math.Abs(ballXSpeed);

                // bounce
                ballYSpeed = - ballYSpeed;
            }

            // border collision
            if (ballPosition.Y == _graphics.PreferredBackBufferHeight - ballTexture.Height / 2
                ||
                ballPosition.Y == ballTexture.Height / 2)
            {
                ballYSpeed = - ballYSpeed;
            }
            if (ballPosition.X == _graphics.PreferredBackBufferWidth - ballTexture.Width / 2
                ||
                ballPosition.X == ballTexture.Width / 2)
            {
                ballXSpeed = - ballXSpeed;
            }
            
            // movement
            ballPosition.Y += ballYSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            ballPosition.X += ballXSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();

            if (screenTexture == null) {
                _spriteBatch.Draw(
                    ballTexture,
                    ballPosition,
                    null,
                    Color.White,
                    0f,
                    new Vector2(ballTexture.Width / 2, ballTexture.Height / 2),
                    Vector2.One,
                    SpriteEffects.None,
                    0f
                );

                _spriteBatch.Draw(
                    lineTexture,
                    linePosition,
                    null,
                    Color.White,
                    0f,
                    new Vector2(lineTexture.Width / 2, lineTexture.Height / 2),
                    Vector2.One,
                    SpriteEffects.None,
                    0f
                );

                foreach (var wall in wallPositions)
                {
                    _spriteBatch.Draw(
                        wallTexture,
                        wall,
                        null,
                        Color.White,
                        0f,
                        new Vector2(wallTexture.Width / 2, wallTexture.Height / 2),
                        Vector2.One,
                        SpriteEffects.None,
                        0f
                    );
                }

            }else {
                _spriteBatch.Draw(screenTexture, Vector2.Zero, Color.White);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected Vector2 RespectBorder(Vector2 vector, Texture2D texture)
        {
            if (vector.X > _graphics.PreferredBackBufferWidth - texture.Width / 2)
            {
                vector.X = _graphics.PreferredBackBufferWidth - texture.Width / 2;
            }
            else if (vector.X < texture.Width / 2)
            {
                vector.X = texture.Width / 2;
            }

            if (vector.Y > _graphics.PreferredBackBufferHeight - texture.Height / 2)
            {
                vector.Y = _graphics.PreferredBackBufferHeight - texture.Height / 2;
            }
            else if (vector.Y < texture.Height / 2)
            {
                vector.Y = texture.Height / 2;
            }

            return vector;
        }
        
        protected void CheckWallHit(Vector2 ballPosition)
        {
            Vector2 collided = new();
            foreach (var wall in wallPositions)
            {
                if (ballPosition.X <= wall.X + wallTexture.Width / 2 + ballTexture.Width / 2
                    &&
                    ballPosition.X >= wall.X - wallTexture.Width / 2 - ballTexture.Width / 2
                    &&
                    ballPosition.Y >= wall.Y - wallTexture.Height / 2 - ballTexture.Height / 2
                    &&
                    ballPosition.Y <= wall.Y + wallTexture.Height / 2 + ballTexture.Height / 2)
                {
                    collided = wall;
                    break;
                }
            }
            if (collided != new Vector2())
            {
                wallPositions = wallPositions.Except(new Vector2[] { collided }).ToArray();
                
                if (ballPosition.Y < collided.Y + wallTexture.Height / 2
                    &&
                    ballPosition.Y > collided.Y - wallTexture.Height / 2)
                {
                    ballXSpeed = -ballXSpeed;
                }
                else
                {
                    ballYSpeed = -ballYSpeed;
                }
            }
        }
    
    }
}