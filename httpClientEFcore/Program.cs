using System.Reflection;
using System;

using System.Net.Http;
using System.Net.Http.Headers;
using NW = Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
//using Colorful;
using System.Drawing;
//using Console = Colorful.Console;
using System.Collections.Generic;
using Newtonsoft.Json;

await Main();

static async Task Main()
{
    //Cada hora lee de Euskalmet
    TimeSpan interval = new TimeSpan(0, 0, 1200);
    while (true)
    {

        await TareaAsincrona();
        Thread.Sleep(interval);

    }
}

static async Task TareaAsincrona()
{
    var client = new HttpClient();
    var key = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJtZXQwMS5hcGlrZXkiLCJpc3MiOiJJRVMgUExBSUFVTkRJIEJISSBJUlVOIiwiZXhwIjoyMjM4MTMxMDAyLCJ2ZXJzaW9uIjoiMS4wLjAiLCJpYXQiOjE2Mzk5OTAyMzYsImVtYWlsIjoiaWthYmFAcGxhaWF1bmRpLm5ldCJ9.MEJufck_C20y2HafuyHFzjIwSyTD0px7qTxinNKshORIBnwZpGHCDR4a2ZGcJg2DDa0j-G_yZht6cuhj7ZdheR5SatWjywLckX-UdQpo7BIxLXPgqVcWItrfTWRKBlZgah93GN2EAAma8QEHLQnPSNA2XgaEw7B6Oh380DR-kw5AaNCse4f_Vy6mQgXQ5OefeSD4k7PS3vtBAyz0IzjRbKkwweNa5fbKjNNKQH_oGdWYz-RkyGRq2e6JjIpVS_bW_m-MvgVz7dx83RgeEmFBiOPAe9_pmwNnxYM4ZGX-9kanMxXaRTlbCQqmwfiRBAXbsMXiyKrSz4hRoQWDfgWnbw";
    client.DefaultRequestHeaders.Add("User-Agent", "mi consola");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + key);
    //Hacemos la primera peticion a la API para recibir las regiones
    var urlRegiones = $"https://api.euskadi.eus/euskalmet/geo/regions/basque_country/zones";
    //Esperamos a recibir la respuesta de la API
    HttpResponseMessage respuestaRegiones = await client.GetAsync(urlRegiones);
    //Esperamos a recibir el contenido de la respuesta
    var sRespRegiones = await respuestaRegiones.Content.ReadAsStringAsync();
    //Parseamos la respuesta de string a objeto json
    dynamic jsonObjectRegiones = NW.JsonConvert.DeserializeObject(sRespRegiones);
    //Limpiamos lo que haya en la consola para arrancar de 0

    //Console.WriteLine("COMIENZA LA ACTUALIZACION DE LA BASE DE DATOS", Color.Gold);

    //Guardamos valores de la fecha actual del sistema
    var diaHoy = DateTime.Today.Day;
    var AñoHoy = DateTime.Today.Year;
    var mesHoy = DateTime.Today.Month;
    var hora = Convert.ToInt32(DateTime.Now.Hour) + 1;

    string urlLocalidades2;
    HttpResponseMessage respuestaLocalidades2;
    dynamic jsonObjectLocalidades2;


    var client2 = new HttpClient();
    var key2 = "950021020128f2515d0efed24a8d2983";
    client2.DefaultRequestHeaders.Add("User-Agent", "mi consola");
    client2.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client2.DefaultRequestHeaders.Add("Authorization", "Bearer " + key2);

    string lat;
    string lon;
    string hum;
    string t;

    foreach (var item in jsonObjectRegiones)
    {

        urlLocalidades2 = $"https://api.euskadi.eus/euskalmet/geo/regions/basque_country/zones/{item.regionZoneId}/locations";
        respuestaLocalidades2 = await client.GetAsync(urlLocalidades2);
        var sRespLocalidades2 = await respuestaLocalidades2.Content.ReadAsStringAsync();
        //Parseamos la respuesta de string a objeto json
        jsonObjectLocalidades2 = NW.JsonConvert.DeserializeObject(sRespLocalidades2);
        //Console.WriteLine(jsonObjectLocalidades2);
        foreach (var item2 in jsonObjectLocalidades2)
        {
            using (var db = new DatosContext())
            {
                try
                {
                    if (hora == 0) hora = 24;// La hora del Pc marca 0 cuando son las 12 de la noche, asi se corrige el error que produciria
                    string sHora;
                    //Si es antes de las 10 de la mañana agregamos un 0
                    if (hora < 10)
                    {
                        sHora = "0" + (hora);
                    }
                    else
                    {
                        sHora = (hora).ToString();
                    }
                    var urlLocalizacionForecast1 = $"https://api.euskadi.eus/euskalmet/weather/regions/basque_country/zones/{item.regionZoneId}/locations/{item2.regionZoneLocationId}/forecast/trends/measures/at/{AñoHoy}/0{mesHoy}/{diaHoy}/for/{AñoHoy}0{mesHoy}{diaHoy}";
                    HttpResponseMessage respuestaRegistrosDeTiempo1 = await client.GetAsync(urlLocalizacionForecast1);
                    var sRespRegistrosDeTiempo1 = await respuestaRegistrosDeTiempo1.Content.ReadAsStringAsync();
                    //Este objeto contiene 24 registros diferentes, uno por cada hora
                    dynamic jsonObjectRegistrosDeTiempo1 = NW.JsonConvert.DeserializeObject(sRespRegistrosDeTiempo1);
                    int ultimoRegistro = 0;
                    //En este bucle buscamos el registro mas actual basandonos en la hora del pc
                    for (var x = 0; x < jsonObjectRegistrosDeTiempo1.trends.set.Count; x++)
                    {
                        //Si el string del apartado range del registro que estamos iterando contiene los digitos buscados, lo hemos encontrado
                        if (((jsonObjectRegistrosDeTiempo1.trends.set[x].range)).ToString().Contains(sHora))
                        {
                            ultimoRegistro = x; //Guardamos la posicion de la iteracion, ya que coincidira con la posicion del registro en el jsonObjectRegistrosDeTiempo

                        }
                    }
                    dynamic muni = jsonObjectRegistrosDeTiempo1.regionZoneLocation.regionZoneLocationId;
                    dynamic temp = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].temperature;
                    dynamic prec = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].precipitation;
                    dynamic vvi = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].windspeed;
                    dynamic desc = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].symbolSet.weather.nameByLang.SPANISH;
                    dynamic pathImg = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].symbolSet.weather.path;
                    dynamic rangHor = jsonObjectRegistrosDeTiempo1.trends.set[ultimoRegistro].range;


                    //Ahora sacamos otros datos de la otra API
                    var urlOpenWeather = $"https://api.openweathermap.org/data/2.5/weather?q={muni},ES&appid={key2}";
                    HttpResponseMessage respuesta = await client2.GetAsync(urlOpenWeather);
                    var sResp = await respuesta.Content.ReadAsStringAsync();

                    Root2 myDeserializedClass = JsonConvert.DeserializeObject<Root2>(sResp);

                    infoTiempo itemNuevo;
                    try
                    {
                        lat = myDeserializedClass.coord.lat.ToString();
                        lon = myDeserializedClass.coord.lon.ToString();
                        hum = myDeserializedClass.main.humidity.ToString();
                        t = myDeserializedClass.main.temp.ToString();

                        //Algunas consultas con LINQ
                        Root jsonObjectHistorialDeTiempo = JsonConvert.DeserializeObject<Root>(sRespRegistrosDeTiempo1);

                        // Temperaturas maximas y minimas del dia
                        var registrosOrdenados = from reg in jsonObjectHistorialDeTiempo.trends.set orderby reg.temperature.value ascending select reg;
                        var tempMin = registrosOrdenados.First().temperature.value;
                        var tempMax = registrosOrdenados.Last().temperature.value;

                        //Temperatura media del dia

                        var listaTemperaturas = from reg in jsonObjectHistorialDeTiempo.trends.set select reg.temperature.value;
                        var tempMedia = listaTemperaturas.Average();

                        //PARA METER LOS DATOS DESDE 0 , SOLO METERA DATOS QUE TENGAN EN COMUN EUSKALMET Y OPENWEATHER

                        float latitud = float.Parse(lat);
                        float longitud = float.Parse(lon);

                        if (latitud >= 42 && latitud < 44 && longitud <= -1 && longitud > -4)
                        {
                            itemNuevo = new infoTiempo
                            {
                                Localidad = $"{item2.regionZoneLocationId}",
                                Hora = $"{diaHoy}/{mesHoy}/{AñoHoy} : {sHora}",
                                Temperatura = $"{(temp.value).ToString()}",
                                VelocidadViento = $"{(vvi.value).ToString()}",
                                Precipitacion = $"{(prec.value).ToString()}",
                                Humedad = $"{hum}",
                                Latitud = $"{latitud}",
                                Longitud = $"{longitud}"
                            };

                            //cuando no tienes la bbdd creada para que te los inserte por primera vez
                            // db.TiempoInfo.Add(itemNuevo);
                            // db.SaveChanges();

                            //cuando la bbdd ya esta creada se usa para actualizar los datos simplemente 
                            string localizacion = itemNuevo.Localidad;
                            var row = db.TiempoInfo.Where(a => a.Localidad == localizacion).Single();
                            row.Hora = itemNuevo.Hora;
                            row.VelocidadViento = itemNuevo.VelocidadViento;
                            row.Temperatura = itemNuevo.Temperatura;
                            row.Precipitacion = "0";
                            row.Humedad = itemNuevo.Humedad;
                            db.SaveChanges();

                            Console.WriteLine("El registro se actualizo correctamente +" + muni + " Region =>" + item.regionZoneId);

                            Console.WriteLine($"{itemNuevo.Localidad}");
                            Console.WriteLine($"{itemNuevo.Hora}");
                            Console.WriteLine($"{itemNuevo.Temperatura} ºC");
                            Console.WriteLine($"{itemNuevo.VelocidadViento} km/h");
                            Console.WriteLine($"{itemNuevo.Precipitacion} ml");
                            Console.WriteLine($"{itemNuevo.Humedad} %");
                            Console.WriteLine($"{itemNuevo.Latitud}");
                            Console.WriteLine($"{itemNuevo.Longitud}");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine($"No hay respuesta de OpenWeather para el municipio {muni}, y no se guardo el registro");
                    }


                }
                catch (Exception p)
                {
                    Console.WriteLine(p);
                    Console.Write("No existe ningún registro de ese municipio en Euskalmet ? =>" + item2.regionZoneLocationId + " Region =>" + item.regionZoneId, Color.IndianRed);
                    Console.WriteLine("");
                }
            }
        }
    }

    System.Console.ForegroundColor = ConsoleColor.Yellow;
    System.Console.WriteLine(" ");
}

