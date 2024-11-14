using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;

class Program
{
    static void Main()
    {
        Console.CursorVisible = false;
        char currentChar = '█';
        ConsoleColor currentColor = ConsoleColor.White;

        int startX = 20, startY = 10;
        int x = startX, y = startY;

        int borderX1 = 10;
        int borderY1 = 5;
        int borderX2 = 110;
        int borderY2 = 25;

        
        InitializeDatabase();

        BorderLine(borderX1, borderY1, borderX2, borderY2, '█', ConsoleColor.Red);

        List<(int x, int y, char c, ConsoleColor color)> currentDrawing = new List<(int, int, char, ConsoleColor)>();

        while (true)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = currentColor;
            Console.Write(currentChar);
            Console.ResetColor();

            currentDrawing.Add((x, y, currentChar, currentColor));

            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.Escape)
            {
                break;
            }

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (y > borderY1 + 1) y--;
                    break;
                case ConsoleKey.DownArrow:
                    if (y < borderY2 - 1) y++;
                    break;
                case ConsoleKey.LeftArrow:
                    if (x > borderX1 + 1) x--;
                    break;
                case ConsoleKey.RightArrow:
                    if (x < borderX2 - 1) x++;
                    break;

                case ConsoleKey.D1:
                    currentColor = ConsoleColor.White;
                    break;
                case ConsoleKey.D2:
                    currentColor = ConsoleColor.Black;
                    break;
                case ConsoleKey.D3:
                    currentColor = ConsoleColor.Blue;
                    break;
                case ConsoleKey.D4:
                    currentColor = ConsoleColor.Yellow;
                    break;
                case ConsoleKey.D5:
                    currentColor = ConsoleColor.Magenta;
                    break;
                case ConsoleKey.D6:
                    currentColor = ConsoleColor.Cyan;
                    break;

                case ConsoleKey.C:
                    Console.Clear();
                    SaveDrawing(currentDrawing);  
                    currentDrawing.Clear();
                    currentColor = ConsoleColor.White;
                    BorderLine(borderX1, borderY1, borderX2, borderY2, '█', ConsoleColor.Red);

                    x = startX;
                    y = startY;
                    break;

                case ConsoleKey.R:
                    List<int> drawingIds = GetSavedDrawingIds();
                    if (drawingIds.Count > 0)
                    {
                        int selectedDrawing = SelectDrawing(drawingIds);
                        Console.Clear();
                        List<(int x, int y, char c, ConsoleColor color)> loadedDrawing = LoadDrawing(selectedDrawing);
                        foreach (var pixel in loadedDrawing)
                        {
                            Console.SetCursorPosition(pixel.x, pixel.y);
                            Console.ForegroundColor = pixel.color;
                            Console.Write(pixel.c);
                        }
                        currentDrawing = new List<(int, int, char, ConsoleColor)>(loadedDrawing);
                        BorderLine(borderX1, borderY1, borderX2, borderY2, '█', ConsoleColor.Red);
                    }
                    break;

                default:
                    break;
            }
        }
    }

    static void InitializeDatabase()
    {
        using (var connection = new SQLiteConnection("Data Source=drawings.db;Version=3;"))
        {
            connection.Open();
            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Drawings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    X INTEGER,
                    Y INTEGER,
                    Char TEXT,
                    Color INTEGER
                );
            ";
            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    static void SaveDrawing(List<(int x, int y, char c, ConsoleColor color)> drawing)
    {
        using (var connection = new SQLiteConnection("Data Source=drawings.db;Version=3;"))
        {
            connection.Open();
            foreach (var pixel in drawing)
            {
                string insertQuery = "INSERT INTO Drawings (X, Y, Char, Color) VALUES (@X, @Y, @Char, @Color)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@X", pixel.x);
                    command.Parameters.AddWithValue("@Y", pixel.y);
                    command.Parameters.AddWithValue("@Char", pixel.c.ToString());
                    command.Parameters.AddWithValue("@Color", (int)pixel.color);
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    static List<int> GetSavedDrawingIds()
    {
        var drawingIds = new List<int>();
        using (var connection = new SQLiteConnection("Data Source=drawings.db;Version=3;"))
        {
            connection.Open();
            string query = "SELECT DISTINCT Id FROM Drawings";
            using (var command = new SQLiteCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        drawingIds.Add(reader.GetInt32(0));
                    }
                }
            }
        }
        return drawingIds;
    }

    static List<(int x, int y, char c, ConsoleColor color)> LoadDrawing(int drawingId)
    {
        var drawing = new List<(int, int, char, ConsoleColor)>();
        using (var connection = new SQLiteConnection("Data Source=drawings.db;Version=3;"))
        {
            connection.Open();
            string query = "SELECT X, Y, Char, Color FROM Drawings WHERE Id = @Id";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", drawingId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int x = reader.GetInt32(0);
                        int y = reader.GetInt32(1);
                        char c = reader.GetString(2)[0];
                        ConsoleColor color = (ConsoleColor)reader.GetInt32(3);
                        drawing.Add((x, y, c, color));
                    }
                }
            }
        }
        return drawing;
    }

    static int SelectDrawing(List<int> drawingIds)
    {
        int selectedIndex = 0;
        bool choosing = true;

        while (choosing)
        {
            Console.Clear();

            for (int i = 0; i < drawingIds.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Drawing {drawingIds[i]}");
                }
                else
                {
                    Console.ResetColor();
                    Console.WriteLine($"Drawing {drawingIds[i]}");
                }
            }

            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (selectedIndex > 0) selectedIndex--;
                    break;
                case ConsoleKey.DownArrow:
                    if (selectedIndex < drawingIds.Count - 1) selectedIndex++;
                    break;
                case ConsoleKey.Enter:
                    choosing = false;
                    break;
            }
        }

        return drawingIds[selectedIndex];
    }

    static void BorderLine(int x1, int y1, int x2, int y2, char borderChar, ConsoleColor color)
    {
        Console.ForegroundColor = color;

        for (int x = x1; x <= x2; x++)
        {
            Console.SetCursorPosition(x, y1);
            Console.Write(borderChar);
            Console.SetCursorPosition(x, y2);
            Console.Write(borderChar);
        }

        for (int y = y1; y <= y2; y++)
        {
            Console.SetCursorPosition(x1, y);
            Console.Write(borderChar);
            Console.SetCursorPosition(x2, y);
            Console.Write(borderChar);
        }
    }
}