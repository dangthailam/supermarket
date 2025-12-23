import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { createClient, SupabaseClient, User, Session } from '@supabase/supabase-js';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private supabase: SupabaseClient;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private sessionSubject = new BehaviorSubject<Session | null>(null);

  public currentUser$: Observable<User | null> = this.currentUserSubject.asObservable();
  public session$: Observable<Session | null> = this.sessionSubject.asObservable();

  constructor(private router: Router) {
    this.supabase = createClient(
      environment.supabase.url,
      environment.supabase.anonKey
    );

    // Initialize auth state
    this.initializeAuthState();
  }

  private async initializeAuthState(): Promise<void> {
    // Get initial session
    const { data: { session } } = await this.supabase.auth.getSession();
    this.sessionSubject.next(session);
    this.currentUserSubject.next(session?.user ?? null);

    // Listen for auth changes
    this.supabase.auth.onAuthStateChange((event, session) => {
      console.log('Auth state changed:', event);
      this.sessionSubject.next(session);
      this.currentUserSubject.next(session?.user ?? null);

      if (event === 'SIGNED_IN') {
        this.router.navigate(['/']);
      } else if (event === 'SIGNED_OUT') {
        this.router.navigate(['/login']);
      }
    });
  }

  /**
   * Sign in with email and password
   */
  async signIn(email: string, password: string): Promise<{ error: any }> {
    const { error } = await this.supabase.auth.signInWithPassword({
      email,
      password
    });
    return { error };
  }

  /**
   * Sign up with email and password
   */
  async signUp(email: string, password: string, metadata?: any): Promise<{ error: any }> {
    const { error } = await this.supabase.auth.signUp({
      email,
      password,
      options: {
        data: metadata
      }
    });
    return { error };
  }

  /**
   * Sign in with OAuth provider (Google, GitHub, etc.)
   */
  async signInWithOAuth(provider: 'google' | 'github' | 'azure'): Promise<{ error: any }> {
    const { error } = await this.supabase.auth.signInWithOAuth({
      provider,
      options: {
        redirectTo: `${window.location.origin}/auth/callback`
      }
    });
    return { error };
  }

  /**
   * Sign out
   */
  async signOut(): Promise<void> {
    await this.supabase.auth.signOut();
  }

  /**
   * Get current session
   */
  async getSession(): Promise<Session | null> {
    const { data: { session } } = await this.supabase.auth.getSession();
    return session;
  }

  /**
   * Get access token for API calls
   */
  async getAccessToken(): Promise<string | null> {
    const session = await this.getSession();
    return session?.access_token ?? null;
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.currentUserSubject.value !== null;
  }

  /**
   * Get current user
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Reset password
   */
  async resetPassword(email: string): Promise<{ error: any }> {
    const { error } = await this.supabase.auth.resetPasswordForEmail(email, {
      redirectTo: `${window.location.origin}/auth/reset-password`
    });
    return { error };
  }

  /**
   * Update password
   */
  async updatePassword(newPassword: string): Promise<{ error: any }> {
    const { error } = await this.supabase.auth.updateUser({
      password: newPassword
    });
    return { error };
  }
}
