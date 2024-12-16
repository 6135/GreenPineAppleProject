using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenPineAppleProject.Bot.Services;
using GreenPineAppleProject.Bot;

namespace GreenPineAppleProject.Data
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string conString = BotEntry.config["ConnectionStrings:conString"] ?? "User Id=<USER>;Password=<PASSWORD>;Data Source=<DATA_SOURCE>;";
            //replace <> with your values
            conString = conString.Replace("<USER>", BotEntry.config["ConnectionStrings:user"]);
            conString = conString.Replace("<PASSWORD>", BotEntry.config["ConnectionStrings:password"]);
            BotEntry.config["ConnectionStrings:conString"] = conString.Replace("<DATA_SOURCE>", BotEntry.config["ConnectionStrings:dataSource"]);
            optionsBuilder.UseOracle(BotEntry.config["ConnectionStrings:conString"]);
        }
    }
}