/**
 * CampusDeadline — Global JavaScript Application Controller
 * Handles Modals, CSRF, Toast Notifications, Mobile Drawer, and API helpers
 */

// Retrieve CSRF Token from meta tag
function getCSRFToken() {
  const meta = document.querySelector('meta[name="csrf-token"]');
  return meta ? meta.getAttribute('content') : '';
}

// Global Toast Notification System
function showToast(message, type = 'info') {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const toast = document.createElement('div');
  toast.className = `toast ${type}`;
  
  let iconName = 'info';
  if (type === 'success') iconName = 'check-circle';
  if (type === 'error') iconName = 'alert-triangle';

  toast.innerHTML = `
    <i data-lucide="${iconName}" style="width: 18px; height: 18px; flex-shrink: 0;"></i>
    <span style="flex: 1;">${message}</span>
    <button type="button" class="btn-ghost" style="padding: 2px; border: none; cursor: pointer; color: var(--text-muted);" aria-label="Close">
      <i data-lucide="x" style="width: 14px; height: 14px;"></i>
    </button>
  `;

  const closeBtn = toast.querySelector('button');
  closeBtn.addEventListener('click', () => toast.remove());

  container.appendChild(toast);
  if (window.lucide) lucide.createIcons({ root: toast });

  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateY(10px)';
    toast.style.transition = 'all 0.3s ease';
    setTimeout(() => toast.remove(), 300);
  }, 4000);
}

// Authenticated AJAX Fetch Wrapper
async function apiFetch(url, options = {}) {
  const headers = options.headers || {};
  headers['X-CSRFToken'] = getCSRFToken();
  headers['X-Requested-With'] = 'XMLHttpRequest';
  if (!headers['Content-Type'] && !(options.body instanceof FormData)) {
    headers['Content-Type'] = 'application/json';
  }

  try {
    const response = await fetch(url, { ...options, headers });
    const data = await response.json().catch(() => ({}));

    if (!response.ok) {
      const errMsg = data.error || `Request failed with status ${response.status}`;
      showToast(errMsg, 'error');
      throw new Error(errMsg);
    }
    return data;
  } catch (err) {
    console.error('API Error:', err);
    throw err;
  }
}

/* ===================================================================
   TASK MODAL CONTROLLER
   =================================================================== */

function openCreateTaskModal(defaultDate = null) {
  const modal = document.getElementById('task-modal');
  const form = document.getElementById('task-modal-form');
  const title = document.getElementById('task-modal-title');
  const taskIdInput = document.getElementById('modal-task-id');
  const submitBtnText = document.getElementById('modal-submit-btn-text');

  if (!modal || !form) return;

  // Reset form
  form.reset();
  form.action = '/tasks/create';
  taskIdInput.value = '';
  title.textContent = 'New Academic Task';
  submitBtnText.textContent = 'Save Task';

  // Default dates
  const now = new Date();
  if (defaultDate) {
    document.getElementById('modal-task-date').value = defaultDate;
  } else {
    // Default to tomorrow
    const tomorrow = new Date(now);
    tomorrow.setDate(tomorrow.getDate() + 1);
    document.getElementById('modal-task-date').value = tomorrow.toISOString().split('T')[0];
  }
  document.getElementById('modal-task-time').value = '23:59';

  modal.classList.add('active');
  document.getElementById('modal-task-title').focus();
}

