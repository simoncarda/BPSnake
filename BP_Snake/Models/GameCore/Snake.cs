using BPSnake.Configuration;

namespace BPSnake.Models.GameCore
{
    /// <summary>
    /// Reprezentuje hada v klasické hře „had“, spravuje segmenty jeho těla a směr pohybu.
    /// </summary>
    /// <remarks>
    /// Třída Snake poskytuje metody pro pohyb hada, zvětšování jeho délky a detekci kolizí se sebou samým.
    /// Tělo hada je reprezentováno jako seznam bodů, přičemž první bod odpovídá hlavě. Směr pohybu lze ovládat pomocí vlastnosti CurrentDirection.
    /// Tato třída je určena k použití jako jádro logiky hry a nezajišťuje přímo vykreslování ani uživatelský vstup.
    /// </remarks>
    internal class Snake
    {
        public List<GridPoint> Body { get; set; } = new();
        public Direction CurrentDirection { get; set; }
        public Snake()
        {
            for (int i = 0; i < GameSettings.InitialSnakeLength; i++) {
                Body.Add(new GridPoint(3 - i, 1)); // Vytvoření počátečního těla hada, vždy na pozici (3,1) až (0,1)
            }
            CurrentDirection = Direction.Right; // Výchozí směr pohybu doprava
        }

        /// <summary>
        /// Vypočítá příští pozici hlavy hada na základě jeho aktuálního směru pohybu.
        /// </summary>
        /// <remarks>
        /// Tato metoda slouží k určení, kam se hlava hada posune při příští aktualizaci, aniž byste měnili stav hada.
        /// Vrácená pozice závisí na aktuálním směru a nebere v úvahu kolize ani hranice herní plochy.
        /// </remarks>
        /// <returns>A Bod <see cref="GridPoint"/> reprezentující pozici o jednu jednotku před aktuální hlavou ve směru určeném vlastností <see cref="CurrentDirection"/>.</returns>
        public GridPoint GetNextHeadPosition()
        {
            GridPoint head = Body[0]; // Aktuální pozice hlavy hada
            GridPoint nextPosition = head; // Proměnná pro uložení další pozice hlavy, výchozí hodnota je aktuální pozice hlavy

            switch (CurrentDirection)
            {
                case Direction.Up:
                    nextPosition = new GridPoint(head.X, head.Y - 1);
                    break;
                case Direction.Down:
                    nextPosition = new GridPoint(head.X, head.Y + 1);
                    break;
                case Direction.Left:
                    nextPosition = new GridPoint(head.X - 1, head.Y);
                    break;
                case Direction.Right:
                    nextPosition = new GridPoint(head.X + 1, head.Y);
                    break;
            }

            return nextPosition;
        }

        /// <summary>
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
