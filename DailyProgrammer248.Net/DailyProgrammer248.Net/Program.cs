using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DailyProgrammer248.Net
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ppmGen = new PpmDrawingReader(new StreamReader(new FileStream(@"../../../Challenge Input.txt", FileMode.Open))))
            {
                var result = ppmGen.Process();
                Console.WriteLine(result.GenerateFile());
            }

            Console.ReadKey();
        }
    }

    class PpmFile
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int MaxColor;

        public readonly List<Color> Points;

        public PpmFile(int width, int height, int maxColor)
        {
            Width = width;
            Height = height;
            MaxColor = maxColor;
            var black = Color.Black;
            Points = Enumerable.Range(0, width * height).Select(num => black).ToList();
        }

        public Color this[int row, int col]
        {
            get { return Points[CalculateIdx(row, col)]; }
            set { Points[CalculateIdx(row, col)] = value; }
        }

        public string GenerateFile()
        {
            var result = new StringBuilder();
            result.AppendLine("P3");
            result.AppendLine($"{Width} {Height}");
            foreach (var point in Points)
            {
                result.Append(ColorToPpmPixl(point));
            }
            return result.ToString();
        }

        private int CalculateIdx(int row, int col)
        {
            return row * Width + col;
        }

        private static string ColorToPpmPixl(Color c)
        {
            return $" {c.R} {c.G} {c.B} ";
        }
    }

    class PpmDrawingReader : IDisposable
    {
        private readonly StreamReader _drawingCommands;

        public PpmDrawingReader(StreamReader drawingCommands)
        {
            _drawingCommands = drawingCommands;
        }

        public PpmFile Process()
        {
            var ppm = InitPpmFromStream(_drawingCommands);
            while (!_drawingCommands.EndOfStream)
            {
                var line = _drawingCommands.ReadLine();
                var pointGen = new PointGenerator(line);
                foreach (var colorPoint in pointGen.Generate())
                {
                    ppm[colorPoint.P.Y, colorPoint.P.X] = colorPoint.C;
                }
            }
            return ppm;
        }

        private PpmFile InitPpmFromStream(StreamReader drawingCommands)
        {
            var firstLine = drawingCommands.ReadLine();
            var split = firstLine.Split(' ');
            return new PpmFile(int.Parse(split[0]), int.Parse(split[1]), 255);
        }

        public void Dispose()
        {
            _drawingCommands.Dispose();
        }
    }

    class PointGenerator
    {
        private readonly string _command;

        private readonly string[] _split;
        private readonly Color _color;

        public PointGenerator(string line)
        {
            _split = line.Split(' ');
            _command = _split[0];
            _color = ParseColor(_split);
        }

        public List<ColorPoint> Generate()
        {
            switch (_command)
            {
                case "point":
                    return GeneratePoint();
                case "line":
                    return GenerateLine();
                case "rect":
                    return GenerateRect();
                default:
                    throw new Exception();
            }
        }

        private List<ColorPoint> GenerateLine()
        {
            var from = new Point(int.Parse(_split[4]), int.Parse(_split[5]));
            var to = new Point(int.Parse(_split[6]), int.Parse(_split[7]));
            return RecursiveLine(from, to, _color);
        }

        private static List<ColorPoint> RecursiveLine(Point from, Point to, Color color)
        {
            if (from.X == to.X && from.Y == to.Y)
            {
                return new List<ColorPoint>
                {
                    new ColorPoint
                    {
                        P = from,
                        C = color,
                    }
                };
            }

            var midPoint = Midpoint(from, to);
            return RecursiveLine(from, midPoint, color).Union(RecursiveLine(midPoint, to, color)).ToList();
        }

        private List<ColorPoint> GenerateRect()
        {
            var topLeft = new Point(int.Parse(_split[4]), int.Parse(_split[5]));
            var height = int.Parse(_split[6]);
            var width = int.Parse(_split[7]);

            return
                Enumerable.Range(topLeft.X, width).Select(x => new ColorPoint
                {
                    P = new Point(x, topLeft.Y),
                    C = _color,
                })
                .Union(Enumerable.Range(topLeft.X, width).Select(x => new ColorPoint
                {
                    P = new Point(x, topLeft.Y + height),
                    C = _color,
                }))
                .Union(Enumerable.Range(topLeft.Y, height).Select(y => new ColorPoint
                {
                    P = new Point(topLeft.X, y),
                    C = _color,
                }))
                .Union(Enumerable.Range(topLeft.Y, height).Select(y => new ColorPoint
                {
                    P = new Point(topLeft.X + width, y),
                    C = _color,
                }))
                .ToList();
        }

        private static Point Midpoint(Point p1, Point p2)
        {
            return new Point((int)Math.Round((p1.X + p2.X) / 2.0), (int)Math.Round((p1.Y + p2.Y) / 2.0));
        }

        private List<ColorPoint> GeneratePoint()
        {
            return new List<ColorPoint>
            {
                new ColorPoint
                {
                    C = _color,
                    P = new Point(int.Parse(_split[4]), int.Parse(_split[5])),
                }
            };
        }

        private Color ParseColor(string[] parts)
        {
            return Color.FromArgb(int.Parse(_split[1]), int.Parse(_split[2]), int.Parse(_split[3]));
        }
    }

    class ColorPoint
    {
        public Point P { get; set; }
        public Color C { get; set; }
    }
}