async function openEditTaskModal(taskId) {
  const modal = document.getElementById('task-modal');
  const form = document.getElementById('task-modal-form');
  const title = document.getElementById('task-modal-title');
  const taskIdInput = document.getElementById('modal-task-id');
  const submitBtnText = document.getElementById('modal-submit-btn-text');

  if (!modal || !form) return;

  try {
    const res = await apiFetch(`/tasks/${taskId}`, { method: 'GET' });
    if (!res.success || !res.task) return;

    const t = res.task;
    form.reset();
    form.action = `/tasks/${taskId}/edit`;
    taskIdInput.value = t.id;
    title.textContent = 'Edit Academic Task';
    submitBtnText.textContent = 'Update Task';

    document.getElementById('modal-task-title').value = t.title;
    document.getElementById('modal-task-subject').value = t.subject_id;
    document.getElementById('modal-task-type').value = t.task_type;
    document.getElementById('modal-task-priority').value = t.priority;
    document.getElementById('modal-task-status').value = t.status;
    document.getElementById('modal-task-desc').value = t.description || '';

    // Split date & time
    if (t.due_date_raw) {
      document.getElementById('modal-task-date').value = t.due_date_raw;
    }
    if (t.due_time_raw) {
      document.getElementById('modal-task-time').value = t.due_time_raw;
    }

    modal.classList.add('active');
  } catch (e) {
    showToast('Failed to load task details.', 'error');
  }
}

function closeTaskModal() {
  const modal = document.getElementById('task-modal');
  if (modal) modal.classList.remove('active');
}

// Handle Task Modal Form Submit (combining date & time)
document.addEventListener('DOMContentLoaded', () => {
  const taskForm = document.getElementById('task-modal-form');
  if (taskForm) {
    taskForm.addEventListener('submit', (e) => {
      const dateVal = document.getElementById('modal-task-date').value;
      const timeVal = document.getElementById('modal-task-time').value || '23:59';
      if (dateVal) {
        document.getElementById('modal-task-combined-due-date').value = `${dateVal} ${timeVal}:00`;
      }
    });
  }
});

/* ===================================================================
   SUBJECT MODAL CONTROLLER
   =================================================================== */

function openCreateSubjectModal() {
  const modal = document.getElementById('subject-modal');
  const form = document.getElementById('subject-modal-form');
  const title = document.getElementById('subject-modal-title');
  const subjectIdInput = document.getElementById('modal-subject-id');
  const submitText = document.getElementById('modal-subject-submit-text');

  if (!modal || !form) return;

  form.reset();
  form.action = '/subjects/create';
  subjectIdInput.value = '';
  title.textContent = 'Add Academic Subject';
  submitText.textContent = 'Save Subject';
  document.getElementById('modal-subject-color').value = '#B98AFF';

  modal.classList.add('active');
  document.getElementById('modal-subject-name').focus();
}

async function openEditSubjectModal(subjectId) {
  const modal = document.getElementById('subject-modal');
  const form = document.getElementById('subject-modal-form');
  const title = document.getElementById('subject-modal-title');
  const subjectIdInput = document.getElementById('modal-subject-id');
  const submitText = document.getElementById('modal-subject-submit-text');

  if (!modal || !form) return;

  try {
    const res = await apiFetch(`/subjects/${subjectId}`, { method: 'GET' });
    if (!res.success || !res.subject) return;

    const s = res.subject;
    form.reset();
    form.action = `/subjects/${subjectId}/edit`;
    subjectIdInput.value = s.id;
    title.textContent = 'Edit Academic Subject';
    submitText.textContent = 'Update Subject';

    document.getElementById('modal-subject-name').value = s.subject_name;
    document.getElementById('modal-subject-code').value = s.subject_code || '';
    document.getElementById('modal-subject-sem').value = s.semester;
    document.getElementById('modal-subject-color').value = s.color || '#B98AFF';

    modal.classList.add('active');
  } catch (e) {
    showToast('Failed to load subject details.', 'error');
  }
}

function closeSubjectModal() {
  const modal = document.getElementById('subject-modal');
  if (modal) modal.classList.remove('active');
}

// Color Swatch buttons in subject modal
document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('.btn-color-swatch').forEach(btn => {
    btn.addEventListener('click', () => {
      const color = btn.getAttribute('data-color');
      const input = document.getElementById('modal-subject-color');
      if (input) input.value = color;
    });
  });
});

