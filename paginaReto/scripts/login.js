var button = document.getElementById("btn"); //variable del boton
button.addEventListener("click", Login); //el evento para llamar a la funcion 

//funcion para logearte y despues que te dirija a la pagina de los marcadores
function Login() {
    $.ajax({
        type: "POST",
        dataType: "json",
        url: `http://10.10.17.189:5000/api/Users/Authenticate/username/${document.getElementById('user').value}/password/${document.getElementById('pwd').value}`,
        headers: {
            accept: "application/json",
        }
    }).then(function(data) {
        localStorage.Token = JSON.stringify(`${data.token}`);

        window.location.href = "./main.html";
    }).fail(function(err) {
        alert("Usuario o contraseña incorrecta.")
    });
}