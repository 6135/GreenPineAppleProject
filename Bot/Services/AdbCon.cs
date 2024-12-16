using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenPineAppleProject.Bot.Services
{
    public class AdbCon
    {
        public static readonly OracleConnection? con;

        static AdbCon()
        {
            //Enter your ADB's user id, password, and net service name
            string conString = "User Id=<ADMIN>;Password=<MYPASSWORD>;Data Source=<mydb_high>;Connection Timeout=30;";
            //replace ADMIN with user in appsettings
            conString = conString.Replace("<ADMIN>", BotEntry.config["ConnectionStrings:user"]);
            conString = conString.Replace("<MYPASSWORD>", BotEntry.config["ConnectionStrings:password"]);
            conString = conString.Replace("<mydb_high>", BotEntry.config["ConnectionStrings:dataSource"]);

            //Enter directory where you unzipped your cloud credentials
            try
            {
                OracleConfiguration.TnsAdmin = BotEntry.config["ConnectionStrings:tnsAdminPath"];
                OracleConfiguration.WalletLocation = OracleConfiguration.TnsAdmin;
                con = new OracleConnection(conString);
                con.Open();
                SentrySdk.AddBreadcrumb(
                    message: "Successfully connected to Oracle Autonomous Database",
                    category: "Database",
                    level: BreadcrumbLevel.Info
                    );
                Console.WriteLine("Successfully connected to Oracle Autonomous Database");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SentrySdk.CaptureException(ex);
            }
        }
    }
}