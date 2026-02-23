

using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    ///<summary>
    /// Třída pro získávání konfigurace úrovní hry. Obsahuje metodu GetLevel, která vrací LevelConfig pro danou úroveň.
    /// LevelConfig obsahuje informace o pozici brány, překážkách a velikosti herního pole.
    /// Překážky jsou generovány pomocí pomocných metod CreateHorizontalLine a CreateVerticalLine, které vytvářejí řady překážek podle zadaných parametrů.
    /// Tato třída umožňuje snadné přidávání nových úrovní s různými konfiguracemi překážek a brány.
    /// Herní mřížka obsahuje 20x20 políček, přičemž pozice (0,0) je v levém horním rohu a pozice (19,19) je v pravém dolním rohu.
    /// Brána se nachází na pozici (9,1) pro všechny úrovně, ale překážky se mění podle úrovně.
    ///</summary>
    internal static class LevelData
    {
        public static LevelConfig GetLevel(int levelIndex) {
            LevelConfig config = new LevelConfig {
                LevelNumber = levelIndex,
                GatePosition = new GridPoint(GameSettings.GameBoardWidth/2, 1), // Brána je vždy uprostřed horní řady
                Obstacles = new List<GridPoint>(),
                Width = GameSettings.GameBoardWidth,
                Height = GameSettings.GameBoardHeight
            };

            // Přidání překážek podle úrovně
            switch (levelIndex) {
                default:
                     break;
                case 1:
                    // "Vodorovná čára"
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 5, 10));
                    break;
                case 2:
                    // "Svislá čára"
                    config.Obstacles.AddRange(CreateVerticalLine(10, 5, 10));
                    break;
                case 3:
                    // "Téčko"
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 5, 10));
                    config.Obstacles.AddRange(CreateVerticalLine(10, 5, 10));
                    break;
                case 4:
                    // "Íčko"
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 5, 10));
                    config.Obstacles.AddRange(CreateVerticalLine(10, 5, 10));
                    config.Obstacles.AddRange(CreateVerticalLine(9, 5, 10));
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 14, 10));
                    break;
                case 5:
                    // "Tunel"
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 5, 10));
                    config.Obstacles.AddRange(CreateVerticalLine(10, 5, 4));
                    config.Obstacles.AddRange(CreateVerticalLine(9, 5, 4));
                    config.Obstacles.AddRange(CreateVerticalLine(10, 11, 4));
                    config.Obstacles.AddRange(CreateVerticalLine(9, 11, 4));
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 14, 10));
                    break;
                case 6:
                    // "Dva čtverce"
                    config.Obstacles.AddRange(CreateRectangle(13, 13, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(4, 4, 3, 3));
                    break;
                case 7:
                    // "Čtyři čtverce"
                    config.Obstacles.AddRange(CreateRectangle(13, 13, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(4, 4, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(4, 13, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(13, 4, 3, 3));
                    break;
                case 8:
                    // "Pětka na kostce"
                    config.Obstacles.AddRange(CreateRectangle(13, 13, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(4, 4, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(4, 13, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(13, 4, 3, 3));
                    config.Obstacles.AddRange(CreateRectangle(9, 9, 2, 2));
                    break;
                case 9:
                    // "Poličky"
                    config.Obstacles.AddRange(CreateHorizontalLine(0, 5, 15)); 
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 10, 15));
                    config.Obstacles.AddRange(CreateHorizontalLine(0, 15, 15));
                    break;
                case 10:
                    // "Sloupy"
                    config.Obstacles.AddRange(CreateVerticalLine(5, 4, 12));
                    config.Obstacles.AddRange(CreateVerticalLine(10, 4, 12)); 
                    config.Obstacles.AddRange(CreateVerticalLine(15, 4, 12));
                    break;
                case 11:
                    // "U-rampa"
                    config.Obstacles.AddRange(CreateVerticalLine(5, 5, 10));
                    config.Obstacles.AddRange(CreateVerticalLine(14, 5, 10));
                    config.Obstacles.AddRange(CreateHorizontalLine(5, 14, 10));
                    config.Obstacles.AddRange(CreateRectangle(9, 8, 2, 3));
                    break;
                case 12:
                    // "Spirála"
                    config.Obstacles.AddRange(CreateVerticalLine(4, 4, 13));
                    config.Obstacles.AddRange(CreateHorizontalLine(4, 16, 13));
                    config.Obstacles.AddRange(CreateVerticalLine(16, 4, 13));

                    config.Obstacles.AddRange(CreateHorizontalLine(8, 4, 9));

                    config.Obstacles.AddRange(CreateVerticalLine(8, 5, 8));
                    config.Obstacles.AddRange(CreateHorizontalLine(8, 12, 5));
                    config.Obstacles.AddRange(CreateVerticalLine(12, 8, 5));
                    break;
            }
            return config;
        }

        // Pomocné metody pro vytváření překážek
        private static List<GridPoint> CreateHorizontalLine(int startX, int startY, int length)
        {
            var walls = new List<GridPoint>();
            for (int i = 0; i < length; i++) {
                walls.Add(new GridPoint(startX + i, startY));
            }
            return walls;
        }
         private static List<GridPoint> CreateVerticalLine(int startX, int startY, int length)
        {
            var walls = new List<GridPoint>();
            for (int i = 0; i < length; i++) {
                walls.Add(new GridPoint(startX, startY + i));
            }
            return walls;
        }
        private static List<GridPoint> CreateRectangle(int startX, int startY, int width, int height)
        {
            var walls = new List<GridPoint>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    walls.Add(new GridPoint(startX + x, startY + y));
                }
            }
            return walls;
        }
    }
}
