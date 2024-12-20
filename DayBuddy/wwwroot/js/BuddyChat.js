const loggedInUserId = $("#loggedInUserId").val();
const $MessagesList = $("#messagesList");
const $BuddyChatTimer = $("#buddyChatTimer");
var connection = new signalR.HubConnectionBuilder().withUrl("/BuddyHub").build();

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
