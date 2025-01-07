


//send Email
$(`#SendEmailBtn`).click(function () {
    $('#SendEmailModal').modal('show');
})
function SendEmail() {

}

//gift Premium
$(`#GiftPremiumBtn`).click(function () {
    $('#GivePremiumModal').modal('show');
    $("#GivePremiumErrorLabel").text("");
})
function GiftPremium(days) {
    var formData = new Object();
    formData.days = days;
    //send the data
    $.ajax({
        url: '/Admin/GiftPremium',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                $('#GiftPremiumBtn').prop('disabled', true);
                $('#GivePremiumModal').modal('hide');
            }
            //if no, show the errors
            else {
                $("#GivePremiumErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#GivePremiumErrorLabel").text("An error occurred while changing the name.");
        }
    });
}