import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

// PrimeNG imports
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    MessageModule,
    DividerModule
  ],
  template: `
    <div class="login-container">
      <p-card class="login-card">
        <ng-template pTemplate="header">
          <div class="login-header">
            <h2>Đăng nhập SuperMarket</h2>
            <p>Vui lòng đăng nhập để tiếp tục</p>
          </div>
        </ng-template>

        @if (errorMessage) {
          <p-message severity="error" [text]="errorMessage" styleClass="w-full mb-3"></p-message>
        }

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-field">
            <label for="email">Email</label>
            <input
              pInputText
              id="email"
              type="email"
              formControlName="email"
              placeholder="email@example.com"
              class="w-full"
              (keydown.enter)="onSubmit()"
            />
            @if (loginForm.get('email')?.invalid && loginForm.get('email')?.touched) {
              <small class="text-red-500">Email không hợp lệ</small>
            }
          </div>

          <div class="form-field">
            <label for="password">Mật khẩu</label>
            <p-password
              formControlName="password"
              [toggleMask]="true"
              [feedback]="false"
              placeholder="Nhập mật khẩu"
              styleClass="w-full"
              inputStyleClass="w-full"
              (keydown.enter)="onSubmit()"
            ></p-password>
            @if (loginForm.get('password')?.invalid && loginForm.get('password')?.touched) {
              <small class="text-red-500">Mật khẩu là bắt buộc</small>
            }
          </div>

          <div class="flex justify-content-between align-items-center mb-3">
            <a href="#" class="text-primary text-sm" (click)="onForgotPassword($event)">
              Quên mật khẩu?
            </a>
          </div>

          <p-button
            type="submit"
            label="Đăng nhập"
            icon="pi pi-sign-in"
            [loading]="loading"
            [disabled]="loginForm.invalid || loading"
            styleClass="w-full"
          ></p-button>
        </form>

        <p-divider align="center">
          <span class="text-sm text-500">HOẶC</span>
        </p-divider>

        <div class="oauth-buttons">
          <p-button
            label="Đăng nhập với Google"
            icon="pi pi-google"
            (onClick)="signInWithGoogle()"
            [outlined]="true"
            styleClass="w-full mb-2"
          ></p-button>
          
          <p-button
            label="Đăng nhập với GitHub"
            icon="pi pi-github"
            (onClick)="signInWithGitHub()"
            [outlined]="true"
            styleClass="w-full"
          ></p-button>
        </div>

        <ng-template pTemplate="footer">
          <div class="text-center">
            <small class="text-500">
              Chưa có tài khoản? 
              <a href="#" class="text-primary" (click)="onSignUp($event)">Đăng ký ngay</a>
            </small>
          </div>
        </ng-template>
      </p-card>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 1rem;
    }

    .login-card {
      width: 100%;
      max-width: 450px;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
    }

    .login-header {
      text-align: center;
      padding: 2rem 1rem 1rem;
    }

    .login-header h2 {
      margin: 0 0 0.5rem;
      color: #333;
      font-size: 1.75rem;
      font-weight: 600;
    }

    .login-header p {
      margin: 0;
      color: #666;
      font-size: 0.95rem;
    }

    .form-field {
      margin-bottom: 1.5rem;
    }

    .form-field label {
      display: block;
      margin-bottom: 0.5rem;
      color: #333;
      font-weight: 500;
      font-size: 0.95rem;
    }

    .oauth-buttons {
      margin-top: 1rem;
    }

    :host ::ng-deep {
      .p-password {
        width: 100%;
      }
      
      .p-password input {
        width: 100%;
      }
    }
  `]
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  errorMessage = '';
  returnUrl = '/';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // Get return url from route parameters or default to '/'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    // If already logged in, redirect
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  async onSubmit(): Promise<void> {
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const { email, password } = this.loginForm.value;
    const { error } = await this.authService.signIn(email, password);

    if (error) {
      this.errorMessage = this.getErrorMessage(error);
      this.loading = false;
    } else {
      // Navigation will be handled by auth state change in AuthService
      this.loading = false;
    }
  }

  async signInWithGoogle(): Promise<void> {
    this.loading = true;
    this.errorMessage = '';

    const { error } = await this.authService.signInWithOAuth('google');

    if (error) {
      this.errorMessage = 'Không thể đăng nhập với Google. Vui lòng thử lại.';
      this.loading = false;
    }
  }

  async signInWithGitHub(): Promise<void> {
    this.loading = true;
    this.errorMessage = '';

    const { error } = await this.authService.signInWithOAuth('github');

    if (error) {
      this.errorMessage = 'Không thể đăng nhập với GitHub. Vui lòng thử lại.';
      this.loading = false;
    }
  }

  onForgotPassword(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/auth/forgot-password']);
  }

  onSignUp(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/auth/signup']);
  }

  private getErrorMessage(error: any): string {
    if (error.message?.includes('Invalid login credentials')) {
      return 'Email hoặc mật khẩu không đúng';
    }
    if (error.message?.includes('Email not confirmed')) {
      return 'Vui lòng xác nhận email trước khi đăng nhập';
    }
    return error.message || 'Có lỗi xảy ra. Vui lòng thử lại.';
  }
}
