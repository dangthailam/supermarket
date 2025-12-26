export const environment = {
  production: false,
  apiUrl: 'https://localhost:7268',
  supabase: {
    url: 'https://uyuhtvwmhhybtmwpuiiw.supabase.co', // e.g., https://xxxxx.supabase.co
    anonKey: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InV5dWh0dndtaGh5YnRtd3B1aWl3Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjQ0ODg3NTcsImV4cCI6MjA4MDA2NDc1N30.bxgNv6hFrAz-Yy9n6a7di2l4F8vnuSKlpaj88RF6iik' // From Supabase Dashboard -> Settings -> API
  },
  sentryDsn: '' // Add your Sentry DSN here for development
};
