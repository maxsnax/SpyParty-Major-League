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
    public class EventsService {
        private readonly UnitOfWork _uow;

        public EventsService() {
            _uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["SML_db-connection"].ToString());
        }

        public List<Tuple<int, string>> LoadSeasons() {
            return _uow.SeasonsRepo.LoadSeasons();
        }

        public DataTable PopulateAllEventData(GridView table) {
            DataTable rawData = _uow.SeasonsRepo.GetSeasonData();
            BindData(table, rawData);

            // Return to store in viewstate as eventData
            return rawData;
        }

        private void BindData(GridView table, DataTable data) {
            if (data != null && data.Rows.Count > 0) {
                table.DataSource = data;
            }
            else {
                table.DataSource = null;
            }
            table.DataBind();
        }

        public void SortGridView(GridView table, string direction, string column) {

        }

        public DataTable GetEventData(GridView table, string eventName) {


            DataTable rawData = _uow.SeasonsRepo.GetPlayersFromSeason(eventName);
            BindData(table, rawData);
            
            // Return to store in viewstate as eventData
            return rawData;
        }

        public bool CheckEventName(string eventName) {
            bool taken = _uow.SeasonsRepo.CheckSeasonName(eventName);

            return taken;
        }

        public void CreateNewEvent(string name, string password) {
            string hashedPassword = _uow.HashPassword(password);

            _uow.SeasonsRepo.CreateSeason(name, hashedPassword);
        }

        public bool VerifyEventPassword(string eventName, string inputPassword) {
            string hashedPassword = _uow.HashPassword(inputPassword);
            string eventPassword = _uow.SeasonsRepo.VerifyPassword(eventName);

            if (hashedPassword != eventPassword) { return false; }
            
            HttpContext.Current.Session["AuthorizedSeason"] = eventName;
            return true;
        }


    }
}
