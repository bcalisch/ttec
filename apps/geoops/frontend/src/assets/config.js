// Runtime configuration - overwritten by Docker entrypoint in production
window.__APP_CONFIG__ = {
  apiBaseUrl: '',  // empty = relative URLs for local dev
  ticketingAppUrl: 'http://localhost:81',  // ticketing frontend URL
};
