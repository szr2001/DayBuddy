var $deleteBtn = $("#DeleteBtn");

$(document).ready(function () {
    setTimeout(EnableButton,3000);
})

function EnableButton() {
    $deleteBtn.attr("disabled", false);
}