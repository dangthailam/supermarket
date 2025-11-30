import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { TableModule } from 'primeng/table';
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
    TableModule,
    ConfirmDialog
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <div class="p-3">
      <div class="px-3 pt-3 flex justify-content-between align-items-center mb-3">
        <h2 class="m-0">Quản lý khách hàng</h2>
        <p-button label="Thêm khách hàng" icon="pi pi-plus" (onClick)="goToCreate()"></p-button>
      </div>

      @if (error) {
        <p-message severity="error" [text]="error" styleClass="mb-3"></p-message>
      }

      <div class="mb-3">
        <input pInputText placeholder="Tìm kiếm theo tên, email, số điện thoại..." 
               [formControl]="searchControl" class="w-full"/>
      </div>

      <p-table [value]="customers" [loading]="loading" [rows]="pageSize" [paginator]="false" responsiveLayout="scroll">
        <ng-template pTemplate="header">
          <tr>
            <th>Tên khách hàng</th>
            <th>Email</th>
            <th>Số điện thoại</th>
            <th>Loại khách hàng</th>
            <th>Trạng thái</th>
            <th>Hành động</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-customer>
          <tr>
            <td>{{ customer.name }}</td>
            <td>{{ customer.email }}</td>
            <td>{{ customer.phone || '-' }}</td>
            <td>{{ customer.customerType || '-' }}</td>
            <td>
              {{ customer.isActive ? 'Hoạt động' : 'Không hoạt động' }}
            </td>
            <td>
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
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="6" class="text-center py-5">
              <span class="text-500">Không có khách hàng nào</span>
            </td>
          </tr>
        </ng-template>
      </p-table>

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
