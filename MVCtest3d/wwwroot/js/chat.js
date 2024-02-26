"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

document.getElementById("MessageSendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    li.textContent = `${user} says ${message}`;
});

connection.start().then(function () {
    document.getElementById("MessageSendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("MessageSendButton").addEventListener("click", function (event) {
    var user = document.getElementById("MessageSender").value;
    var message = document.getElementById("MessageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});