import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  template: `
    <div class="callback-container">
      <div class="loading-spinner">
        <i class="pi pi-spin pi-spinner" style="font-size: 3rem"></i>
        <p>Đang xác thực...</p>
      </div>
    </div>
  `,
  styles: [`
    .callback-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: #f5f5f5;
    }

    .loading-spinner {
      text-align: center;
      color: #666;

      p {
        margin-top: 1rem;
        font-size: 1rem;
      }
    }
  `]
})
export class AuthCallbackComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    // The AuthService will automatically handle the auth state change
    // and redirect to the home page
    const session = await this.authService.getSession();
    
    if (session) {
      this.router.navigate(['/']);
    } else {
      // If no session, redirect back to login
      this.router.navigate(['/login']);
    }
  }
}
