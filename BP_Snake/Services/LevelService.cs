using System;
using System.Collections.Generic;
using System.Text;

namespace BP_Snake.Services
{
    /// <summary>
    /// Jednoduchá služba spravující logiku přechodu mezi úrovněmi.
    /// Zde je centralizované pravidlo, kdy se přejde na další level a jaké jsou hranice.
    /// Aktuálně hra obsahuje 12 úrovní, po dosažení 12. úrovně se přechází zpět na 1. úroveň, což umožňuje nekonečné hraní s opakováním úrovní.
    /// </summary>
    internal sealed class LevelService
    {
        public int GetNextLevel(int currentLevel)
        {
            return currentLevel >= 12 ? 1 : currentLevel + 1;
        }
    }
}
