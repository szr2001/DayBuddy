
// I use the jQuery instead of $ to remember in the future
// 'jQuery' is equal to writing '$'
// here we make use of Jquery already made functions
// so we don't have to write stuff like Document.getElement and stuff
jQuery(`#BtnName`).click(function () {
    jQuery('#ChangeNameModal').modal('show');
})

function EditName() {
    //get the data from html
    var formData = new Object();
    formData.newName = $("#NewNameInput").val();

    //send the data
    $.ajax({
        //set up the target
        url:'/Account/EditName',
        data:formData,
        type: 'post',
        //handle scenarious
        success: function (response) {
            alert(response);
        },
        error: function (response) {
            alert(response);
        },
    })
}

function HideNameModal() {
    $('#ChangeNameModal').modal('hide');
}