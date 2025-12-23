export const environment = {
  production: false,
  apiUrl: 'https://localhost:7268',
  supabase: {
    url: '${SUPABASE_URL}', // Set via Railway environment variable
    anonKey: '${SUPABASE_ANON_KEY}' // Set via Railway environment variable
  }
};
