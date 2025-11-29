import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SuperMarketApiClient, CustomerDto, CustomerDtoPaginatedResult } from '../../../core/api/api-client';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-customer-list',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    Button,
    InputText,
    Message,
    Card,
    ConfirmDialog
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <div class="p-3">
      <p-card>
        <ng-template pTemplate="header">
          <div class="px-3 pt-3 flex justify-content-between align-items-center">
            <h3>Quản lý khách hàng</h3>
            <p-button label="Thêm khách hàng" icon="pi pi-plus" (onClick)="goToCreate()"></p-button>
          </div>
        </ng-template>

        @if (error) {
          <p-message severity="error" [text]="error" styleClass="mb-3"></p-message>
        }

        <div class="mb-3">
          <input pInputText placeholder="Tìm kiếm theo tên, email, số điện thoại..." 
                 [formControl]="searchControl" class="w-full"/>
        </div>

        @if (loading) {
          <div class="text-center py-5">
            <p>Đang tải dữ liệu...</p>
          </div>
        }

        <table class="w-full border-collapse" [style.display]="loading ? 'none' : 'table'">
          <thead class="bg-surface-100">
            <tr>
              <th class="p-3 text-left border border-surface-300">Tên khách hàng</th>
              <th class="p-3 text-left border border-surface-300">Email</th>
              <th class="p-3 text-left border border-surface-300">Số điện thoại</th>
              <th class="p-3 text-left border border-surface-300">Loại khách hàng</th>
              <th class="p-3 text-left border border-surface-300">Trạng thái</th>
              <th class="p-3 text-center border border-surface-300">Hành động</th>
            </tr>
          </thead>
          <tbody>
            @for (customer of customers; track customer.id) {
              <tr class="hover:bg-surface-50">
                <td class="p-3 border border-surface-300">{{ customer.name }}</td>
                <td class="p-3 border border-surface-300">{{ customer.email }}</td>
                <td class="p-3 border border-surface-300">{{ customer.phone || '-' }}</td>
                <td class="p-3 border border-surface-300">{{ customer.customerType || '-' }}</td>
                <td class="p-3 border border-surface-300">
                  {{ customer.isActive ? 'Hoạt động' : 'Không hoạt động' }}
                </td>
                <td class="p-3 border border-surface-300 text-center">
                  <div class="flex gap-2 justify-content-center">
                    <p-button 
                      icon="pi pi-pencil" 
                      [rounded]="true" 
                      [text]="true"
                      severity="info"
                      (onClick)="goToEdit(customer.id)"
                      title="Sửa">
                    </p-button>
                    <p-button 
                      icon="pi pi-trash" 
                      [rounded]="true" 
                      [text]="true"
                      severity="danger"
                      (onClick)="confirmDelete(customer)"
                      title="Xóa">
                    </p-button>
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>

        @if (customers.length === 0 && !loading) {
          <div class="text-center py-5 text-500">
            <p>Không có khách hàng nào</p>
          </div>
        }

        <div class="flex justify-content-between align-items-center mt-3">
          <span>Tổng: {{ totalRecords }} khách hàng</span>
          <div class="flex gap-2">
            <p-button 
              icon="pi pi-chevron-left" 
              [text]="true"
              (onClick)="previousPage()"
              [disabled]="currentPageNumber === 1">
            </p-button>
            <span class="p-2">Trang {{ currentPageNumber }}</span>
            <p-button 
              icon="pi pi-chevron-right" 
              [text]="true"
              (onClick)="nextPage()"
              [disabled]="(currentPageNumber * pageSize) >= totalRecords">
            </p-button>
          </div>
        </div>
      </p-card>
    </div>

    <p-confirmDialog
      [style]="{width: '50vw'}"
      [breakpoints]="{'960px': '75vw', '640px': '90vw'}"
      header="Xác nhận"
      icon="pi pi-exclamation-triangle"
      [modal]="true">
      <ng-template pTemplate="message">
        <span>Bạn có chắc chắn muốn xóa khách hàng này?</span>
      </ng-template>
    </p-confirmDialog>
  `
})
export class CustomerListComponent implements OnInit {
  customers: CustomerDto[] = [];
  loading = false;
  error: string | null = null;
  totalRecords = 0;
  pageSize = 10;
  currentPageNumber = 1;
  searchControl = new FormControl('');

  constructor(
    private apiClient: SuperMarketApiClient,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadCustomers();
    this.searchControl.valueChanges.subscribe(() => {
      this.currentPageNumber = 1;
      this.loadCustomers();
    });
  }

  loadCustomers() {
    this.loading = true;
    this.error = null;

    this.apiClient.paged(
      this.currentPageNumber,
      this.pageSize,
      this.searchControl.value || undefined
    ).subscribe({
      next: (response: CustomerDtoPaginatedResult) => {
        this.customers = response.items || [];
        this.totalRecords = response.totalCount || 0;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Không thể tải danh sách khách hàng';
        this.loading = false;
      }
    });
  }

  previousPage() {
    if (this.currentPageNumber > 1) {
      this.currentPageNumber--;
      this.loadCustomers();
    }
  }

  nextPage() {
    if ((this.currentPageNumber * this.pageSize) < this.totalRecords) {
      this.currentPageNumber++;
      this.loadCustomers();
    }
  }

  goToCreate() {
    this.router.navigate(['/customers/new']);
  }

  goToEdit(id?: string) {
    if (id) {
      this.router.navigate(['/customers/edit', id]);
    }
  }

  confirmDelete(customer: CustomerDto) {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa khách hàng ${customer.name}?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.deleteCustomer(customer.id!);
      }
    });
  }

  deleteCustomer(id: string) {
    this.apiClient.customersDELETE(id).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Xóa khách hàng thành công' });
        this.loadCustomers();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Lỗi', detail: 'Xóa khách hàng thất bại' });
      }
    });
  }
}
