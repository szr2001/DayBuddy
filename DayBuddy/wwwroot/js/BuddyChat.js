const loggedInUsername = $("#loggedInUsername").val();
const MessagesList = $("#messagesList");
const BuddyChatTimer = $("#buddyChatTimer");
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
    MessagesList.append(div);
    ScrollMessageList();
});

function ScrollMessageList() {
    MessagesList.scrollTop(MessagesList[0].scrollHeight);
}

$(document).ready(ScrollMessageList);


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
