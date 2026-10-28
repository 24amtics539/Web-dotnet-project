/**
 * CampusDeadline — My Subjects Page Controller
 */

function confirmDeleteSubject(subjectId, subjectName, taskCount) {
  let warningExtra = '';
  if (taskCount > 0) {
    warningExtra = `<br><br><span style="color: #f87171; font-weight: 600;">⚠️ Warning: This subject currently has ${taskCount} academic task(s) assigned to it. Deleting this subject will permanently delete all associated tasks!</span>`;
  }

  showConfirmDialog(
    'Delete Subject',
    `Are you sure you want to delete the course <strong>"${subjectName}"</strong>?${warningExtra}`,
    async () => {
      try {
        const res = await apiFetch(`/subjects/${subjectId}/delete`, {
          method: 'POST'
        });
        if (res.success) {
          showToast('Subject and tasks deleted successfully.', 'info');
          const card = document.getElementById(`subject-card-${subjectId}`);
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
        showToast('Failed to delete subject.', 'error');
      }
    }
  );
}
