using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KPI_API_Synchronizer
{
    public partial class Synchronizer : Form
    {
        String dbConnectionString = string.Format("server={0};uid={1};pwd={2};database={3};", ConstantMysql.HOST, ConstantMysql.USERNAME, ConstantMysql.PASSWORD, ConstantMysql.DB_NAME);

        public Synchronizer()
        {
            InitializeComponent();
        }

        private void Synchronizer_Load(object sender, EventArgs e)
        {
            textBoxTahun.Text = DateTime.Now.Year.ToString();
        }

        private void logAdd(String message) {
            listBoxLog.Items.Add(DateTime.Now.ToString() + " : " + message);
        }

        private void buttonProcess_Click(object sender, EventArgs e)
        {
            this.logAdd("Beginning Sync Data To server");
            this.logAdd("Reading URL From KPI Database");
            buttonProcess.Enabled = false;

            backgroundWorkerProcess.RunWorkerAsync();
        }

        private Boolean check_mysql_connection(object sender)
        {            
            var conn = new MySqlConnection(dbConnectionString);
            try
            {
                conn.Open();
                (sender as BackgroundWorker).ReportProgress(0, "Connected to Mysql Server");
            }
            catch (MySqlException e)
            {
                (sender as BackgroundWorker).ReportProgress(0, "Connection to Mysql Server Failed : " + e.Message);
                conn.Close();
                conn.Dispose();
                return false;
            }
            conn.Close();
            conn.Dispose();
            return true;
        }

        private MySqlDataReader get_api_url_from_server(object sender) {
            var conn = new MySqlConnection(dbConnectionString);
            conn.Open();
            var query = @"SELECT
                            CONCAT(period.start_year, '-', period.start_month) AS start_periode,
                            CONCAT(period.end_year, '-', period.end_month) AS end_periode,
                            kpi_item.id,
                            kpi_item.external
                            FROM
                            kpi_item
                            INNER JOIN period ON kpi_item.period = period.id
                            WHERE
                            kpi_item.external <> '' AND
                            CONCAT(period.start_year, '-', period.start_month) = '2018-1' AND
                            CONCAT(period.end_year, '-', period.end_month) = '2018-6'";

            var cmd = new MySqlCommand(query, conn);
            var dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                (sender as BackgroundWorker).ReportProgress(0, dataReader["external"].ToString());
            }

            conn.Close();
            cmd.Dispose();
            conn.Dispose();
            return dataReader;
        }

        private void backgroundWorkerProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonProcess.Enabled = true;
            GC.Collect();
        }

        private void backgroundWorkerProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            if (check_mysql_connection(sender) == false) {
                return;
            }

            get_api_url_from_server(sender);

            (sender as BackgroundWorker).ReportProgress(0, "Still Running");
        }

        private void backgroundWorkerProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.logAdd(e.UserState as string);
        }
    }
}
