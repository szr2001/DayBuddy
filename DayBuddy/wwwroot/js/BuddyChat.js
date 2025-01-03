const loggedInUserId = $("#loggedInUserId").val();
const $MessagesList = $("#messagesList");
const $BuddyChatTimer = $("#buddyChatTimer");
const $BuddyProfile = $("#BuddyProfile")
const $UserChat = $("#UserChat")
var connection = new signalR.HubConnectionBuilder().withUrl("/BuddyHub").build();
var reportReasonMax = 20;
var reportReasonLength = 0;
// Disable the send button until connection is established.
$("#sendButton").prop("disabled", true);
$("#messageInput").prop("disabled", true);

connection.on("ReceiveMessage", function (user, message) {
    var div = $("<div></div>").addClass("p-2 rounded-6 text-white");
    
    if (user === loggedInUserId) {
        div.addClass("background-grass-green align-self-start");
    } else {
        div.addClass("bg-secondary align-self-end");
    }

    div.text(message);
    $MessagesList.append(div);
    ScrollMessageList();
});

function cooldownEndCallback() {
    $BuddyChatTimer.addClass("text-grass-green");
    $BuddyChatTimer.removeClass("text-black-50");
}

function toggleBuddyProfile() {
    console.log("RAWR");
    if ($BuddyProfile.hasClass("d-none")) {
        $BuddyProfile.removeClass("d-none");
    }
    else {
        $BuddyProfile.addClass("d-none");
    }
}

function startTimer() {
    //the & here is a naming convention to show that this is a jquery object
    var $cooldownIndicator = $("#buddyChatTimer");
    if ($cooldownIndicator == null) return;
    var cooldownAmount = parseInt($cooldownIndicator.attr('dataCooldown'), 10);
    timeSpanTimer(cooldownAmount, $cooldownIndicator, cooldownEndCallback);
}

function ScrollMessageList() {
    $MessagesList.scrollTop($MessagesList[0].scrollHeight);
}

function loadMessages() {
    var scrollHeightBefore = $MessagesList[0].scrollHeight;
    var $loading = $("<div>Loading messages...</div>").addClass("fw-bold text-center text-grass-green");
    $MessagesList.prepend($loading); 

    $.ajax({
        url: '/DayBuddy/GetBuddyMessages',
        type: 'get',
        datatype: 'json',
        data: { offset: $("#messagesList").children().length - 1 }, //- 1 because of the loading element
        contentype: 'application/json;charset=utf-8',
        success: function (result) {
            if (result.success) {
                result.messagesFound.forEach(function (mess) {
                    var $div = $("<div></div>").addClass("p-2 rounded-6 text-white");
                    if (mess.senderId === loggedInUserId) {
                        $div.addClass("background-grass-green align-self-start");
                    } else {
                        $div.addClass("bg-secondary align-self-end");
                    }

                    $div.text(mess.message);
                    $MessagesList.prepend($div);
                });

            } else {
                console.error(result.errors.join(", "));
            }
        },
        error: function () {
            console.error("Failed to load messages");
        },
        complete: function () {
            $loading.remove();
            var scrollHeightAfter = $MessagesList[0].scrollHeight;
            $MessagesList.scrollTop(scrollHeightAfter - scrollHeightBefore);
        }
    })
}

$("#messagesList").on("scroll", function () {
    if ($(this).scrollTop() === 0) {
        loadMessages();
    }
});

$(document).ready(function ()
{
    RecordReportInputChars();
    loadMessages();
    startTimer();
});

connection.on("UnMatched", function () {
    window.location.href = '/DayBuddy/SearchBuddy';
});

connection.start().then(function () {
    $("#sendButton").prop("disabled", false);
    $("#messageInput").prop("disabled", false);
    $("#sendButton").on("click", SubmitText);
    $("#messageForm").on("submit", SubmitText);
}).catch(function (err) {
    console.error(err.toString());
});

function SubmitText(event) {
    var message = $("#messageInput");
    if (message.val() === "") return;
    connection.invoke("SendMessage", message.val()).catch(function (err) {
        console.error(err.toString());
    });
    message.val("");
    event.preventDefault();
}

function SendReport() {
    if (reportReasonLength <= reportReasonMax) {
        var formData = new Object();
        formData.reason = $("#ReportReasonInput").val();
        $.ajax({
            url: '/DayBuddy/ReportDayBuddy',
            data: formData,
            type: 'post',
            success: function (response) {
                if (!response.success) {
                    $("#ReportLabel").removeClass("text-info");
                    $("#ReportLabel").addClass("text-danger");
                    $("#ReportLabel").text(response.errors.join(", "));
                }
                else {
                    window.location.reload(true);
                }
            },
            error: function () {
                $("#ReportLabel").text("An error occurred while reporting the user.");
            }
        });
    }
}

function RecordReportInputChars() {
    $("#ReportReasonInput").on("keyup", function () {
        reportReasonLength = $(this).val().length;
        if (reportReasonLength > reportReasonMax) {
            $("#ReportLabel").addClass("text-danger");
            $("#ReportLabel").removeClass("text-info");

            $("#ReportReasonInput").removeClass("green-outline-focus");
            $("#ReportReasonInput").addClass("is-invalid");
        }
        else {
            $("#ReportLabel").removeClass("text-danger");
            $("#ReportLabel").addClass("text-info");

            $("#ReportReasonInput").addClass("green-outline-focus");
            $("#ReportReasonInput").removeClass("is-invalid");
        }
        $("#ReportLabel").text(reportReasonLength +"/"+ reportReasonMax);
    });
}
function HideNameModal() {
    $("#ReportUserModal").modal("hide");
}
function ShowReportModal() {
    reportReasonLength = 0;

    $("#ReportLabel").removeClass("text-danger");
    $("#ReportLabel").addClass("text-info");
    $("#ReportLabel").text(reportReasonLength + "/" + reportReasonMax);
    $("#ReportReasonInput").val("");

    $("#ReportBtn").addClass("disabled");
    setTimeout(function () {
        $("#ReportBtn").removeClass("disabled");
    }, 2000);

    $("#ReportUserModal").modal("show");
}