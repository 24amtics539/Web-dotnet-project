/**
 * CampusDeadline — My Tasks Page Controller
 */

function confirmDeleteTask(taskId, taskTitle) {
  showConfirmDialog(
    'Delete Task',
    `Are you sure you want to delete <strong>"${taskTitle}"</strong>?<br><br>This will permanently remove the task and its status history from your database.`,
    async () => {
      try {
        const res = await apiFetch(`/tasks/${taskId}/delete`, {
          method: 'POST'
        });
        if (res.success) {
          showToast('Task deleted successfully.', 'info');
          const card = document.getElementById(`task-card-${taskId}`);
          if (card) {
            card.style.opacity = '0';
            card.style.transform = 'scale(0.95)';
            card.style.transition = 'all 0.25s ease';
            setTimeout(() => card.remove(), 250);
          } else {
            window.location.reload();
          }
        }
      } catch (err) {
        showToast('Failed to delete task.', 'error');
      }
    }
  );
}

async function updateTaskStatusDirect(taskId, newStatus) {
  try {
    const res = await apiFetch(`/tasks/${taskId}/status`, {
      method: 'POST',
      body: JSON.stringify({ status: newStatus })
    });
    if (res.success) {
      showToast(res.message, 'success');
      const card = document.getElementById(`task-card-${taskId}`);
      if (card) {
        if (newStatus === 'completed') {
          card.classList.add('completed');
        } else {
          card.classList.remove('completed');
        }
      }
    }
  } catch (err) {
    showToast('Failed to update status.', 'error');
  }
}

// Live search debouncing on typing
document.addEventListener('DOMContentLoaded', () => {
  const searchInput = document.getElementById('task-search-input');
  const form = document.getElementById('tasks-filter-form');
  let debounceTimeout = null;

  if (searchInput && form) {
    searchInput.addEventListener('input', () => {
      clearTimeout(debounceTimeout);
      debounceTimeout = setTimeout(() => {
        form.submit();
      }, 550);
    });
  }
});
