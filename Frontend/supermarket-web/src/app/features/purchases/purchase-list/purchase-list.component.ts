import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SuperMarketApiClient, PurchaseDto } from '../../../core/api/api-client';
import { Table, TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Tag } from 'primeng/tag';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { Toast } from 'primeng/toast';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    Button,
    Card,
    Tag,
    ConfirmDialog,
    Toast
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './purchase-list.component.html',
  styleUrl: './purchase-list.component.scss'
})
export class PurchaseListComponent implements OnInit {
  purchases: PurchaseDto[] = [];
  loading = false;
  error = '';

  constructor(
    private apiClient: SuperMarketApiClient,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadPurchases();
  }

  loadPurchases(): void {
    this.loading = true;
    this.error = '';
    
    this.apiClient.purchasesAll().subscribe({
      next: (purchases) => {
        this.purchases = purchases;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading purchases:', err);
        this.error = 'Không thể tải danh sách phiếu nhập';
        this.loading = false;
      }
    });
  }

  getStatusSeverity(status: number): 'success' | 'warn' | 'danger' | 'info' {
    switch (status) {
      case 1: return 'warn';    // Pending
      case 2: return 'success'; // Paid
      case 3: return 'danger';  // Cancelled
      default: return 'info';
    }
  }

  getStatusText(status: number): string {
    switch (status) {
      case 1: return 'Chờ xử lý';
      case 2: return 'Đã thanh toán';
      case 3: return 'Đã hủy';
      default: return 'Không xác định';
    }
  }

  onCreateNew(): void {
    this.router.navigate(['/purchases/new']);
  }

  onView(id: string): void {
    this.router.navigate(['/purchases', id]);
  }

  onEdit(id: string): void {
    this.router.navigate(['/purchases/edit', id]);
  }

  onDelete(purchase: PurchaseDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa phiếu nhập ${purchase.code}?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: 'Hủy',
      accept: () => {
        this.apiClient.purchasesDELETE(purchase.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa phiếu nhập'
            });
            this.loadPurchases();
          },
          error: (err) => {
            console.error('Error deleting purchase:', err);
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: 'Không thể xóa phiếu nhập'
            });
          }
        });
      }
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0
    }).format(value);
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('vi-VN');
  }
}
