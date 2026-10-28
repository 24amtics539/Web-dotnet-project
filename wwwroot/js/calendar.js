/**
 * CampusDeadline — Academic Calendar Controller
 * FullCalendar integration with dynamic event fetching, filtering, and click-to-create
 */

let calendarInstance = null;

document.addEventListener('DOMContentLoaded', () => {
  const calendarEl = document.getElementById('calendar-root');
  if (!calendarEl || typeof FullCalendar === 'undefined') return;

  calendarInstance = new FullCalendar.Calendar(calendarEl, {
    initialView: 'dayGridMonth',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay'
    },
    buttonText: {
      today: 'Today',
      month: 'Month',
      week: 'Week',
      day: 'Day'
    },
    navLinks: true,
    editable: false,
    dayMaxEvents: 3,
    events: function(info, successCallback, failureCallback) {
      const subject = document.getElementById('calendar-filter-subject')?.value || '';
      const status = document.getElementById('calendar-filter-status')?.value || '';

      const params = new URLSearchParams();
      if (subject) params.append('subject_id', subject);
      if (status) params.append('status', status);

      fetch(`/api/calendar/events?${params.toString()}`, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
      })
      .then(res => res.json())
      .then(events => {
        successCallback(events);
      })
      .catch(err => {
        console.error('Error fetching calendar events:', err);
        failureCallback(err);
      });
    },

    // Click date to open Create Task modal with selected date
    dateClick: function(info) {
      openCreateTaskModal(info.dateStr);
    },

    // Click event to view/edit task
    eventClick: function(info) {
      const taskId = info.event.extendedProps.id;
      if (taskId) {
        openEditTaskModal(taskId);
      }
    },

    eventDidMount: function(info) {
      const props = info.event.extendedProps;
      info.el.setAttribute('title', `${props.raw_title} (${props.task_type})\nDue: ${props.due_date_formatted}\nStatus: ${props.status}`);
    }
  });

  calendarInstance.render();
});

function refreshCalendarEvents() {
  if (calendarInstance) {
    calendarInstance.refetchEvents();
  }
}
