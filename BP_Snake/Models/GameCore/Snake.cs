using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje hada v klasické hře „had“, spravuje segmenty jeho těla a směr pohybu.
    /// </summary>
    internal class Snake
    {
        // List uchovává souřadnice jednotlivých článků. Index 0 představuje vždy hlavu.
        // Kolekce List je vhodná pro snadné vkládání (při růstu) a odebírání (při pohybu).
        public List<GridPoint> Body { get; set; } = new();
        public Direction CurrentDirection { get; set; }
        public Snake()
        {
            // Odečítáním 'i' se tělo generuje horizontálně za hlavou (směrem doleva).
            for (int i = 0; i < GameSettings.InitialSnakeLength; i++) {
                Body.Add(new GridPoint(GameSettings.InitialSnakeStartX - i, GameSettings.InitialSnakeStartY));
            }
            CurrentDirection = Direction.Right;
        }

        /// <summary>
        /// Vypočítá příští pozici hlavy hada na základě jeho aktuálního směru pohybu.
        /// Využívá moderní C# switch expression pro čistší syntaxi.
        /// </summary>
        /// <returns>Bod <see cref="GridPoint"/> reprezentující pozici o jednu jednotku před aktuální hlavou ve směru určeném vlastností <see cref="CurrentDirection"/>.</returns>
        public GridPoint GetNextHeadPosition()
        {
            GridPoint head = Body[0];

            return CurrentDirection switch
            {
                Direction.Up => new GridPoint(head.X, head.Y - 1),
                Direction.Down => new GridPoint(head.X, head.Y + 1),
                Direction.Left => new GridPoint(head.X - 1, head.Y),
                Direction.Right => new GridPoint(head.X + 1, head.Y),
                _ => head
            };
        }

        /// <summary>
        /// Provede fyzický posun hada po herní ploše.
        /// Z důvodu optimalizace výkonu se nepřepočítávají souřadnice všech článků, 
        /// ale pouze se přidá nová hlava a případně se odstraní ocas.
        /// </summary>
        /// <param name="grow">Pokud je true (např. po konzumaci potravy), ocas se neodstraní a had se prodlouží.</param>
        public void Move(bool grow = false)
        {
            GridPoint nextHeadPosition = GetNextHeadPosition();
            Body.Insert(0, nextHeadPosition); // Přidání nové hlavy na začátek seznamu těla
            if (!grow)
            {
                Body.RemoveAt(Body.Count - 1);
            }
        }
    }
}
