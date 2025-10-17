import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction, CreateTransaction } from '../models/transaction.model';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private apiUrl = 'http://localhost:5000/api/transactions';

  constructor(private http: HttpClient) { }

  getTransactionById(id: number): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.apiUrl}/${id}`);
  }

  getTodaysTransactions(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/today`);
  }

  getTodaysSales(): Observable<{ totalSales: number }> {
    return this.http.get<{ totalSales: number }>(`${this.apiUrl}/today/sales`);
  }

  getTransactionsByDateRange(startDate: Date, endDate: Date): Observable<Transaction[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<Transaction[]>(`${this.apiUrl}/date-range`, { params });
  }

  createTransaction(transaction: CreateTransaction): Observable<Transaction> {
    return this.http.post<Transaction>(this.apiUrl, transaction);
  }

  cancelTransaction(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/cancel`, {});
  }
}
