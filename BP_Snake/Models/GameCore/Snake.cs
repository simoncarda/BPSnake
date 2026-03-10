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
            // Inicializace počátečních článků těla.
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
        /// <returns>A Bod <see cref="GridPoint"/> reprezentující pozici o jednu jednotku před aktuální hlavou ve směru určeném vlastností <see cref="CurrentDirection"/>.</returns>
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
        /// Algoritmus posunu: Místo složitého přepočítávání všech článků se pouze 
        /// přidá nová hlava na začátek seznamu a smaže se poslední článek (ocas).
        /// Posune hada o jeden krok vpřed v jeho aktuálním směru, s volitelnou možností zvětšení jeho délky.
        /// </summary>
        /// <remarks>
        /// Pokud se had pohybuje s parametrem grow nastaveným na false, poslední článek je odstraněn, aby délka zůstala nezměněna.
        /// Parametr grow použijte v případě, že se má had prodloužit, například po snědení jídla.
        /// </remarks>
        /// <param name="grow"> true pro zvětšení délky hada přidáním nového článku u hlavy; v opačném případě false pro zachování aktuální délky.</param>
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
