using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

    public class DatosContext : DbContext
    {
        
        public DbSet<infoTiempo> TiempoInfo { get; set; }

        public string connString { get; private set; }

        public DatosContext()
        {
            // var database = "prueba"; // "EF{XX}Nombre" => EF00Santi
            // connString = $"Server=(localdb)\\mssqllocaldb;Database={database};MultipleActiveResultSets=true";

            string BDAlumno = "DB02Paula";
            connString = $"Server=185.60.40.210\\SQLEXPRESS,58015;Database={BDAlumno};User Id=sa;Password=Pa88word;MultipleActiveResultSets=true;";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(connString);


    }