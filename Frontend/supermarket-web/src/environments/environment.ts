export const environment = {
  production: false,
  apiUrl: 'https://localhost:7268',
  supabase: {
    url: 'YOUR_SUPABASE_PROJECT_URL', // e.g., https://xxxxx.supabase.co
    anonKey: 'YOUR_SUPABASE_ANON_KEY' // From Supabase Dashboard -> Settings -> API
  },
  sentryDsn: '' // Add your Sentry DSN here for development
};
