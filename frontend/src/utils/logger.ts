/**
 * Logger utility for app-wide consistent logging
 */

// Set to true to enable all logs, false to disable all logs, or undefined to use module-specific settings
let GLOBAL_DEBUG_ENABLED: boolean | undefined = undefined;

/**
 * Create a customized logger for a specific module
 * @param module The module name (e.g., 'Auth', 'API', etc.)
 * @param emoji An emoji to use as visual identifier in logs
 * @param moduleDebugEnabled Whether logs should be enabled for this module
 */
export const createLogger = (
  module: string,
  emoji: string = '📝',
  moduleDebugEnabled: boolean = true,
) => {
  // Return the logger function
  return (message: string, data?: unknown) => {
    // Only log if globally enabled (true), not globally disabled (false), or if no global setting (undefined) then use module setting
    if (
      GLOBAL_DEBUG_ENABLED === false ||
      (GLOBAL_DEBUG_ENABLED === undefined && !moduleDebugEnabled)
    ) {
      return;
    }

    console.group(`${emoji} ${module}: ${message}`);
    console.log(`⏰ ${new Date().toISOString()}`);
    if (data) {
      console.log('📄 Data:', data);
    }
    console.groupEnd();
  };
};

// Pre-configured loggers for common modules
export const logAuth = createLogger('Auth', '🔐', true);
export const logAPI = createLogger('API', '🌐', true);
export const logError = createLogger('Error', '❌', true);
export const logNavigation = createLogger('Navigation', '🧭', true);
export const logPerformance = createLogger('Performance', '⚡', true);

/**
 * Enable or disable all logging globally
 */
export const setLoggingEnabled = (enabled: boolean) => {
  GLOBAL_DEBUG_ENABLED = enabled;
  console.log(`🔊 All logging ${enabled ? 'enabled' : 'disabled'}`);
};

/**
 * Helper function to safely stringify objects with circular references
 */
export const safeStringify = (obj: unknown) => {
  const seen = new WeakSet();
  return JSON.stringify(
    obj,
    (key, value) => {
      if (typeof value === 'object' && value !== null) {
        if (seen.has(value)) {
          return '[Circular]';
        }
        seen.add(value);
      }
      return value;
    },
    2,
  );
};

/**
 * Log HTTP request/response details
 */
export const logHttpDetails = (
  type: 'Request' | 'Response',
  url: string,
  options: Record<string, unknown>,
) => {
  logAPI(`HTTP ${type}: ${url}`, {
    url,
    ...options,
    headers: options.headers
      ? Object.fromEntries(
          Object.entries(options.headers as Record<string, string>),
        )
      : undefined,
  });
};

// Export default for convenience
export default {
  logAuth,
  logAPI,
  logError,
  logNavigation,
  logPerformance,
  setLoggingEnabled,
};
