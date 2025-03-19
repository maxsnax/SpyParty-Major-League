using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SML.Models {
    public class Results {
        public int Spy_SpyShot { get; set; } = 0;
        public int Spy_CivilianShot { get; set; } = 0;
        public int Spy_MissionsWin { get; set; } = 0;
        public int Spy_TimeOut { get; set; } = 0;
        public int Sniper_SpyShot { get; set; } = 0;
        public int Sniper_CivilianShot { get; set; } = 0;
        public int Sniper_MissionsWin { get; set; } = 0;
        public int Sniper_TimeOut { get; set; } = 0;
        // Spy stats
        public int Spy_Wins => Spy_CivilianShot + Spy_MissionsWin;
        public int Spy_Losses => Spy_SpyShot + Spy_TimeOut;
        // Sniper stats
        public int Sniper_Wins => Sniper_SpyShot + Sniper_TimeOut;
        public int Sniper_Losses => Sniper_CivilianShot + Sniper_MissionsWin;
        // Game stats
        public int Points_Won => Spy_Wins + Sniper_Wins;
        public int Points_Lost => Spy_Losses + Sniper_Losses;
        public int Total_Games => Points_Won + Points_Lost;
        public Results() { }

        public override string ToString() {
            return $"Spy: [Spy_SpyShot: {Spy_SpyShot}, Spy_CivilianShot: {Spy_CivilianShot}, Spy_MissionsWin: {Spy_MissionsWin}, Spy_TimeOut: {Spy_TimeOut}]\n" +
                   $"Sniper: [Sniper_SpyShot: {Sniper_SpyShot}, Sniper_CivilianShot: {Sniper_CivilianShot}, Sniper_MissionsWin: {Sniper_MissionsWin}, Sniper_TimeOut: {Sniper_TimeOut}]\n" +
                   $"Points: [Won: {Points_Won}, Lost: {Points_Lost}]\n";
        }

    }
}
