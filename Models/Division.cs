using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SML.Models {
    public class Division {
        public int SeasonID { get; set; }
        public int DivisionID { get; set; }
        public string DivisionName { get; set; }
        public int LoadOrder { get; set; }
        public List<Player> Players { get; set; }

        public Division() { }

        public Division(int Season, int ID, string Name, int Order) {
            SeasonID = Season;
            DivisionID = ID;
            DivisionName = Name;
            LoadOrder = Order;
            Players = new List<Player>();
        }
    }
}
