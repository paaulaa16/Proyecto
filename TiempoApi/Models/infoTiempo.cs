using System;
using System.ComponentModel.DataAnnotations;


public class infoTiempo
{

        [Key]
        public string Localidad { get; set; }
        public string Latitud { get; set; }
        public string Longitud  { get; set; }
        public string Temperatura { get; set; }
        public string Humedad { get; set; }
        public string VelocidadViento { get; set; }
        public string Precipitacion { get; set; }

        public string Hora { get; set; }

}