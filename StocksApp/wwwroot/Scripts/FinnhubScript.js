const prices = [];
const labels = [];

const ctx = document.getElementById('stock-chart').getContext('2d');

// Gradient fill
const gradient = ctx.createLinearGradient(0, 0, 0, 400);
gradient.addColorStop(0, 'rgba(10, 138, 198, 0.3)');
gradient.addColorStop(1, 'rgba(10, 138, 198, 0.0)');

const chartData = {
    labels: labels,
    datasets: [{
        label: 'Stock price',
        data: prices,
        fill: true,
        backgroundColor: gradient,
        borderColor: '#0a8ac6',
        borderWidth: 2,
        tension: 0.4,
        pointRadius: 0,           // hide all points
        pointHoverRadius: 5,      // show on hover
        pointHoverBackgroundColor: '#0a8ac6',
        pointHoverBorderColor: '#fff',
        pointHoverBorderWidth: 2,
    }]
};

const chartOptions = {
    animation: false,             // no lag on live updates
    scales: {
        x: {
            ticks: { display: false },
            grid: { display: false },
            border: { display: false }
        },
        y: {
            position: 'right',
            ticks: {
                color: '#888',
                font: { size: 11 },
                callback: (val) => '$' + val.toFixed(2)
            },
            grid: {
                color: 'rgba(255,255,255,0.05)',
                drawBorder: false
            },
            border: { display: false }
        }
    },
    plugins: {
        legend: { display: false },
        tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: '#1a1a2e',
            titleColor: '#888',
            bodyColor: '#fff',
            borderColor: '#0a8ac6',
            borderWidth: 1,
            padding: 10,
            callbacks: {
                label: (ctx) => ' $' + ctx.parsed.y.toFixed(2)
            }
        }
    },
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
        mode: 'nearest',
        axis: 'x',
        intersect: false
    }
};

const chart = new Chart(ctx, { type: 'line', data: chartData, options: chartOptions });

// Live dot plugin — draws a pulsing dot on the last data point
const liveDotPlugin = {
    id: 'liveDot',
    afterDraw(chart) {
        const dataset = chart.data.datasets[0];
        if (!dataset.data.length) return;

        const meta = chart.getDatasetMeta(0);
        const lastPoint = meta.data[meta.data.length - 1];
        if (!lastPoint) return;

        const { x, y } = lastPoint.getProps(['x', 'y'], true);
        const ctx = chart.ctx;

        // Outer glow
        ctx.save();
        ctx.beginPath();
        ctx.arc(x, y, 6, 0, Math.PI * 2);
        ctx.fillStyle = 'rgba(10, 138, 198, 0.25)';
        ctx.fill();

        // Inner dot
        ctx.beginPath();
        ctx.arc(x, y, 3.5, 0, Math.PI * 2);
        ctx.fillStyle = '#0a8ac6';
        ctx.fill();
        ctx.restore();
    }
};

Chart.register(liveDotPlugin);

document.querySelector(".date").innerText = new Date().toLocaleDateString();

const token = document.querySelector("#FinnhubToken").value;
const stockSymbol = document.getElementById("StockSymbol").value;
const updateInterval = 1000;
let lastUpdateTime = 0;

async function loadHistoricalData() {
    try {
        const url = `/api/history/${stockSymbol}`;
        const res = await fetch(url);
        const data = await res.json();
        const result = data.chart.result[0];

        if (!result) return;

        const closes = result.indicators.quote[0].close;

        result.timestamp.forEach((t, i) => {
            const price = closes[i];
            if (price === null || price === undefined) return;
            prices.push(price);
            labels.push(new Date(t * 1000).toLocaleTimeString());
        });

        chart.update();
    } catch (err) {
        console.warn("History fetch failed:", err);
    }
}

function openLiveFeed() {
    const socket = new WebSocket(`wss://ws.finnhub.io?token=${token}`);

    socket.addEventListener('open', () => {
        socket.send(JSON.stringify({ type: 'subscribe', symbol: stockSymbol }));
    });

    socket.addEventListener('message', (event) => {
        const eventData = JSON.parse(event.data);
        if (!eventData.data || eventData.data.length === 0) return;

        const updatedPrice = eventData.data[0].p;
        const timeStamp = eventData.data[0].t;
        const now = Date.now();

        if (prices.length === 0 || now - lastUpdateTime > updateInterval) {
            prices.push(updatedPrice);
            labels.push(new Date(timeStamp).toLocaleTimeString());
            chart.update();
            lastUpdateTime = now;
        }

        $(".price").text(updatedPrice.toFixed(2));
        $("#price").val(updatedPrice.toFixed(2));
    });

    window.addEventListener('beforeunload', () => {
        socket.send(JSON.stringify({ type: 'unsubscribe', symbol: stockSymbol }));
    });
}

loadHistoricalData().then(() => openLiveFeed());