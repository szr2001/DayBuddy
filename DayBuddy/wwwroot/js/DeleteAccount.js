var $deleteBtn = $("#DeleteBtn");

$(document).ready(function () {
    setTimeout(EnableButton,3000);
})

function EnableButton() {
    $deleteBtn.removeClass("disabled");
}