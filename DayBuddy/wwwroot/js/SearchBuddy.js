"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/BuddyHub").build();

connection.on("Matched", function () {
    window.location.href = '/DayBuddy/BuddyChat';
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
