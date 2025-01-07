//send Email
$(`#SendEmailBtn`).click(function () {
    $('#SendEmailModal').modal('show');
    $("#SendEmailErrorLabel").text("");
})
function SendEmail() {
    var content = $("#SendEmailContent").val();

    if (content == null) {

        $("#SendEmailErrorLabel").text("Email content can't be empty");
        return;
    }

    $("#SendEmailModalBtn").prop('disabled', true);

    var formData = new Object();
    formData.content = content;
    //send the data
    $.ajax({
        url: '/Admin/SendEmail',
        data: formData,
        type: 'post',
        //if the call was a success, check if the action was a success
        success: function (response) {
            //if yes, reload the page
            if (response.success) {
                $('#SendEmailBtn').prop('disabled', true);
                $('#SendEmailModal').modal('hide');
            }
            //if no, show the errors
            else {
                $("#SendEmailModalBtn").prop('disabled', false);
                $("#SendEmailErrorLabel").text(response.errors.join(", "));
            }
        },
        //if the call couldn't be made, show a generic error
        error: function () {
            $("#SendEmailModalBtn").prop('disabled', false);
            $("#SendEmailErrorLabel").text("An error occurred while changing the name.");
        }
    });
}

//gift Premium
$(`#GiftPremiumBtn`).click(function () {
    $('#GivePremiumModal').modal('show');
    $("#GivePremiumErrorLabel").text("");
})
function GiftPremium(days) {

    if (days > 5) {
        $("#GivePremiumErrorLabel").text("Can't give more than 5 days premium as a reward");
        return;
    }
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