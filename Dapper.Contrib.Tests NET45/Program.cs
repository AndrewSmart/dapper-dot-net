using System;
#if MSSQL
using System.Data.SqlServerCe;
#else
using System.Data;
#endif //#if MSSQL
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Dapper.Contrib.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Setup();
            RunTests();
            Setup();
            RunAsyncTests();
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        private static void Setup()
        {
            #if SQL_SERVER_CE
            var keySyntax = "IDENTITY(1,1)";
            var projLoc = Assembly.GetAssembly(typeof(Program)).Location;
            var projFolder = Path.GetDirectoryName(projLoc);

            if (File.Exists(projFolder + "\\Test.sdf"))
                File.Delete(projFolder + "\\Test.sdf");
            var connectionString = "Data Source = " + projFolder + "\\Test.sdf;";
            var engine = new SqlCeEngine(connectionString);
            engine.CreateDatabase();
            using (var connection = new SqlCeConnection(connectionString))
            {
            #elif MYSQL
            var keySyntax = "AUTO_INCREMENT KEY";
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection("server=127.0.0.1;uid=crunchbang;Database=Test;"))
            {
                connection.Execute(@"drop database Test; create database Test; use Test;");
            #endif
                connection.Open();
                connection.Execute(String.Format(@" create table Stuff (TheId int {0} not null, Name nvarchar(100) not null, Created DateTime null) ", keySyntax));
                connection.Execute(String.Format(@" create table People (Id int {0} not null, Name nvarchar(100) not null) ", keySyntax));
                connection.Execute(String.Format(@" create table Users (Id int {0} not null, Name nvarchar(100) not null, Age int not null) ", keySyntax));
                connection.Execute(String.Format(@" create table Automobiles (Id int {0} not null, Name nvarchar(100) not null) ", keySyntax));
                connection.Execute(String.Format(@" create table Results (Id int {0} not null, Name nvarchar(100) not null, `Order` int not null) ", keySyntax));
                connection.Execute(@" create table ObjectX (ObjectXId nvarchar(100) not null, Name nvarchar(100) not null) ");
                connection.Execute(@" create table ObjectY (ObjectYId int not null, Name nvarchar(100) not null) ");
            }
            Console.WriteLine("Created database");
        }

        private static void RunTests()
        {
            var tester = new Tests();
            foreach (var method in typeof(Tests).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Console.Write("Running " + method.Name);
                method.Invoke(tester, null);
                Console.WriteLine(" - OK!");
            }
        }

        private static void RunAsyncTests()
        {
            var tester = new TestsAsync();
            foreach (var method in typeof(TestsAsync).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Console.Write("Running " + method.Name);
                Task.WaitAll((Task)method.Invoke(tester, null));
                Console.WriteLine(" - OK!");
            }
        }
    }
}
