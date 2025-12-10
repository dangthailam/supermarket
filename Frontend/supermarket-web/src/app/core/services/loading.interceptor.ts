import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoadingService } from './loading.service';

/**
 * HTTP Interceptor that manages global loading state.
 * Increments loading counter on request start, decrements on completion.
 * This allows a global progress bar to show/hide automatically.
 */
@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
  constructor(private loadingService: LoadingService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Start loading
    this.loadingService.start();

    return next.handle(request).pipe(
      // Stop loading when the request completes (success or error)
      finalize(() => this.loadingService.stop())
    );
  }
}
