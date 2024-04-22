using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky.Enums
{
    /// <summary>
    /// E = Empty, O = Opponent, T = Threat
    /// </summary>
    public enum ThreatEnum
    {
        TOOOT,
        MOOOOT,
        TOOOOM,
        OOOOT,  // touching border of plane
        TOOOO,  // touching border of plane
        OOOTO,
        OTOOO,
        OOTOO,
        EOOTOE,
        EOTOOE
    }
}
