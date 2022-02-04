import L from 'leaflet';

var zoom = 8.5; //el zoom para el mapa
var cont = 0; //el contador para poner el maximo de 4 paneles

//el mapa 
var map = L.map('map').setView([43.0644, -2.4898], zoom);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
var token = JSON.parse(localStorage.Token);
var aPaneles = {}; //se crea un array vacio

//variable del icono seleccionado, es decir, el azul
const iconoSeleccionado = new L.Icon({
    iconUrl: './imagenes/Seleccionado.png',
    iconSize: [40, 40]
});

//variable del icono predeterminado, es decir, el negro
const iconoPredeterminado = new L.Icon({
    iconUrl: './imagenes/Predeterminado.png',
    iconSize: [40, 40]
});

var idBaliza = "";

//funcion para devolver las balizas con el token 
function DevolverBalizas() {
    $.ajax({
        type: "GET",
        dataType: "html",
        url: "http://10.10.17.189:5000/api/infoTiempo",
        headers: {
            accept: "application/json",
            Authorization: "Bearer " + token,
        },
    }).then(function(data) {
        window.aDatos = JSON.parse(data);

        if (localStorage.Elementos == undefined) { //si no esta creado
            for (var i = 0; i < aDatos.length; i++) { //recorre el array donde estan los datos del json
                aPaneles[aDatos[i].localidad] = { "seleccionado": false, "viento": false, "precipitacion": false }; //se añaden al array vacio creado anteriormente (ssolo el Id + los datos seleccionado, viento y precipitacion)

            }
            localStorage.Elementos = JSON.stringify(aPaneles); //lo guarda y lo pasa a string
        } else { //sino
            aPaneles = JSON.parse(localStorage.Elementos); //los guarda en el array
        }
        $.each(aDatos, (i) => { //recorre el array de los datos

            if (aPaneles[aDatos[i].localidad].seleccionado) { //si el marcador seleccionado es true
                var marcador = iconoSeleccionado; //el marcador se convertira en el icono azul
                $("#paneles").append("<div class='card' id='" + aDatos[i].localidad + "'><h4>" + aDatos[i].localidad.toUpperCase() + "</h4><br><h7>Temperatura: " + aDatos[i].temperatura + "ºC</h7><h7>Humedad: " + aDatos[i].humedad + "%</h7></div>"); //se crea el div con los datos
                idBaliza = aDatos[i].localidad;
                cont++;
                $("#" + aDatos[i].localidad).droppable({ //coges el id del div para hacer la funcion del drop 
                    drop: function(event, ui) {
                        if (ui.draggable[0].id == "viento" && !aPaneles[aDatos[i].localidad].viento) { //si la id del dagrable es viento y si en el localstorage el viento esta en false
                            $("#" + aDatos[i].localidad).append(`<p id='velo${aDatos[i].localidad}'>Velocidad del Viento: ${aDatos[i].velocidadViento}Km/h</p>`); //se añade al div el "p"
                            //funcion del dragable para el div de la velocidad del viento
                            $(`#velo${aDatos[i].localidad}`).draggable({
                                revert: "invalid",
                                helper: "clone"
                            });
                            aPaneles[aDatos[i].localidad].viento = true; //en el localstorage se cambia a true
                            localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda

                        } else if (ui.draggable[0].id == "precipitacion" && !aPaneles[aDatos[i].localidad].precipitacion) { //si la id del dragable es precipitacion y en el localstorage esta en false
                            $("#" + aDatos[i].localidad).append(`<p id='pres${aDatos[i].localidad}'>Precipitacion: ${aDatos[i].precipitacion}%</p>`); //se añade al div el "p"
                            //funcion para el dagrable de la precipitacion
                            $(`#pres${aDatos[i].localidad}`).draggable({
                                revert: "invalid",
                                helper: "clone"
                            });
                            aPaneles[aDatos[i].localidad].precipitacion = true; //en el localstorage se cambia a true
                            localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda
                        }
                    }
                });
                if (aPaneles[aDatos[i].localidad].viento) { //si la id del panel es del viento
                    $("#" + aDatos[i].localidad).append(`<p id='velo${aDatos[i].localidad}'>Velocidad del Viento: ${aDatos[i].velocidadViento}Km/h</p>`); //se añade la linea "p" del viento
                    //dragable para el div de la velocidad del viento
                    $(`#velo${aDatos[i].localidad}`).draggable({
                        revert: "invalid",
                        helper: "clone"
                    });
                }

                if (aPaneles[aDatos[i].localidad].precipitacion) { //si la id del panel es de la precipitacion
                    $("#" + aDatos[i].localidad).append(`<p id='pres${aDatos[i].localidad}'>Precipitacion: ${aDatos[i].precipitacion}%</p>`); //se añade la linea "p" de la precipitacion
                    //dragable para el div de la precipitacion
                    $(`#pres${aDatos[i].localidad}`).draggable({
                        revert: "invalid",
                        helper: "clone"
                    });
                }

            } else if (!aPaneles[aDatos[i].localidad].seleccionado) { //en cambio, si el marcador no esta seleccionado y en localstorage esta en false
                var marcador = iconoPredeterminado; //se le cambia el icono de nuevo al predeterminado
            }

            var latitud = (aDatos[i].latitud).replace(',', '.');
            var longitud = (aDatos[i].longitud).replace(',', '.');

            const marker = new L.marker([parseFloat(latitud), parseFloat(longitud)], {
                    //me creo el marcador con el icono predeterminado
                    icon: marcador
                })
                .on('click', function(e) { //cuando se haga clic en uno de los marcadores que haga lo siguiente:
                    //si el marcador esta de color azul, que vuelva a ser el predeterminado es decir, el negro
                    if (this.options.icon.options.iconUrl == iconoSeleccionado.options.iconUrl) { //si las 2 urls de los marcadores (el creado y el seleccionado) son iguales 
                        this.setIcon(iconoPredeterminado); //se le cambia el icono al predeterminado (el negro)
                        aPaneles[aDatos[i].localidad].seleccionado = false; //en el localstorage el "seleccionado" se volvera false

                        localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda
                        $("#" + aDatos[i].localidad).remove(); //se borra el panel de dicho marcador
                        cont--; //restamos -1 al contador de los paneles (como maximo seran siempre 4)

                        //si el marcador esta de color negro, que se ponga de color azul que seria el seleccionado
                    } else if (this.options.icon.options.iconUrl == iconoPredeterminado.options.iconUrl && cont < 4) { //si las 2 urls son iguales y si el contador tiene menos de 4
                        this.setIcon(iconoSeleccionado); //cambiamos el icono al seleccionado
                        aPaneles[aDatos[i].localidad].seleccionado = true; //en el localstorage el "seleccionado" se cambiara a true
                        localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda
                        $("#paneles").append("<div class='card' id='" + aDatos[i].localidad + "'><h4>" + aDatos[i].localidad + "</h4><br><h7>Temperatura: " + aDatos[i].temperatura + "ºC</h7><h7>Humedad: " + aDatos[i].humedad + "%</h7></div>"); //añadimos el div (el panel) con la informacion
                        idBaliza = aDatos[i].localidad;
                        cont++; //sumamos +1 al contador de los paneles
                    }

                    //para añadir dicho elemento al panel con el resto de la informacion ya añadida
                    $("#" + aDatos[i].localidad).droppable({
                        drop: function(event, ui) {
                            if (ui.draggable[0].id == "viento" && !aPaneles[aDatos[i].localidad].viento) { //si la id del dragable es viento y si en el localstorage el "viento" esta en false
                                $("#" + aDatos[i].localidad).append(`<p id='velo${aDatos[i].localidad}'>Velocidad del Viento: ${aDatos[i].velocidadViento}Km/h</p>`); //se añade el texto al div
                                $(`#velo${aDatos[i].localidad}`).draggable({
                                    revert: "invalid",
                                    helper: "clone"
                                });
                                //se añade un nuevo "p" (que en este caso seria sobre el viento) al panel
                                aPaneles[aDatos[i].localidad].viento = true; //en el localstorage se cambia a true
                                localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda

                            }
                            if (ui.draggable[0].id == "precipitacion" && !aPaneles[aDatos[i].localidad].precipitacion) { //si la id del dragable es precipitacion y si en el localstorage el "precipitacion" esta en false
                                $("#" + aDatos[i].localidad).append(`<p id='pres${aDatos[i].localidad}'>Precipitacion: ${aDatos[i].precipitacion}%</p>`); //se añade el texto al div

                                $(`#pres${aDatos[i].localidad}`).draggable({
                                    revert: "invalid",
                                    helper: "clone"
                                });
                                //se añade un nuevo "p" (que en este caso seria sobre la precipitacion) al panel
                                aPaneles[aDatos[i].localidad].precipitacion = true; //en el localstorage se cambia a true
                                localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda
                            }
                        }
                    });
                })
                .bindPopup(aDatos[i].localidad)
                .addTo(map); //añadimos al mapa los marcadores

            //para que los elementos a añadir a los paneles se puedan arrastrar al panel
            $("#viento").draggable({
                revert: "invalid",
                helper: "clone",
                zIndex: 1000
            });

            $("#precipitacion").draggable({
                revert: "invalid",
                helper: "clone",
                zIndex: 10000
            });

            $(`#velo${aDatos[i].localidad}`).draggable({
                revert: "invalid",
                helper: "clone"
            });

            $(`#pres${aDatos[i].localidad}`).draggable({
                revert: "invalid",
                helper: "clone"
            });

        }).fail(function(err) {
            console.log("ERROR: " + err);
        })
    });
}

