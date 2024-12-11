$(document).ready(function () {
    //start timmer
});
function startTimer(duration, display, modelProperty) {
    var timer = duration, hours, minutes, seconds;
    var interval = setInterval(function () {
        hours = parseInt(timer / 3600, 10);
        minutes = parseInt((timer % 3600) / 60, 10);
        seconds = parseInt(timer % 60, 10);

        hours = hours < 10 ? "0" + hours : hours;
        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;

        display.textContent = hours + ":" + minutes + ":" + seconds;

        if (--timer < 0) {
            clearInterval(interval);
            // Update the model property to true
            if (modelProperty === "CanWrite") {
                location.reload();
            } else if (modelProperty === "CanRead") {
                location.reload();
            }
            // Refresh the page or update the UI as necessary
            location.reload();
        }
    }, 1000);
}