"use strict";

const loggedInUsername = document.getElementById("loggedInUsername").value;
const MessageList = document.getElementById("messagesList");
var connection = new signalR.HubConnectionBuilder().withUrl("/BuddyHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    li.classList.add("p-2", "mb-2", "rounded-6", "text-white");
    
    if (user === loggedInUsername) {
        li.classList.add("bg-secondary");
        li.style.alignSelf = "flex-start";
    } else {
        li.classList.add("background-grass-green");
        li.style.alignSelf = "flex-end";
    }

    li.textContent = `${message}`;
    MessageList.appendChild(li);
});
connection.on("UnMatched", function () {
    window.location.href = '/DayBuddy/SearchBuddy';
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput");
    if (message.value == "") return;
    connection.invoke("SendMessage", message.value).catch(function (err) {
        return console.error(err.toString());
    });
    message.value = "";
    event.preventDefault();
});