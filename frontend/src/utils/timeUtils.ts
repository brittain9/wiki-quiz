/**
 * Format a date string in a user-friendly format
 */
export const formatDate = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(date);
  } catch (error) {
    console.error('Error formatting date:', error);
    return dateString; // Return original string if formatting fails
  }
};

/**
 * Format a duration in seconds to a human-readable string
 * Example: formatDuration(125) => "2m 5s"
 */
export const formatDuration = (seconds: number): string => {
  if (seconds < 60) {
    return `${seconds}s`;
  }

  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;

  if (minutes < 60) {
    return `${minutes}m ${remainingSeconds}s`;
  }

  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;

  return `${hours}h ${remainingMinutes}m ${remainingSeconds}s`;
};

/**
 * Get a relative time string (e.g., "2 hours ago", "yesterday")
 */
export const getTimeAgo = (dateString: string): string => {
  try {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSec = Math.round(diffMs / 1000);

    // Less than a minute
    if (diffSec < 60) {
      return 'just now';
    }

    // Less than an hour
    if (diffSec < 3600) {
      const minutes = Math.floor(diffSec / 60);
      return `${minutes} minute${minutes === 1 ? '' : 's'} ago`;
    }

    // Less than a day
    if (diffSec < 86400) {
      const hours = Math.floor(diffSec / 3600);
      return `${hours} hour${hours === 1 ? '' : 's'} ago`;
    }

    // Less than a week
    if (diffSec < 604800) {
      const days = Math.floor(diffSec / 86400);

      if (days === 1) {
        return 'yesterday';
      }

      return `${days} days ago`;
    }

    // Just return the formatted date for older times
    return formatDate(dateString);
  } catch (error) {
    console.error('Error calculating time ago:', error);
    return formatDate(dateString);
  }
};
