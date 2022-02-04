using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

    public class DatosContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(connString);
    public static string connString = $"Server=185.60.40.210\\SQLEXPRESS,58015;Database=DB02Paula;User Id=sa;Password=Pa88word;MultipleActiveResultSets=true;";
        public DbSet<infoTiempo> TiempoInfo { get; set; }

    }