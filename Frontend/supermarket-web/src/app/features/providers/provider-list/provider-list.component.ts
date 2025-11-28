import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputGroupModule } from 'primeng/inputgroup';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ProviderDto, ProviderDtoPaginatedResult, SuperMarketApiClient } from '../../../core/api/api-client';

@Component({
  selector: 'app-provider-list',
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    TableModule,
    ToastModule,
    ConfirmDialogModule,
    InputGroupModule,
    InputTextModule
  ],
  providers: [ConfirmationService, MessageService],
  template: `
    <p-toast></p-toast>
    <p-confirmDialog></p-confirmDialog>
    
    <div class="p-3">
      <div class="flex justify-content-between align-items-center mb-3">
        <h2 class="m-0">Nhà cung cấp</h2>
        <p-button label="Thêm nhà cung cấp" icon="pi pi-plus" (onClick)="addProvider()"></p-button>
      </div>

      <div class="mb-3">
        <p-input-group>
          <input
            pInputText
            type="text"
            placeholder="Tìm kiếm nhà cung cấp..."
            [(ngModel)]="searchTerm"
            (keyup.enter)="onSearch()"
          />
          <p-button icon="pi pi-search" (onClick)="onSearch()"></p-button>
        </p-input-group>
      </div>

      <p-table
        [value]="providers"
        [loading]="loading"
        [paginator]="true"
        [rows]="pageSize"
        [totalRecords]="totalRecords"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Hiển thị {first} đến {last} của {totalRecords} nhà cung cấp"
        styleClass="p-datatable-sm"
        (onLazyLoad)="loadProviders($event)"
        [lazy]="true"
      >
        <ng-template pTemplate="header">
          <tr>
            <th>Tên nhà cung cấp</th>
            <th>Mã</th>
            <th>Số điện thoại</th>
            <th>Email</th>
            <th>Địa chỉ</th>
            <th style="width: 8rem">Thao tác</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-provider>
          <tr>
            <td>{{ provider.name }}</td>
            <td>{{ provider.code }}</td>
            <td>{{ provider.phone }}</td>
            <td>{{ provider.email }}</td>
            <td>{{ provider.address }}</td>
            <td>
              <p-button
                icon="pi pi-pencil"
                [text]="true"
                (onClick)="editProvider(provider)"
                styleClass="p-button-sm"
              ></p-button>
              <p-button
                icon="pi pi-trash"
                severity="danger"
                [text]="true"
                (onClick)="deleteProvider(provider)"
                styleClass="p-button-sm"
              ></p-button>
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="6" class="text-center">Chưa có nhà cung cấp nào</td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `
})
export class ProviderListComponent implements OnInit {
  providers: ProviderDto[] = [];
  loading = false;
  pageSize = 10;
  totalRecords = 0;
  currentPage = 1;
  searchTerm = '';

  constructor(
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private providerService: SuperMarketApiClient
  ) {}

  ngOnInit() {
    this.loadProviders();
  }

  loadProviders(event?: any) {
    this.loading = true;
    const pageNumber = event?.first ? event.first / event.rows + 1 : 1;
    const pageSize = event?.rows || this.pageSize;

    this.providerService.paged2(pageNumber, pageSize, this.searchTerm).subscribe({
      next: (result: ProviderDtoPaginatedResult) => {
        this.providers = result.items!;
        this.totalRecords = result.totalCount!;
        this.currentPage = result.pageNumber!;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không thể tải dữ liệu nhà cung cấp'
        });
        this.loading = false;
      }
    });
  }

  onSearch() {
    this.currentPage = 1;
    this.loadProviders();
  }

  addProvider() {
    this.router.navigate(['/providers/new']);
  }

  editProvider(provider: ProviderDto) {
    this.router.navigate(['/providers/edit', provider.id]);
  }

  deleteProvider(provider: ProviderDto) {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa nhà cung cấp ${provider.name}?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.providerService.providersDELETE(provider.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Thành công',
              detail: 'Đã xóa nhà cung cấp'
            });
            this.loadProviders();
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Lỗi',
              detail: 'Không thể xóa nhà cung cấp'
            });
          }
        });
      }
    });
  }
}
