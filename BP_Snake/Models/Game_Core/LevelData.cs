using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Models.Game_Core
{
    internal static class LevelData
    {
        public static LevelConfig GetLevel(int levelIndex) {
            LevelConfig config = new LevelConfig {
                LevelNumber = levelIndex,
                GatePosition = new Point(9, 1),
                Obstacles = new List<Point>(),
                Width = 20, // Velikost šířky herního pole
                Height = 20
            };

            // Přidání překážek podle úrovně
            switch (levelIndex) {
                default:
                     break;
                case 1:
                    config.Obstacles.AddRange(CreateHorizontalLine(4, 4, 10));
                    break;
                case 2:
                    config.Obstacles.AddRange(CreateVerticalLine(9, 4, 10));
                    break;
                case 3:
                    config.Obstacles.AddRange(CreateHorizontalLine(4, 4, 10));
                    config.Obstacles.AddRange(CreateVerticalLine(9, 4, 10));
                    break;
                case 4:
                    config.Obstacles.AddRange(CreateHorizontalLine(4, 4, 10));
                    config.Obstacles.AddRange(CreateHorizontalLine(4, 14, 10));
                    break;
            }
            return config;
        }

        // Pomocné metody pro vytváření překážek
        private static List<Point> CreateHorizontalLine(int startX, int startY, int length)
        {
            var walls = new List<Point>();
            for (int i = 0; i < length; i++) {
                walls.Add(new Point(startX + i, startY));
            }
            return walls;
        }
         private static List<Point> CreateVerticalLine(int startX, int startY, int length)
        {
            var walls = new List<Point>();
            for (int i = 0; i < length; i++) {
                walls.Add(new Point(startX, startY + i));
            }
            return walls;
        }
    }
}
