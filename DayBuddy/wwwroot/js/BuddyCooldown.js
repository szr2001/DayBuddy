function cooldownEndCallback() {
    window.location.href = '/DayBuddy/SearchBuddy';
}

function startTimer() {
    //the & here is a naming convention to show that this is a jquery object
    var $cooldownIndicator = $("#cooldownIndicator");
    var cooldownAmount = parseInt($cooldownIndicator.attr('dataCooldown'), 10);
    timeSpanTimer(cooldownAmount, $cooldownIndicator, cooldownEndCallback);
}

$(document).ready(startTimer);

