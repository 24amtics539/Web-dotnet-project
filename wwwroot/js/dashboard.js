/**
 * CampusDeadline — Dashboard Controller
 * Heatmap interactions, Mini Calendar widget, and Chart.js integration
 */

let dashboardChartInstance = null;

// Initialize Dashboard Progress Chart
function initDashboardProgressChart(stats) {
  const canvas = document.getElementById('dashboardProgressChart');
  if (!canvas) return;

  const ctx = canvas.getContext('2d');
  if (dashboardChartInstance) {
    dashboardChartInstance.destroy();
  }

  const total = stats.total || 0;
  if (total === 0) {
    // Empty dataset handling
    ctx.font = '14px Inter, sans-serif';
    ctx.fillStyle = '#79718C';
    ctx.textAlign = 'center';
    ctx.fillText('No academic tasks recorded yet.', canvas.width / 2, canvas.height / 2);
    return;
  }

  dashboardChartInstance = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: ['Completed', 'Pending', 'Overdue'],
      datasets: [{
        label: 'Tasks Count',
        data: [stats.completed, stats.pending, stats.overdue],
        backgroundColor: [
          'rgba(16, 185, 129, 0.85)',  // green
          'rgba(185, 138, 255, 0.85)', // lavender
          'rgba(239, 68, 68, 0.85)'    // red
        ],
        borderColor: [
          '#10B981',
          '#B98AFF',
          '#EF4444'
        ],
        borderWidth: 1.5,
        borderRadius: 8
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          backgroundColor: '#201F29',
          titleColor: '#F1EAFB',
          bodyColor: '#A9A1BC',
          borderColor: 'rgba(185, 138, 255, 0.3)',
          borderWidth: 1,
          padding: 12,
          cornerRadius: 10
        }
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: { color: '#A9A1BC', font: { family: 'Inter', size: 12 } }
        },
        y: {
          beginAtZero: true,
          grid: { color: 'rgba(255, 255, 255, 0.05)' },
          ticks: { color: '#79718C', stepSize: 1, font: { family: 'Inter', size: 11 } }
        }
      }
    }
  });
}

// Update Heatmap Weeks display filter
function updateHeatmapWeeks(weeksToShow) {
  const columns = document.querySelectorAll('.heatmap-column');
  const count = parseInt(weeksToShow, 10);
  const total = columns.length;
  
  columns.forEach((col, idx) => {
    // Show only the rightmost 'count' weeks
    if (idx >= total - count) {
      col.style.display = 'flex';
    } else {
      col.style.display = 'none';
    }
  });
}

/* ===================================================================
   MINI MONTHLY CALENDAR WIDGET (Right Workspace)
   =================================================================== */

let miniCalDate = new Date();

function renderMiniCalendar() {
  const titleEl = document.getElementById('mini-cal-title');
  const gridEl = document.getElementById('mini-cal-days-grid');
  if (!titleEl || !gridEl) return;

  const year = miniCalDate.getFullYear();
  const month = miniCalDate.getMonth();

  const monthNames = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
  ];
  titleEl.textContent = `${monthNames[month]} ${year}`;

  gridEl.innerHTML = '';

  const firstDayIndex = new Date(year, month, 1).getDay(); // 0 is Sun
  // Convert so Monday is 0
  const adjustedFirstDay = (firstDayIndex === 0 ? 6 : firstDayIndex - 1);

  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const daysInPrevMonth = new Date(year, month, 0).getDate();

  const today = new Date();
  const isCurrentMonth = today.getFullYear() === year && today.getMonth() === month;

  // Previous month trailing days
  for (let i = adjustedFirstDay - 1; i >= 0; i--) {
    const dayNum = daysInPrevMonth - i;
    const cell = document.createElement('div');
    cell.className = 'mini-cal-day other-month';
    cell.textContent = dayNum;
    gridEl.appendChild(cell);
  }

  // Current month days
  for (let day = 1; day <= daysInMonth; day++) {
    const cell = document.createElement('div');
    cell.className = 'mini-cal-day';
    cell.textContent = day;

    if (isCurrentMonth && day === today.getDate()) {
      cell.classList.add('today');
    }

    // Format ISO date string
    const mStr = String(month + 1).padStart(2, '0');
    const dStr = String(day).padStart(2, '0');
    const isoDate = `${year}-${mStr}-${dStr}`;
    cell.setAttribute('data-date', isoDate);

    // Clicking date opens create task modal with this date
    cell.addEventListener('click', () => {
      openCreateTaskModal(isoDate);
    });

    gridEl.appendChild(cell);
  }

  // Fetch upcoming task deadlines to mark dots on calendar
  fetchCalendarDots(year, month + 1);
}

async function fetchCalendarDots(year, month) {
  try {
    const res = await fetch('/api/calendar/events', {
      headers: { 'X-Requested-With': 'XMLHttpRequest' }
    });
    if (!res.ok) return;
    const events = await res.json();
    
    events.forEach(ev => {
      if (!ev.start) return;
      const dateKey = ev.start.split('T')[0];
      const cell = document.querySelector(`.mini-cal-day[data-date="${dateKey}"]`);
      if (cell) {
        cell.classList.add('has-deadline');
      }
    });
  } catch (e) {
    // Graceful fallback
  }
}

document.addEventListener('DOMContentLoaded', () => {
  renderMiniCalendar();

  document.getElementById('mini-cal-prev')?.addEventListener('click', () => {
    miniCalDate.setMonth(miniCalDate.getMonth() - 1);
    renderMiniCalendar();
  });

  document.getElementById('mini-cal-next')?.addEventListener('click', () => {
    miniCalDate.setMonth(miniCalDate.getMonth() + 1);
    renderMiniCalendar();
  });
});
