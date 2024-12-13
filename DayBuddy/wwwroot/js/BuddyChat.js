const loggedInUsername = $("#loggedInUsername").val();
const $MessagesList = $("#messagesList");
const $BuddyChatTimer = $("#buddyChatTimer");
var connection = new signalR.HubConnectionBuilder().withUrl("/BuddyHub").build();

// Disable the send button until connection is established.
$("#sendButton").prop("disabled", true);

connection.on("ReceiveMessage", function (user, message) {
    var div = $("<div></div>").addClass("p-2 rounded-6 text-white");
    
    if (user === loggedInUsername) {
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
    var cooldownAmount = parseInt($cooldownIndicator.attr('dataCooldown'), 10);
    timeSpanTimer(cooldownAmount, $cooldownIndicator, cooldownEndCallback);
}

function ScrollMessageList() {
    $MessagesList.scrollTop($MessagesList[0].scrollHeight);
}

function loadMessages() {
    $.ajax({
        url: '/DayBuddy/GetBuddyMessages',
        type: 'get',
        datatype: 'json',
        data: { offset: $("#messagesList").children().length},
        contentype: 'application/json;charset=utf-8',
        success: function (result) {
            if (result.success) {
                var scrollHeightBefore = $MessagesList[0].scrollHeight;
                result.messagesFound.forEach(function (mess) {
                    var $div = $("<div></div>").addClass("p-2 rounded-6 text-white");

                    if (mess.sender === loggedInUsername) {
                        $div.addClass("background-grass-green align-self-start");
                    } else {
                        $div.addClass("bg-secondary align-self-end");
                    }

                    $div.text(mess.message);
                    $MessagesList.prepend($div);
                });

                var scrollHeightAfter = $MessagesList[0].scrollHeight;
                $MessagesList.scrollTop(scrollHeightAfter - scrollHeightBefore);
            } else {
                console.error(result.errors.join(", "));
            }
        },
        error: function () {
            console.error("Failed to load messages");
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
    startTimer();
    ScrollMessageList();
});

connection.on("UnMatched", function () {
    window.location.href = '/DayBuddy/SearchBuddy';
});

connection.start().then(function () {
    $("#sendButton").prop("disabled", false);
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
