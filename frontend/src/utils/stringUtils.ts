/**
 * Truncate a string to a specified length and add ellipsis if needed
 */
export const truncateText = (
  text: string,
  maxLength: number,
  addEllipsis = true,
): string => {
  if (!text || text.length <= maxLength) {
    return text;
  }

  // Find a good breakpoint (space, comma, period) near the max length
  const breakpoint = text.lastIndexOf(' ', maxLength);

  // If no good breakpoint found, just use the max length
  const truncated =
    breakpoint > maxLength / 2
      ? text.substring(0, breakpoint)
      : text.substring(0, maxLength);

  return addEllipsis ? `${truncated}...` : truncated;
};

/**
 * Capitalize the first letter of a string
 */
export const capitalizeFirstLetter = (text: string): string => {
  if (!text || typeof text !== 'string' || text.length === 0) {
    return text;
  }

  return text.charAt(0).toUpperCase() + text.slice(1);
};

/**
 * Remove HTML tags from a string
 */
export const stripHtml = (html: string): string => {
  if (!html || typeof html !== 'string') {
    return '';
  }

  return html.replace(/<[^>]*>/g, '');
};

/**
 * Format a number with commas as thousands separators
 */
export const formatNumber = (num: number): string => {
  return new Intl.NumberFormat('en-US').format(num);
};

/**
 * Generate a slug from a string (e.g., "Hello World" -> "hello-world")
 */
export const slugify = (text: string): string => {
  return text
    .toString()
    .toLowerCase()
    .trim()
    .replace(/\s+/g, '-') // Replace spaces with -
    .replace(/&/g, '-and-') // Replace & with 'and'
    .replace(/[^\w-]+/g, '') // Remove all non-word characters
    .replace(/--+/g, '-'); // Replace multiple - with single -
};
