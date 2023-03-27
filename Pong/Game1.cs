using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Pong
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Paddle paddle1;
        private Paddle paddle2;
        private Ball ball;
        private Score player1Score;
        private Score player2Score;
        private SpriteFont font;
        private const int WinCondition = 2;
        private bool IsGameOver;
        private readonly Random random;
        private Color BackgroundColor;
        private readonly Point LeftPaddleStartPosition = new(0 + 10, 0);
        private const int PadlleWidth = 10;
        private const int PadlleHeight = 100;
        private bool IsCKeyPressed;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            random = new Random();
            IsCKeyPressed = false;
        }

        private void SetBackGroundColor()
        {
            int r = random.Next(0, 255);
            int g = random.Next(0, 255);
            int b = random.Next(0, 255);
            BackgroundColor = new Color(r, g, b);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            font = Content.Load<SpriteFont>("font");

            //set screen to fullscreen and update graphic device
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            //turn off mouse pointer
            IsMouseVisible = false;

            player1Score = new Score();
            player2Score = new Score();
            IsGameOver = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D paddleTexture = Content.Load<Texture2D>("paddle");
            paddle1 = new Paddle(paddleTexture, LeftPaddleStartPosition, new Point(PadlleWidth, PadlleHeight));
            paddle2 = new Paddle(paddleTexture, RightPaddleStartPosition(PadlleWidth), new Point(PadlleWidth, PadlleHeight));
            ball = new Ball(paddleTexture, GraphicsDevice.Viewport);

            ball.Reset();
        }

        private Point RightPaddleStartPosition(int paddleWidth)
        {
            return new Point(GraphicsDevice.Viewport.Width - paddleWidth - 10, 0);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (IsGameOver && Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                RestartGame();
            }
            //if c key is pressed and then released change background color

            if (Keyboard.GetState().IsKeyDown(Keys.C))
            {
                IsCKeyPressed= true;
            }
            else if (IsCKeyPressed && Keyboard.GetState().IsKeyUp(Keys.C))
            {
                IsCKeyPressed= false;
                SetBackGroundColor();
            }
            MovePaddleIfKeysDown();

            AI();

            //detect ball collision with top and bottom of screen and reverse direction
            VerticalScreenCollision();

            //detect ball collision with paddles and reverse direction
            PaddCollision();

            ball.Update();
            DetectScore();

            //create win condition if player1 or player2 score is 10
            if (player1Score.score == WinCondition || player2Score.score == WinCondition)
            {
                GameOver();
            }

            base.Update(gameTime);
        }

        private void MovePaddleIfKeysDown()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                paddle1.Up();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                paddle1.Down();
            }
        }

        private void PaddCollision()
        {
            if (ball.Rectangle.Intersects(paddle1.Rectangle))
            {
                ball.movement.X *= -1;
            }
            else if (ball.Rectangle.Intersects(paddle2.Rectangle))
            {
                ball.movement.X *= -1;
            }
        }

        private void VerticalScreenCollision()
        {
            if (ball.Position.Y <= 0 || ball.Position.Y >= GraphicsDevice.Viewport.Height - ball.SideLength)
            {
                ball.movement.Y *= -1;
            }
        }

        private void RestartGame()
        {
            IsGameOver = false;
            SetBackGroundColor();
            player1Score.score = 0;
            player2Score.score = 0;
            ball.Reset();
        }

        private void AI()
        {
            float timeToPaddle = (paddle2.Position.X - ball.Position.X) / ball.movement.X;
            float predictedY = ball.Position.Y + (ball.movement.Y * timeToPaddle);

            if (predictedY < paddle2.Position.Y + (paddle2.Height / 2))
            {
                // If the ball is predicted to be above the middle of the paddle, move up
                paddle2.Up();
            }
            else if (predictedY > paddle2.Position.Y + (paddle2.Height / 2))
            {
                // If the ball is predicted to be below the middle of the paddle, move down
                paddle2.Down();
            }

            //}
        }

        //game over screen
        public void GameOver()
        {
            IsGameOver = true;

            ball.Stop();
            //Show game over screen

        }

        private void DetectScore()
        {
            if (IsPlayer2Score())
            {
                player2Score.AddPoint();
                ball.Reset();
            }
            else if (IsPlayer1Score())
            {
                player1Score.AddPoint();
                ball.Reset();
            }

        }

        private bool IsPlayer1Score()
        {
            return ball.Position.X >= GraphicsDevice.Viewport.Width;
        }

        private bool IsPlayer2Score()
        {
            return ball.Position.X <= 0;
        }

        protected override void Draw(GameTime gameTime)
        {

            //set variable equal to random color

            GraphicsDevice.Clear(BackgroundColor);
            if (!IsGameOver)
            {
                // Begin drawing sprites
                _spriteBatch.Begin();

                // Draw the paddles
                _spriteBatch.Draw(paddle1.texture, new Vector2(paddle1.Position.X, paddle1.Position.Y), paddle1.Rectangle, Color.White);
                _spriteBatch.Draw(paddle2.texture, new Vector2(paddle2.Position.X, paddle2.Position.Y), paddle2.Rectangle, Color.White);

                // Draw the ball
                _spriteBatch.Draw(ball.Texture, new Vector2(ball.Position.X, ball.Position.Y), ball.Rectangle, Color.White);

                //draw score for player1

                _spriteBatch.DrawString(font, player1Score.score.ToString(), new Vector2(100, 100), Color.White);
                //draw score for player2
                _spriteBatch.DrawString(font, player2Score.score.ToString(), new Vector2(700, 100), Color.White);

                // End drawing sprites
                _spriteBatch.End();
            }
            else
            {
                //set RESULT to winner or loser depending on score
                string RESULT = player1Score.score >= WinCondition ? "You Win!" : player2Score.score >= WinCondition ? "You Lose!" : "Press SPACE to start...";
                _spriteBatch.Begin();
                _spriteBatch.DrawString(font, $"{RESULT}", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.White);

                _spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }

    //create a paddle object class

    public class Paddle
    {
        private Rectangle rectangle;
        // texture
        public Texture2D texture;

        //position
        public Point Position => rectangle.Location;


        private readonly int PaddleSpeed = 6;

        //size
        public int Width => rectangle.Width;
        public int Height => rectangle.Height;

        private const int BottomOfScreen = 720;
        public Rectangle Rectangle => rectangle;
        //create a paddle object
        public Paddle(Texture2D texture, Point startPosition, Point size)
        {
            //set paddle size
            rectangle = new Rectangle(startPosition, size);
            //set paddle texture
            this.texture = texture;

            //set paddle position
            rectangle.Location = startPosition;

            //get bottom of screen based on viewport
            
        }

        //method to move up
        public void Up()
        {
            rectangle.Y -= PaddleSpeed;
            // if paddle is at top of screen stop moving
            if (rectangle.Y <= 0)
            {
                rectangle.Y = 0;
            }
        }

        public void Down()
        {
            rectangle.Y += PaddleSpeed;
            //get paddle height
            int paddleHeight = texture.Height;

            // use paddleHeight and stop moving it at bottom of screen
            if (rectangle.Y >= BottomOfScreen - paddleHeight)
            {
                rectangle.Y = BottomOfScreen - paddleHeight;
            }
        }
    }

    //object class for ball
    public class Ball
    {
        private const float accelerationValue = 0.01f;
        private readonly Random random = new();
        private float acceleration = 0.1f;
        public const int sideLength = 10;
        private Point StartPosition;
        private Rectangle rectangle;
        //position

        public float Acceleration { get; }

        public Point Position => rectangle.Location;
        public Rectangle Rectangle => rectangle;
        //size
        public int SideLength { get; }

        //movement vector
        public Vector2 movement;

        public Texture2D Texture { get; set; }

        //create a ball object
        public Ball(Texture2D texture, Viewport viewport)
        {
            
            //set ball texture
            Texture = texture;

            //set ball position
            StartPosition = new Point(viewport.Width / 2, viewport.Height / 2);
            rectangle = new Rectangle(StartPosition, new Point(sideLength, sideLength));
            Reset();
        }

        //method to update ball position based on movement vector * acceleration
        public void Update()
        {
            acceleration += accelerationValue;
            rectangle.X = (int)(rectangle.X + movement.X * acceleration);
            rectangle.Y = (int)(rectangle.Y + movement.Y * acceleration);
        }

        //method to reset ball position and movement with random directional vector
        public void Reset()
        {
            rectangle = new Rectangle(StartPosition, new Point(sideLength, sideLength));
            int randX;
            do
            {
                randX = random.Next(-3, 4); // generates random number between -3 and 3 (inclusive of -3 and 3)
            } while (randX is > (-1) and < 1); // keep generating random numbers until the number is outside the range of -1 and 1
            int randY;
            do
            {
                randY = random.Next(-3, 4); // generates random number between -3 and 3 (inclusive of -3 and 3)
            } while (randY is > (-1) and < 1);

            movement = new Vector2(randX, randY);
            acceleration = accelerationValue;
        }
        public void Stop()
        {
            rectangle.X = 0;
            rectangle.Y = 0;
            movement = new Vector2(0, 0);
            acceleration = 0;
        }
    }

    //score object
    public class Score
    {
        //score
        public int score;

        //create a score object
        public Score()
        {
            //set score to 0
            score = 0;
        }

        //method to add 1 to score
        public void AddPoint()
        {
            score += 1;
        }
    }

}