using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TetrisDemo
{
    public partial class Form1 : Form
    {
        Bitmap canvasBitmap;
        Graphics canvasGraphics;
        int canvasWidth = 15;
        int canvasHeight = 20;
        int[,] canvasDotArray;
        int dotSize = 20;

        int currentX;
        int currentY;

        private int _clientWidth;
        private int _clientHeight;

        Shape currentShape;
        Timer timer = new Timer();

        Bitmap workingBitmap;
        Graphics workingGraphics;

        public Form1()
        {
            InitializeComponent();

            // Set ClientSize based on grid size
            _clientWidth = dotSize * canvasWidth;
            _clientHeight = dotSize * canvasHeight;

            ClientSize = new Size(_clientWidth + 20, _clientHeight + 20);

            LoadCanvas();

            currentShape = GetRandomShapeWithCenterAligned();

            timer.Tick += Timer_Tick;
            timer.Interval = 10;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var MoveIsSuccessful = MoveShapeIfPossible(moveDown: 1);

            // If shape reaches bottom or touched any other shapes
            if (!MoveIsSuccessful)
            {
                // Copy Working Image into Canvas Image
                canvasBitmap = new Bitmap(workingBitmap);

                UpdateCanvasDotArrayWithCurrentShape();

                // Get Next Shape
                currentShape = GetRandomShapeWithCenterAligned();
            }
        }

        private void LoadCanvas()
        {
            // Set pictureBox size based on dotSize and Canvas sizes
            pBoxGameField.Width = canvasWidth * dotSize;
            pBoxGameField.Height = canvasHeight * dotSize;

            // Create Bitmap with pictureBox size
            canvasBitmap = new Bitmap(pBoxGameField.Width, pBoxGameField.Height);

            canvasGraphics = Graphics.FromImage(canvasBitmap);

            canvasGraphics.FillRectangle(Brushes.LightGray, 0, 0, canvasBitmap.Width, canvasBitmap.Height);

            // Load bitmap into pictureBox
            pBoxGameField.Image = canvasBitmap;

            // Initialize canvas dot array. By default, all elements = 0.
            canvasDotArray = new int[canvasWidth, canvasHeight];
        }

        public class Shape
        {
            public int Width;
            public int Height;
            public int[,] Dots;

            private int[,] backupDots;

            public void TurnShape()
            {
                // Back up the dots values to roll-back a turn if invalid position
                backupDots = Dots;

                Dots = new int[Width, Height];

                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        Dots[i, j] = backupDots[Height - 1 - j, i];
                    }
                }

                // Adjust properties based on rotated position (Width and Height flip)
                var temp = Width;
                Width = Height;
                Height = temp;
            }

            public void RollBack()
            {
                Dots = backupDots;

                // Adjust properties based on rotated position (Width and Height flip)
                var temp = Width;
                Width = Height;
                Height = temp;
            }
        }

        public static class ShapesHandler
        {
            private static Shape[] shapesArray;

            static ShapesHandler()
            {
                // Create shapes to add into the array
                shapesArray = new Shape[]
                {
                    new Shape
                    {
                        Width = 2,
                        Height = 2,
                        Dots = new int[,]
                        {
                            { 1, 1 },
                            { 1, 1 }
                        }
                    },
                    new Shape
                    {
                        Width = 1,
                        Height = 4,
                        Dots = new int[,]
                        {
                            { 1 },
                            { 1 },
                            { 1 },
                            { 1 }
                        }
                    },
                    new Shape
                    {
                        Width = 3,
                        Height = 2,
                        Dots = new int[,]
                        {
                            { 0, 1, 0 },
                            { 1, 1, 1 }
                        }
                    },
                    new Shape
                    {
                        Width = 3,
                        Height = 2,
                        Dots = new int[,]
                        {
                            { 0, 0, 1 },
                            { 1, 1, 1 }
                        }
                    },
                    new Shape
                    {
                        Width = 3,
                        Height = 2,
                        Dots = new int[,]
                        {
                            { 1, 0, 0 },
                            { 1, 1, 1 }
                        }
                    },
                    new Shape
                    {
                        Width = 3,
                        Height = 2,
                        Dots = new int[,]
                        {
                            { 1, 1, 0 },
                            { 0, 1, 1 }
                        }
                    },
                    new Shape
                    {
                        Width = 3,
                        Height = 2,
                        Dots = new int[,]
                        {
                            { 0, 1, 1 },
                            { 1, 1, 0 }
                        }
                    }
                // NEW SHAPES HERE
                };
            }

            public static Shape GetRandomShape()
            {
                var shape = shapesArray[new Random().Next(shapesArray.Length)];

                return shape;
            }
        }

        private Shape GetRandomShapeWithCenterAligned()
        {
            var shape = ShapesHandler.GetRandomShape();

            // Calculate the x and y values as if the shape lies in the center
            currentX = 7;
            currentY = -shape.Height;

            return shape;
        }

        private bool MoveShapeIfPossible(int moveDown = 0, int moveSide = 0)
        {
            var newX = currentX + moveSide;
            var newY = currentY + moveDown;

            // Check if it reaches the bottom or side of playfield
            if (newX < 0 || newX + currentShape.Width > canvasWidth || newY + currentShape.Height > canvasHeight)
            {
                return false;
            }
            
            // Check for collision against other blocks
            for (int i = 0; i < currentShape.Width; i++)
            {
                for (int j = 0; j < currentShape.Height; j++)
                {
                    if (newY + j > 0 && canvasDotArray[newX + i, newY + j] == 1 && currentShape.Dots[j, i] == 1)
                    {
                        return false;
                    }
                }
            }

            currentX = newX;
            currentY = newY;

            DrawShape();

            return true;
        }

        private void DrawShape()
        {
            workingBitmap = new Bitmap(canvasBitmap);
            workingGraphics = Graphics.FromImage(workingBitmap);

            for (int i = 0; i < currentShape.Width; i++)
            {
                for (int j = 0; j < currentShape.Height; j++)
                {
                    if (currentShape.Dots[j, i] == 1)
                    {
                        workingGraphics.FillRectangle(Brushes.Black, (currentX + i) * dotSize, (currentY + j) * dotSize, dotSize, dotSize);
                    }
                }
            }

            pBoxGameField.Image = workingBitmap;
        }

        private void UpdateCanvasDotArrayWithCurrentShape()
        {
            for (int i = 0; i < currentShape.Width; i++)
            {
                for (int j = 0; j < currentShape.Height; j++)
                {
                    if (currentShape.Dots[j, i] == 1)
                    {
                        CheckIfGameOver();

                        canvasDotArray[currentX + i, currentY + j] = 1;
                    }
                }
            }
        }

        private void CheckIfGameOver()
        {
            if (currentY < 0)
            {
                timer.Stop();
                MessageBox.Show("Game Over");
                Application.Exit();
            }
        }
    }
}
