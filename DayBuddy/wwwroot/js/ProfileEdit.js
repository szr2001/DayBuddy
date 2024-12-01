
// I use the jQuery instead of $ to remember in the future
// 'jQuery' is equal to writing '$'
// here we make use of Jquery already made functions
// so we don't have to write stuff like Document.getElement and stuff
jQuery(`#BtnName`).click(function () {
    //reset ChangeNameModal
    $("#NewNameErrorLabel").text("");
    $("#NewNameInput").css('border-color', 'lightgray');
    $("#NewNameInput").val("");
    //show Modal
    jQuery('#ChangeNameModal').modal('show');
})

function EditName() {
    //get the data from html
    var formData = new Object();
    formData.newName = $("#NewNameInput").val();

    //send the data
    $.ajax({
        url: '/Account/EditName',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                //make use of Id to change the name directly instead of calling reload
                //add clientside validation
                window.location.reload(true);
            }
            //if no, show the errors
            else {
                $("#NewNameInput").css('border-color','Red');
                $("#NewNameErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#NewNameErrorLabel").text("An error occurred while changing the name.");
        }
    });
}

function HideNameModal() {
    $('#ChangeNameModal').modal('hide');
}