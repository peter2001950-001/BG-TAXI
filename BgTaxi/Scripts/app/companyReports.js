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
            var acceptedRequestsByDateValues = [];
            for (var i in data.acceptedRequestsByDate) {
                requestsByDateLabels[i] = data.acceptedRequestsByDate[i].day + " Май";
            }
            for (var i in data.acceptedRequestsByDate) {
                acceptedRequestsByDateValues[i] = data.acceptedRequestsByDate[i].value;
            }
            var allRequestsByDateValues = [];
           
            for (var i in data.acceptedRequestsByDate) {
                allRequestsByDateValues[i] = data.allRequestsByDate[i].value;
            }
            
            var carsByDateValues = [];
            for (var i in data.carsByDate) {
                carsByDateValues[i] = data.carsByDate[i].value;
            }
            requestsByDateChart = new Chart(requestsByDateChartCanvas, {
                type: "line",
                data: {
                    labels: requestsByDateLabels,
                    datasets: [
                        {
                            label: "Брой постъпили заявки",
                            fill: true,
                            lineTension: 0.2,
                            fillColor: "rgba(220,220,220,0.2)",
                            backgroundColor: "rgba(220,220,220,0.4)",
                            borderColor: "rgba(200,200,200,1)",
                            borderCapStyle: 'butt',
                            borderDash: [],
                            borderDashOffset: 0.0,
                            borderJoinStyle: 'miter',
                            pointBorderColor: "rgba(220,220,220,1)",
                            pointBackgroundColor: "#fff",
                            pointBorderWidth: 1,
                            pointHoverRadius: 3,
                            pointHoverBackgroundColor: "rgba(200,200,200,1)",
                            pointHoverBorderColor: "rgba(180,180,180,1)",
                            pointHoverBorderWidth: 2,
                            pointRadius: 1,
                            pointHitRadius: 10,
                            data: allRequestsByDateValues,
                            spanGaps: false,
                        },
                        {
                            label: "Брой приети заявки",
                            fill: true,
                            lineTension: 0.2,
                            fillColor: "rgba(55, 232, 105,0.2)",
                            backgroundColor: "rgba(55, 232, 105,0.4)",
                            borderColor: "rgba(54, 201, 96,1)",
                            borderCapStyle: 'butt',
                            borderDash: [],
                            borderDashOffset: 0.0,
                            borderJoinStyle: 'miter',
                            pointBorderColor: "rgba(55, 232, 105,1)",
                            pointBackgroundColor: "#fff",
                            pointBorderWidth: 1,
                            pointHoverRadius: 3,
                            pointHoverBackgroundColor: "rgba(54, 201, 96,1)",
                            pointHoverBorderColor: "rgba(57, 168, 88,1)",
                            pointHoverBorderWidth: 2,
                            pointRadius: 1,
                            pointHitRadius: 10,
                            data: acceptedRequestsByDateValues,
                            spanGaps: false,
                        }
                    ]
                },
                options: { 
                           
                    multiTooltipTemplate: "<%= requestsByDateChart.datasets[0].label %> - <%= value %>",
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: false,
                                userCallback: function (label, index, labels) {
                                    if (Math.floor(label) === label) {
                                        return label;
                                    }

                                },
                            }
                        }]
                    },
                    padding: { top: 5 }
                }

            });

            carsByDateChart = new Chart(carsByDateChartCanvas, {
                type: "line",
                data: {
                    labels: requestsByDateLabels,
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
                                beginAtZero: false,
                                userCallback: function (label, index, labels) {
                                    if (Math.floor(label) === label) {
                                        return label;
                                    }

                                },
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