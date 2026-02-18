const token = document.querySelector("#FinnhubToken").value;
const socket = new WebSocket(`wss://ws.finnhub.io?token=${token}`);
let stockSymbol = document.getElementById("StockSymbol").value;

socket.addEventListener("open", () => {
  socket.send(JSON.stringify({ type: "subscribe", symbol: stockSymbol }));
});

socket.addEventListener("message", (event) => {
  let parsed = JSON.parse(event.data);

  if (parsed.type === "error") {
    $(".price").text(parsed.msg);
    return;
  }

  if (parsed.data && parsed.data.length > 0) {
    let updatedPrice = parsed.data[0].p;
    $(".price").text(updatedPrice.toFixed(2));
  }
});

window.addEventListener("beforeunload", function () {
  socket.send(JSON.stringify({ type: "unsubscribe", symbol: stockSymbol }));
  socket.close();
});
