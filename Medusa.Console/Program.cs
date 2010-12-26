using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Medusa;

namespace Medusa.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            MedusaMapper medusa = new MedusaMapper(@"Server=server;Database=MedusaTestDB;Uid=login;Pwd=pass; CharSet=utf8;");

            // get all users
            foreach (var item in medusa.ExecuteList<User>("GetUsers"))
            {
                System.Console.WriteLine(item.Surename + " " + item.Name);
            }
            System.Console.WriteLine("\n-------------------------------------\n");

            // search for users with "B" starting letter in their surename
            List<DbParameter> parameters = new List<DbParameter>();
            parameters.Add(new DbParameter("userSurename", DbDirection.Input, "B%"));

            foreach (var item in medusa.ExecuteList<User>("SearchUsers", parameters))
            {
                System.Console.WriteLine(item.Surename + " " + item.Name);
            }

            System.Console.ReadLine();
        }
    }

    public class User
    {
        public ulong ID { get; set; }
        public string Name { get; set; }
        public string Surename { get; set; }
        public DateTime DateBirth { get; set; }
        public sbyte Position { get; set; }
    }
}
