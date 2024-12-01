
// I use the jQuery instead of $ to remember in the future
// 'jQuery' is equal to writing '$'
// here we make use of Jquery already made functions
// so we don't have to write stuff like Document.getElement and stuff
jQuery(`#BtnName`).click(function () {
    jQuery('#ChangeNameModal').modal('show');
})

function EditName() {

}

function HideNameModal() {
    $('#ChangeNameModal').modal('hide');
}