//Configuracion de del cliente para conectarse a Euskalmet
//MODELO DE CLASES PARA EUSKALMET
public class Region
{
    public string typeId { get; set; }
    public string key { get; set; }
    public string regionId { get; set; }
}

public class RegionZone
{
    public string typeId { get; set; }
    public string key { get; set; }
    public string regionId { get; set; }
    public string regionZoneId { get; set; }
}

public class RegionZoneLocation
{
    public string typeId { get; set; }
    public string key { get; set; }
    public string regionId { get; set; }
    public string regionZoneId { get; set; }
    public string regionZoneLocationId { get; set; }
}

public class Temperature
{
    public double value { get; set; }
    public string unit { get; set; }
}

public class Precipitation
{
    public double value { get; set; }
    public string unit { get; set; }
}

public class Winddirection
{
    public double value { get; set; }
    public string unit { get; set; }
    public string cardinalpoint { get; set; }
}

public class Windspeed
{
    public double value { get; set; }
    public string unit { get; set; }
}

public class NameByLang
{
    public string SPANISH { get; set; }
    public string BASQUE { get; set; }
}

public class DescriptionByLang
{
    public string SPANISH { get; set; }
    public string BASQUE { get; set; }
}

public class Weather
{
    public string id { get; set; }
    public string path { get; set; }
    public NameByLang nameByLang { get; set; }
    public DescriptionByLang descriptionByLang { get; set; }
}

