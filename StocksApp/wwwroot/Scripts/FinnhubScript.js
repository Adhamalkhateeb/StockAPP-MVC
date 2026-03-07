const token = document.querySelector("#FinnhubToken").value;
const socket = new WebSocket(`wss://ws.finnhub.io?token=${token}`);
let stockSymbol = document.getElementById("StockSymbol").value;
// Connection opened. Subscribe to a symbol
socket.addEventListener("open", function (event) {
  socket.send(JSON.stringify({ type: "subscribe", symbol: stockSymbol }));
});

// Listen (ready to receive) for messages
socket.addEventListener("message", function (event) {
  //if error message is received from server
  if (event.data.type == "error") {
    $(".price").text(event.data.msg);
    return; //exit the function
  }

  var eventData = JSON.parse(event.data);
  if (eventData) {
    if (eventData.data) {
      //get the updated price
      var updatedPrice = JSON.parse(event.data).data[0].p;
      var timeStamp = JSON.parse(event.data).data[0].t;
      //console.log(updatedPrice, timeStamp);
      //console.log(new Date(timeStamp).toLocaleTimeString());

      //update the UI
      $(".price").text(updatedPrice.toFixed(2)); //price - big display
      $("#price").val(updatedPrice.toFixed(2)); //price - input hidden
    }
  }
});

// Unsubscribe
var unsubscribe = function (symbol) {
  //disconnect from server
  socket.send(JSON.stringify({ type: "unsubscribe", symbol: symbol }));
};

//when the page is being closed, unsubscribe from the WebSocket
window.onunload = function () {
  unsubscribe(stocksSymbol);
};