$(document).ready(() => {
    DevolverBalizas(); //llamada a la funcion

    //el dropable de la basura para quitar el texto del div de la informacion de la localidad
    $("#imgBasura").droppable({
        drop: function(event, ui) {
            var idDrag = ui.draggable[0].id; //id entera preIrun
            var idPrincipio = idDrag.substring(0, 4); //el principio del id ej: pres
            var idFinal = idDrag.substring(4); //el final de la id ej: irun

            if (ui.draggable[0].id == `velo${idFinal}` && aPaneles[idFinal].viento) { //si la id del dragable es velo+numeros y si en el localstorage el viento esta en true
                $(`#velo${idFinal}`).remove(); //borra del div esa "p"
                aPaneles[idFinal].viento = false; //en el localstorage se vuelve a false
                localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda
            }

            if (ui.draggable[0].id == `pres${idFinal}` && aPaneles[idFinal].precipitacion) { //si la id del dragable es precipitacion+numeros y si en el localstorage la precipitacion esta en true
                $(`#pres${idFinal}`).remove(); //se borra del div la "p" correspondiente
                aPaneles[idFinal].precipitacion = false; //en el localstorage se vuelve a false
                localStorage.Elementos = JSON.stringify(aPaneles); //y se guarda
            }
        }
    })
});