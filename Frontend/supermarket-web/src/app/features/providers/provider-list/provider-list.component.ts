import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { Toast } from 'primeng/toast';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';

interface Provider {
  id: number;
  name: string;
  contactName: string;
  phone: string;
  email: string;
  address: string;
}

@Component({
  selector: 'app-provider-list',
  imports: [
    CommonModule,
    Button,
    Card,
    TableModule,
    Toast,
    ConfirmDialog
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

      <p-table
        [value]="providers"
        [loading]="loading"
        [paginator]="true"
        [rows]="10"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Hiển thị {first} đến {last} của {totalRecords} nhà cung cấp"
        styleClass="p-datatable-sm"
      >
        <ng-template pTemplate="header">
          <tr>
            <th>Tên nhà cung cấp</th>
            <th>Người liên hệ</th>
            <th>Số điện thoại</th>
            <th>Email</th>
            <th style="width: 8rem">Thao tác</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-provider>
          <tr>
            <td>{{ provider.name }}</td>
            <td>{{ provider.contactName }}</td>
            <td>{{ provider.phone }}</td>
            <td>{{ provider.email }}</td>
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
            <td colspan="5" class="text-center">Chưa có nhà cung cấp nào</td>
          </tr>
        </ng-template>
      </p-table>
    </div>
  `
})
export class ProviderListComponent implements OnInit {
  providers: Provider[] = [];
  loading = false;

  constructor(
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadProviders();
  }

  loadProviders() {
    this.loading = true;
    // Mock data - replace with actual API call
    setTimeout(() => {
      this.providers = [
        {
          id: 1,
          name: 'Công ty TNHH ABC',
          contactName: 'Nguyễn Văn A',
          phone: '0123456789',
          email: 'abc@example.com',
          address: 'Hà Nội'
        },
        {
          id: 2,
          name: 'Công ty CP XYZ',
          contactName: 'Trần Thị B',
          phone: '0987654321',
          email: 'xyz@example.com',
          address: 'TP.HCM'
        }
      ];
      this.loading = false;
    }, 500);
  }

  addProvider() {
    this.router.navigate(['/providers/new']);
  }

  editProvider(provider: Provider) {
    this.router.navigate(['/providers/edit', provider.id]);
  }

  deleteProvider(provider: Provider) {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa nhà cung cấp ${provider.name}?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        // Delete provider - replace with actual API call
        this.providers = this.providers.filter(p => p.id !== provider.id);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã xóa nhà cung cấp'
        });
      }
    });
  }
}
