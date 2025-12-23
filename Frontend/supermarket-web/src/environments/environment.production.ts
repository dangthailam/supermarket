export const environment = {
  production: true,
  apiUrl: '${API_URL}',
  supabase: {
    url: '${SUPABASE_URL}', // Set via Railway environment variable
    anonKey: '${SUPABASE_ANON_KEY}' // Set via Railway environment variable
  }
};
