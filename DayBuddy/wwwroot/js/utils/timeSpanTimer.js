function timeSpanTimer(durationSeconds, $display, timeOutCallback) {
    var timer = durationSeconds , hours, minutes, seconds;
    var interval = setInterval(function() {
        hours = parseInt(timer / 3600, 10);
        minutes = parseInt((timer % 3600) / 60, 10);
        seconds = parseInt(timer % 60, 10);

        hours = hours < 10 ? "0" + hours : hours;
        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;

        $display.text(hours + ":" + minutes + ":" + seconds);

        if (--timer < 0) {
            clearInterval(interval);
            if (typeof timeOutCallback === 'function') {
                timeOutCallback();
            }
        }
    }, 1000);
}
