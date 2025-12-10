import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, map } from 'rxjs';

/**
 * Service to manage global HTTP request loading state.
 * Used to display a progress bar for all HTTP requests.
 */
@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private readonly requestCount = new BehaviorSubject<number>(0);

  constructor() {}

  /**
   * Increment the request count when a request starts.
   */
  start(): void {
    this.requestCount.next(this.requestCount.value + 1);
  }

  /**
   * Decrement the request count when a request completes.
   */
  stop(): void {
    const count = Math.max(0, this.requestCount.value - 1);
    this.requestCount.next(count);
  }

  /**
   * Get the current loading state as an observable.
   * Returns true if there are any pending requests.
   */
  getLoading(): Observable<boolean> {
    return this.requestCount.asObservable().pipe(
      map(count => count > 0)
    );
  }

  /**
   * Check if there are any pending requests.
   */
  isPending(): boolean {
    return this.requestCount.value > 0;
  }
}
