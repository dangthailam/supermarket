import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgressBar } from 'primeng/progressbar';
import { LoadingService } from '../../../core/services/loading.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * Global loading progress bar component.
 * Automatically shows/hides based on HTTP request state.
 * Place this component in the root layout (app.component).
 */
@Component({
  selector: 'app-global-progress-bar',
  standalone: true,
  imports: [CommonModule, ProgressBar],
  template: `
    @if (isLoading$ | async) {
      <p-progressBar
        [value]="100"
        [showValue]="false"
        class="global-progress-bar"
        [style]="{ 'height': '3px' }"
      ></p-progressBar>
    }
  `,
  styles: [`
    :host {
      display: block;
    }

    .global-progress-bar {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      z-index: 9999;
      margin: 0;
    }

    ::ng-deep .global-progress-bar .p-progressbar {
      background-color: #e9ecef;
    }

    ::ng-deep .global-progress-bar .p-progressbar-value {
      background: linear-gradient(90deg, #0066cc, #0099ff);
      transition: width 0.3s ease;
    }
  `]
})
export class GlobalProgressBarComponent implements OnInit, OnDestroy {
  isLoading$ = this.loadingService.getLoading();
  private destroy$ = new Subject<void>();

  constructor(private loadingService: LoadingService) {}

  ngOnInit(): void {
    // Optional: You could add additional logic here if needed
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
