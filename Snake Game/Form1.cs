/* 
© 2024 Sachin Chhetri. All rights reserved. 
**** Description
The Snake game is a classic arcade-style game developed in .NET Framework, 
where the player controls a growing line, simulating a snake, which moves 
around the playing area. As the snake eats food, it grows in length; the 
game ends if the snake runs into the screen border, an obstacle, or itself.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using static Snake_Game.Form1.Settings;

namespace Snake_Game
{
    public partial class Form1 : Form
    {
        private List<Circle> Snake = new List<Circle>(); // Snake representation
        private Circle food = new Circle(); // Food
        private Label highScoreLabel;
        private Button startButton;
        private Button endButton;

        public Form1()
        {
            InitializeComponent();
            InitializeGameSettings();
            InitializeGameComponents();
            this.Focus();
        }

        private void InitializeGameSettings()
        {
            Settings.Width = 16;
            Settings.Height = 16;
            Settings.Speed = 16; // You can adjust speed as needed
            Settings.Score = 0;
            Settings.GameOver = false;
            Settings.direction = Settings.Directions.Down;
        }

        private void InitializeGameComponents()
        {
            this.KeyPreview = true; // Enable key event capturing
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

            gameTimer.Interval = 1000 / Settings.Speed;
            gameTimer.Tick += UpdateScreen;
            gameTimer.Start();

            pbCanvas.Paint += PbCanvas_Paint;

            scoreLabel = new Label
            {
                Location = new Point(13, 366),
                Text = "Score: 0"
            };
            this.Controls.Add(scoreLabel);

            highScoreLabel = new Label
            {
                Location = new Point(13, 390), // Adjust as needed
                Text = $"High Score: {Properties.Settings.Default.HighScore}",
                AutoSize = true
            };
            this.Controls.Add(highScoreLabel);

            startButton = new Button
            {
                Text = "Start Game",
                Location = new Point(13, 420), // Adjust the position as needed
            };
            startButton.Click += StartButton_Click; // Event handler for click
            this.Controls.Add(startButton);

            // End Button
            endButton = new Button
            {
                Text = "End Game",
                Location = new Point(150, 420), // Adjust the position as needed
                Enabled = false // Disable it at game start; it should only be enabled during an active game
            };
            endButton.Click += EndButton_Click; // Event handler for click
            this.Controls.Add(endButton);

            InitializeGameOverLabel();
            StartGame();
        }

        private void InitializeGameOverLabel()
        {
            lblGameOver = new Label
            {
                Visible = false,
                Text = "",
                AutoSize = true,
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
                Location = new Point(340, 362),
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(lblGameOver);
        }

        private void StartGame()
        {
            Settings.GameOver = false;
            Settings.Score = 0;
            Snake.Clear();
            Circle head = new Circle { X = 10, Y = 5 };
            Snake.Add(head);
            GenerateFood();
            Settings.direction = Settings.Directions.Down; // Set initial direction
            scoreLabel.Text = "Score: 0";
            highScoreLabel.Text = $"High Score: {Properties.Settings.Default.HighScore}";
            lblGameOver.Visible = false; // Hide game over label
            gameTimer.Enabled = true; // Start or resume the game timer
            startButton.Enabled = false; // Disable the start button while the game is active
            endButton.Enabled = true; 
        }

        private void UpdateScreen(object sender, EventArgs e)
        {
            if (Settings.GameOver)
            {
                lblGameOver.Text = $"Game over\nYour final score is: {Settings.Score}\nHigh score: {Properties.Settings.Default.HighScore}\nPress Enter to restart";
                lblGameOver.Visible = true;
                gameTimer.Enabled = false; // Stop the game loop
            }
            else
            {
                MovePlayer();
                pbCanvas.Invalidate(); // Refreshes the canvas to update the visuals.
            }
        }

        private void MovePlayer()
        {
            // Move the snake
            for (int i = Snake.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    // Change direction
                    switch (Settings.direction)
                    {
                        case Settings.Directions.Up: Snake[i].Y--; break;
                        case Settings.Directions.Down: Snake[i].Y++; break;
                        case Settings.Directions.Left: Snake[i].X--; break;
                        case Settings.Directions.Right: Snake[i].X++; break;
                    }

                    // Wrap around the screen
                    int maxXPos = pbCanvas.Size.Width / Settings.Width;
                    int maxYPos = pbCanvas.Size.Height / Settings.Height;
                    if (Snake[i].X < 0) Snake[i].X = maxXPos - 1;
                    if (Snake[i].X >= maxXPos) Snake[i].X = 0;
                    if (Snake[i].Y < 0) Snake[i].Y = maxYPos - 1;
                    if (Snake[i].Y >= maxYPos) Snake[i].Y = 0;

                    // Check collision with the body
                    for (int j = 1; j < Snake.Count; j++)
                    {
                        if (Snake[i].X == Snake[j].X && Snake[i].Y == Snake[j].Y)
                        {
                            Settings.GameOver = true;
                            // Show game over message
                            lblGameOver.Text = "Game over\nYour final score is: " + Settings.Score + "\nPress Enter to restart";
                            lblGameOver.Visible = true;
                            gameTimer.Enabled = false;
                            break;
                        }
                    }

                    // Check for collision with food
                    if (Snake[0].X == food.X && Snake[0].Y == food.Y)
                    {
                        EatFood();
                    }
                }
                else
                {
                    // Move the body
                    Snake[i].X = Snake[i - 1].X;
                    Snake[i].Y = Snake[i - 1].Y;
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartGame();
            startButton.Enabled = false; // Disable the start button as the game is now running
            endButton.Enabled = true; // Enable the end button
        }

        private void EndButton_Click(object sender, EventArgs e)
        {
            Settings.GameOver = true;
            GameOver(); // Handle game over logic
        }

        private void GameOver()
        {
            lblGameOver.Text = $"Game over\nYour final score is: {Settings.Score}\nHigh score: {Properties.Settings.Default.HighScore}\nPress Start to play again";
            lblGameOver.Visible = true;
            gameTimer.Enabled = false; // Stop the game
            startButton.Enabled = true; // Re-enable the start button
            endButton.Enabled = false; // Disable the end button as the game has ended
        }

        private void EatFood()
        {
            // Add a segment to the snake's body
            Circle segment = new Circle
            {
                X = Snake[Snake.Count - 1].X,
                Y = Snake[Snake.Count - 1].Y
            };
            Snake.Add(segment);

            // Update score
            Settings.Score += 100;
            scoreLabel.Text = "Score: " + Settings.Score;

            if (Settings.Score > Properties.Settings.Default.HighScore)
            {
                Properties.Settings.Default.HighScore = Settings.Score;
                Properties.Settings.Default.Save(); // Save the new high score to settings
            }

            GenerateFood();
        }

        private void GenerateFood()
        {
            int maxXPos = pbCanvas.Size.Width / Settings.Width;
            int maxYPos = pbCanvas.Size.Height / Settings.Height;
            Random rnd = new Random();
            food = new Circle { X = rnd.Next(0, maxXPos), Y = rnd.Next(0, maxYPos) };

            // Ensure food does not spawn on the snake
            while (Snake.Exists(s => s.X == food.X && s.Y == food.Y))
            {
                food.X = rnd.Next(0, maxXPos);
                food.Y = rnd.Next(0, maxYPos);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (Settings.direction != Settings.Directions.Down)
                        Settings.direction = Settings.Directions.Up;
                    break;
                case Keys.S:
                    if (Settings.direction != Settings.Directions.Up)
                        Settings.direction = Settings.Directions.Down;
                    break;
                case Keys.A:
                    if (Settings.direction != Settings.Directions.Right)
                        Settings.direction = Settings.Directions.Left;
                    break;
                case Keys.D:
                    if (Settings.direction != Settings.Directions.Left)
                        Settings.direction = Settings.Directions.Right;
                    break;
                case Keys.Up:
                    if (Settings.direction != Settings.Directions.Down)
                        Settings.direction = Settings.Directions.Up;
                    break;
                case Keys.Down:
                    if (Settings.direction != Settings.Directions.Up)
                        Settings.direction = Settings.Directions.Down;
                    break;
                case Keys.Left:
                    if (Settings.direction != Settings.Directions.Right)
                        Settings.direction = Settings.Directions.Left;
                    break;
                case Keys.Right:
                    if (Settings.direction != Settings.Directions.Left)
                        Settings.direction = Settings.Directions.Right;
                    break;
                case Keys.Enter:
                    if (Settings.GameOver)
                    {
                        StartGame();
                        lblGameOver.Visible = false;
                        gameTimer.Enabled = true; // Restart game updates
                    }
                    break;
            }
        }

        private void PbCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;
            Pen borderPen = new Pen(Color.Black, 2); // Change color and thickness as needed
            canvas.DrawRectangle(borderPen, 0, 0, pbCanvas.Width - 1, pbCanvas.Height - 1);
            if (!Settings.GameOver)
            {
                // Draw the snake
                for (int i = 0; i < Snake.Count; i++)
                {
                    Brush snakeColor = (i == 0) ? Brushes.Black : Brushes.Green; // Head is black, body is green
                    canvas.FillEllipse(snakeColor,
                        new Rectangle(Snake[i].X * Settings.Width,
                                      Snake[i].Y * Settings.Height,
                                      Settings.Width, Settings.Height));

                    // Draw the food
                    canvas.FillEllipse(Brushes.Red,
                        new Rectangle(food.X * Settings.Width,
                                      food.Y * Settings.Height,
                                      Settings.Width, Settings.Height));
                }
            }
        }

        public class Circle
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Circle()
            {
                X = 0;
                Y = 0;
            }
        }

        public class Settings
        {
            public static int Width { get; set; }
            public static int Height { get; set; }
            public static int Speed { get; set; }
            public static int Score { get; set; }
            public static bool GameOver { get; set; }
            public static Directions direction { get; set; }

            static Settings()
            {
                Width = 16;
                Height = 16;
                Speed = 16;
                Score = 0;
                GameOver = false;
                direction = Directions.Down;
            }

            public enum Directions
            {
                Up,
                Down,
                Left,
                Right
            }
        }
    }
}
