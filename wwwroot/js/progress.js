/**
 * CampusDeadline — Progress & Analytics Controller
 * 4 Chart.js charts, dynamic period filter fetching, and insights renderer
 */

let chartStatusInstance = null;
let chartSubjectInstance = null;
let chartTrendInstance = null;
let chartTypeInstance = null;

function initProgressCharts(analytics) {
  const charts = analytics.charts;
  if (!charts) return;

  // 1. Task Status Distribution (Doughnut)
  const ctxStatus = document.getElementById('chartStatusDist')?.getContext('2d');
  if (ctxStatus) {
    if (chartStatusInstance) chartStatusInstance.destroy();
    chartStatusInstance = new Chart(ctxStatus, {
      type: 'doughnut',
      data: {
        labels: charts.status_distribution.labels,
        datasets: [{
          data: charts.status_distribution.data,
          backgroundColor: [
            '#10B981', // completed
            '#38BDF8', // in_progress
            '#8B5CF6', // pending
            '#EF4444'  // overdue
          ],
          borderColor: '#201F29',
          borderWidth: 3
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            labels: { color: '#A9A1BC', font: { family: 'Inter', size: 12 }, padding: 14 }
          },
          tooltip: {
            backgroundColor: '#201F29',
            titleColor: '#F1EAFB',
            bodyColor: '#A9A1BC',
            borderColor: 'rgba(185, 138, 255, 0.3)',
            borderWidth: 1,
            padding: 10,
            cornerRadius: 8
          }
        },
        cutout: '68%'
      }
    });
  }

  // 2. Tasks by Subject (Grouped / Stacked Bar)
  const ctxSubject = document.getElementById('chartSubjectDist')?.getContext('2d');
  if (ctxSubject) {
    if (chartSubjectInstance) chartSubjectInstance.destroy();
    chartSubjectInstance = new Chart(ctxSubject, {
      type: 'bar',
      data: {
        labels: charts.subject_distribution.labels,
        datasets: [
          {
            label: 'Completed',
            data: charts.subject_distribution.completed,
            backgroundColor: 'rgba(16, 185, 129, 0.85)',
            borderColor: '#10B981',
            borderRadius: 6
          },
          {
            label: 'Incomplete',
            data: charts.subject_distribution.incomplete,
            backgroundColor: 'rgba(185, 138, 255, 0.85)',
            borderColor: '#B98AFF',
            borderRadius: 6
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
            labels: { color: '#A9A1BC', font: { family: 'Inter', size: 11 } }
          },
          tooltip: {
            backgroundColor: '#201F29',
            titleColor: '#F1EAFB',
            bodyColor: '#A9A1BC',
            borderColor: 'rgba(185, 138, 255, 0.3)',
            borderWidth: 1
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: {
              color: '#A9A1BC',
              font: { family: 'Inter', size: 10 },
              maxRotation: 35,
              minRotation: 0
            }
          },
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(255, 255, 255, 0.05)' },
            ticks: { color: '#79718C', stepSize: 1 }
          }
        }
      }
    });
  }

  // 3. Completion Over Time (Line)
  const ctxTrend = document.getElementById('chartTrend')?.getContext('2d');
  if (ctxTrend) {
    if (chartTrendInstance) chartTrendInstance.destroy();
    
    // Gradient fill
    const gradient = ctxTrend.createLinearGradient(0, 0, 0, 240);
    gradient.addColorStop(0, 'rgba(185, 138, 255, 0.35)');
    gradient.addColorStop(1, 'rgba(185, 138, 255, 0.0)');

    chartTrendInstance = new Chart(ctxTrend, {
      type: 'line',
      data: {
        labels: charts.completion_trend.labels,
        datasets: [{
          label: 'Cumulative Finished',
          data: charts.completion_trend.data,
          fill: true,
          backgroundColor: gradient,
          borderColor: '#B98AFF',
          borderWidth: 2.5,
          tension: 0.35,
          pointBackgroundColor: '#B98AFF',
          pointBorderColor: '#201F29',
          pointBorderWidth: 2,
          pointRadius: 4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#201F29',
            titleColor: '#F1EAFB',
            bodyColor: '#A9A1BC',
            borderColor: 'rgba(185, 138, 255, 0.3)',
            borderWidth: 1
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: { color: '#A9A1BC', maxTicksLimit: 7 }
          },
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(255, 255, 255, 0.05)' },
            ticks: { color: '#79718C', stepSize: 1 }
          }
        }
      }
    });
  }

  // 4. Task Type Distribution (Bar)
  const ctxType = document.getElementById('chartTypeDist')?.getContext('2d');
  if (ctxType) {
    if (chartTypeInstance) chartTypeInstance.destroy();
    chartTypeInstance = new Chart(ctxType, {
      type: 'bar',
      data: {
        labels: charts.type_distribution.labels,
        datasets: [{
          label: 'Total Tasks',
          data: charts.type_distribution.data,
          backgroundColor: [
            'rgba(185, 138, 255, 0.85)',
            'rgba(56, 189, 248, 0.85)',
            'rgba(245, 158, 11, 0.85)',
            'rgba(236, 72, 153, 0.85)',
            'rgba(156, 163, 175, 0.85)'
          ],
          borderColor: 'transparent',
          borderRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: '#201F29',
            titleColor: '#F1EAFB',
            bodyColor: '#A9A1BC',
            borderColor: 'rgba(185, 138, 255, 0.3)',
            borderWidth: 1
          }
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: {
              color: '#A9A1BC',
              font: { family: 'Inter', size: 10 },
              maxRotation: 30,
              minRotation: 0
            }
          },
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(255, 255, 255, 0.05)' },
            ticks: { color: '#79718C', stepSize: 1 }
          }
        }
      }
    });
  }
}

