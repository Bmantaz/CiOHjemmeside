let chartInstance = null;

export function renderChart(canvasId, labels, revenueData, itemCounts, dotNetRef) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        return;
    }

    destroyChart();

    const ctx = canvas.getContext('2d');
    chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Omsætning (kr.)',
                    data: revenueData,
                    backgroundColor: 'rgba(75, 192, 192, 0.6)',
                    yAxisID: 'y'
                },
                {
                    label: 'Antal solgt',
                    data: itemCounts,
                    backgroundColor: 'rgba(153, 102, 255, 0.6)',
                    yAxisID: 'y1'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            onClick: (evt, elements) => {
                if (elements.length > 0 && dotNetRef) {
                    const index = elements[0].index;
                    const label = labels[index];
                    dotNetRef.invokeMethodAsync('OnDayClicked', label);
                }
            },
            scales: {
                y: {
                    type: 'linear',
                    position: 'left',
                    beginAtZero: true
                },
                y1: {
                    type: 'linear',
                    position: 'right',
                    beginAtZero: true,
                    grid: {
                        drawOnChartArea: false
                    }
                }
            }
        }
    });
}

export function destroyChart() {
    if (chartInstance) {
        chartInstance.destroy();
        chartInstance = null;
    }
}
