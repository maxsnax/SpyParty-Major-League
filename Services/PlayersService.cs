using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using SML.Exceptions;
using System.IO.Compression;
using SML.Models;
using static SML.Models.Replays;
using SML.DAL.Repositories;
using SML.DAL;
using System.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Web.UI.WebControls;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SML {
    public class PlayersService {
        private readonly UnitOfWork _uow;

        public PlayersService() {
            _uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());
        }

        public List<Tuple<int, string>> LoadSeasons() {
            return _uow.SeasonsRepo.LoadSeasons();
        }

        public DataTable PopulateAllPlayerData(GridView table) {
            DataTable rawData = _uow.PlayersRepo.GetPlayerData();
            BindPlayerData(table, rawData);
            return rawData;
        }

        private void BindPlayerData(GridView table, DataTable playerData) {
            if (playerData != null && playerData.Rows.Count > 0) {
                table.DataSource = playerData;
            }
            else {
                table.DataSource = null;
            }
            table.DataBind();
        }

        public void SortGridView(GridView table, string direction, string column) {

        }


        public List<Player> GetPlayerData(string playerName) {
            return _uow.PlayersRepo.GetPlayerByName(playerName);
        }

    }
}