/* ===================================================================
   GENERIC CONFIRMATION DIALOG MODAL
   =================================================================== */

let confirmCallback = null;

function showConfirmDialog(title, message, onConfirm) {
  const modal = document.getElementById('confirm-modal');
  if (!modal) {
    if (confirm(message)) onConfirm();
    return;
  }

  document.getElementById('confirm-modal-title').textContent = title || 'Confirm Action';
  document.getElementById('confirm-modal-message').innerHTML = message;
  confirmCallback = onConfirm;

  modal.classList.add('active');
}

function closeConfirmModal() {
  const modal = document.getElementById('confirm-modal');
  if (modal) modal.classList.remove('active');
  confirmCallback = null;
}

document.addEventListener('DOMContentLoaded', () => {
  const btnConfirm = document.getElementById('confirm-modal-submit-btn');
  if (btnConfirm) {
    btnConfirm.addEventListener('click', () => {
      if (confirmCallback) confirmCallback();
      closeConfirmModal();
    });
  }
});

/* ===================================================================
   MOBILE NAVIGATION DRAWER
   =================================================================== */

document.addEventListener('DOMContentLoaded', () => {
  const toggleBtn = document.getElementById('btn-toggle-mobile-menu');
  const closeBtn = document.getElementById('btn-close-mobile-drawer');
  const backdrop = document.getElementById('mobile-drawer-backdrop');
  const panel = document.getElementById('mobile-drawer-panel');

  function openDrawer() {
    backdrop?.classList.add('active');
    panel?.classList.add('active');
  }

  function closeDrawer() {
    backdrop?.classList.remove('active');
    panel?.classList.remove('active');
  }

  toggleBtn?.addEventListener('click', openDrawer);
  closeBtn?.addEventListener('click', closeDrawer);
  backdrop?.addEventListener('click', closeDrawer);
  panel?.querySelectorAll('.nav-link').forEach(link => {
    link.addEventListener('click', closeDrawer);
  });

  // Close modals on Escape key
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      closeTaskModal();
      closeSubjectModal();
      closeConfirmModal();
      closeDrawer();
    }
  });

  // Password Visibility Toggle
  document.querySelectorAll('.btn-toggle-pwd').forEach(btn => {
    btn.addEventListener('click', () => {
      const targetId = btn.getAttribute('data-target');
      const input = document.getElementById(targetId);
      if (!input) return;

      if (input.type === 'password') {
        input.type = 'text';
        btn.innerHTML = '<i data-lucide="eye-off"></i>';
      } else {
        input.type = 'password';
        btn.innerHTML = '<i data-lucide="eye"></i>';
      }
      if (window.lucide) lucide.createIcons({ root: btn });
    });
  });
});

/* ===================================================================
   QUICK TASK STATUS TOGGLER
   =================================================================== */

async function toggleTaskStatus(taskId, newStatus) {
  try {
    const res = await apiFetch(`/tasks/${taskId}/status`, {
      method: 'POST',
      body: JSON.stringify({ status: newStatus })
    });

    if (res.success) {
      showToast(res.message, 'success');
      // If on dashboard, update row
      const row = document.getElementById(`dash-task-row-${taskId}`);
      if (row) {
        if (newStatus === 'completed') {
          row.classList.add('completed');
          setTimeout(() => row.style.opacity = '0.4', 300);
        } else {
          row.classList.remove('completed');
          row.style.opacity = '1';
        }
      }

      // If on tasks page, reload or update
      const taskCard = document.getElementById(`task-card-${taskId}`);
      if (taskCard) {
        if (newStatus === 'completed') {
          taskCard.classList.add('completed');
        } else {
          taskCard.classList.remove('completed');
        }
      }

      // Re-fetch or reload after brief delay if needed
      setTimeout(() => window.location.reload(), 600);
    }
  } catch (err) {
    showToast('Failed to update task status.', 'error');
  }
}