// Switch period dynamically via AJAX
async function switchAnalyticsPeriod(period) {
  // Update button active states
  document.querySelectorAll('.quick-filter-pills .quick-pill').forEach(btn => {
    btn.classList.remove('active');
  });
  event?.target?.classList.add('active');

  try {
    const data = await apiFetch(`/api/progress?period=${period}`, { method: 'GET' });
    if (!data || !data.summary) return;

    // Update summary metric cards
    document.getElementById('stat-total-tasks').textContent = data.summary.total_tasks;
    document.getElementById('stat-completed-tasks').textContent = data.summary.completed_tasks;
    document.getElementById('stat-completion-rate').textContent = `${data.summary.completion_rate}% overall rate`;
    document.getElementById('stat-pending-tasks').textContent = data.summary.pending_tasks;
    document.getElementById('stat-overdue-tasks').textContent = data.summary.overdue_tasks;
    document.getElementById('stat-best-subject').textContent = data.summary.best_subject;
    document.getElementById('stat-best-rate').textContent = `${data.summary.best_subject_rate}% completed`;

    // Update Insights container
    const insightsContainer = document.getElementById('insights-container');
    if (insightsContainer && data.insights) {
      insightsContainer.innerHTML = '';
      data.insights.forEach(ins => {
        const item = document.createElement('div');
        item.className = `insight-item ${ins.type}`;
        item.innerHTML = `
          <div class="insight-icon-box">
            <i data-lucide="${ins.icon}"></i>
          </div>
          <div>
            <div class="insight-title">${ins.title}</div>
            <div class="insight-text">${ins.text}</div>
          </div>
        `;
        insightsContainer.appendChild(item);
      });
      if (window.lucide) lucide.createIcons({ root: insightsContainer });
    }

    // Re-render all charts
    initProgressCharts(data);
    showToast(`Updated analytics for period: ${period}`, 'info');
  } catch (e) {
    showToast('Failed to update analytics filter.', 'error');
  }
}
