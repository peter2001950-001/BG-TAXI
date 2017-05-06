var requestsByDateChart;
var carsByDateChart;
function monthChanged() {
    var requestsByDateChartCanvas = document.getElementById("requestsbyDays");
    var carsByDateChartCanvas = document.getElementById("carsbyDays");
    $.ajax({
        url: "/Profile/GetReport?month=" + $("#month").val(),
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        success: function (data) {
            try {

                requestsByDateChart.destroy();
                carsByDateChart.destroy();
            } catch (e) {
            }
            var requestsByDateLabels = [];
            var requestsByDateValues = [];
            for (var i in data.requestsByDate) {
                requestsByDateLabels[i] = data.requestsByDate[i].day;
            }
            for (var i in data.requestsByDate) {
                requestsByDateValues[i] = data.requestsByDate[i].value;
            }
            var carsByDateLabels = [];
            var carsByDateValues = [];
            for (var i in data.requestsByDate) {
                carsByDateLabels[i] = data.carsByDate[i].day;
            }
            for (var i in data.requestsByDate) {
                carsByDateValues[i] = data.carsByDate[i].value;
            }
            requestsByDateChart = new Chart(requestsByDateChartCanvas, {
                type: "line",
                data: {
                    labels: requestsByDateLabels,
                    datasets: [
                        {
                            label: "Брой постъпили заявки",
                            fill: false,
                            lineTension: 0.2,
                            backgroundColor: "rgba(75,192,192,0.4)",
                            borderColor: "rgba(75,192,192,1)",
                            borderCapStyle: 'butt',
                            borderDash: [],
                            borderDashOffset: 0.0,
                            borderJoinStyle: 'miter',
                            pointBorderColor: "rgba(75,192,192,1)",
                            pointBackgroundColor: "#fff",
                            pointBorderWidth: 1,
                            pointHoverRadius: 5,
                            pointHoverBackgroundColor: "rgba(75,192,192,1)",
                            pointHoverBorderColor: "rgba(220,220,220,1)",
                            pointHoverBorderWidth: 2,
                            pointRadius: 1,
                            pointHitRadius: 10,
                            data: requestsByDateValues,
                            spanGaps: false,
                        }
                    ]
                },
                options: {
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: false
                            }
                        }]
                    },
                    padding: { top: 5 }
                }

            });

            carsByDateChart = new Chart(carsByDateChartCanvas, {
                type: "line",
                data: {
                    labels: carsByDateLabels,
                    datasets: [
                        {
                            label: "Брой работили автомобили",
                            fill: false,
                            lineTension: 0.2,
                            backgroundColor: "rgba(75,192,192,0.4)",
                            borderColor: "rgba(75,192,192,1)",
                            borderCapStyle: 'butt',
                            borderDash: [],
                            borderDashOffset: 0.0,
                            borderJoinStyle: 'miter',
                            pointBorderColor: "rgba(75,192,192,1)",
                            pointBackgroundColor: "#fff",
                            pointBorderWidth: 1,
                            pointHoverRadius: 5,
                            pointHoverBackgroundColor: "rgba(75,192,192,1)",
                            pointHoverBorderColor: "rgba(220,220,220,1)",
                            pointHoverBorderWidth: 2,
                            pointRadius: 1,
                            pointHitRadius: 10,
                            data: carsByDateValues,
                            spanGaps: false,
                        }
                    ]
                },
                options: {
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: false
                            }
                        }]
                    },
                    padding: { top: 5 }
                }

            })
        },
        error: function (error) {

        }
    });
}