public class SymbolSet
{
    public Weather weather { get; set; }
}

public class ShortDescription
{
    public string SPANISH { get; set; }
    public string BASQUE { get; set; }
}

public class Set
{
    public string range { get; set; }
    public Temperature temperature { get; set; }
    public Precipitation precipitation { get; set; }
    public Winddirection winddirection { get; set; }
    public Windspeed windspeed { get; set; }
    public SymbolSet symbolSet { get; set; }
    public ShortDescription shortDescription { get; set; }
}

public class Trends
{
    public List<Set> set { get; set; }
}

public class Root

{
    public string oid { get; set; }
    public int numericId { get; set; }
    public int entityVersion { get; set; }
    public DateTime at { get; set; }
    public DateTime @for { get; set; }
    public Region region { get; set; }
    public RegionZone regionZone { get; set; }
    public RegionZoneLocation regionZoneLocation { get; set; }
    public Trends trends { get; set; }
}





//MODELO DE CLASES PARA API OPEN-WEATHER
public class Coord
{
    public double lon { get; set; }
    public double lat { get; set; }
}

public class Weather2
{
    public int id { get; set; }
    public string main { get; set; }
    public string description { get; set; }
    public string icon { get; set; }
}

public class Main
{
    public double temp { get; set; }
    public double feels_like { get; set; }
    public double temp_min { get; set; }
    public double temp_max { get; set; }
    public int pressure { get; set; }
    public int humidity { get; set; }
}

public class Wind
{
    public double speed { get; set; }
    public int deg { get; set; }
}

public class Clouds
{
    public int all { get; set; }
}

public class Sys
{
    public int type { get; set; }
    public int id { get; set; }
    public double message { get; set; }
    public string country { get; set; }
    public int sunrise { get; set; }
    public int sunset { get; set; }
}

public class Root2
{
    public Coord coord { get; set; }
    public List<Weather2> weather { get; set; }
    public string @base { get; set; }
    public Main main { get; set; }
    public int visibility { get; set; }
    public Wind wind { get; set; }
    public Clouds clouds { get; set; }
    public int dt { get; set; }
    public Sys sys { get; set; }
    public int timezone { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public int cod { get; set; }
}
