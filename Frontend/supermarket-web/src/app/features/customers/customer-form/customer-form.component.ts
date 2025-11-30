import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { SuperMarketApiClient, CustomerDto, CreateCustomerDto, UpdateCustomerDto } from '../../../core/api/api-client';

@Component({
  selector: 'app-customer-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    Card,
    InputText,
    Button,
    Message
  ],
  providers: [MessageService],
  template: `
    <div class="p-3">
      <p-card>
        <ng-template pTemplate="header">
          <div class="px-3 pt-3">
            <h3>{{ isEditMode ? 'Cập nhật' : 'Thêm' }} khách hàng</h3>
          </div>
        </ng-template>

        @if (error) {
          <p-message severity="error" [text]="error" styleClass="mb-3"></p-message>
        }

        <form [formGroup]="customerForm" (ngSubmit)="onSubmit()">
          <div class="grid">
            <div class="col-12">
              <label for="name" class="block mb-2">Tên khách hàng <span class="text-red-500">*</span></label>
              <input
                pInputText
                id="name"
                formControlName="name"
                class="w-full"
                [class.ng-invalid]="isFieldInvalid('name')"
                [class.ng-dirty]="customerForm.get('name')?.touched"
              />
              @if (isFieldInvalid('name')) {
                <small class="text-red-500">Vui lòng nhập tên khách hàng</small>
              }
            </div>

            <div class="col-12 md:col-6">
              <label for="email" class="block mb-2">Email</label>
              <input
                pInputText
                id="email"
                formControlName="email"
                type="email"
                class="w-full"
                [class.ng-invalid]="isFieldInvalid('email')"
                [class.ng-dirty]="customerForm.get('email')?.touched"
              />
              @if (isFieldInvalid('email')) {
                <small class="text-red-500">Vui lòng nhập email hợp lệ</small>
              }
            </div>

            <div class="col-12 md:col-6">
              <label for="phone" class="block mb-2">Số điện thoại</label>
              <input
                pInputText
                id="phone"
                formControlName="phone"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="address" class="block mb-2">Địa chỉ</label>
              <input
                pInputText
                id="address"
                formControlName="address"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="district" class="block mb-2">Quận/Huyện</label>
              <input
                pInputText
                id="district"
                formControlName="district"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="city" class="block mb-2">Thành phố/Tỉnh</label>
              <input
                pInputText
                id="city"
                formControlName="city"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="dateOfBirth" class="block mb-2">Ngày sinh (YYYY-MM-DD)</label>
              <input
                pInputText
                id="dateOfBirth"
                formControlName="dateOfBirth"
                type="date"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="gender" class="block mb-2">Giới tính</label>
              <select formControlName="gender" class="w-full p-2" id="gender">
                <option value="">Chọn giới tính</option>
                <option value="Male">Nam</option>
                <option value="Female">Nữ</option>
                <option value="Other">Khác</option>
              </select>
            </div>

            <div class="col-12 md:col-6">
              <label for="customerType" class="block mb-2">Loại khách hàng</label>
              <select formControlName="customerType" class="w-full p-2" id="customerType">
                <option value="Regular">Bình thường</option>
                <option value="VIP">VIP</option>
                <option value="Wholesale">Bán sỉ</option>
              </select>
            </div>

            @if (isEditMode) {
              <div class="col-12">
                <label for="isActive" class="block mb-2">Trạng thái</label>
                <div class="flex align-items-center">
                  <input type="checkbox" formControlName="isActive" id="isActive" class="w-auto"/>
                  <label for="isActive" class="ml-2">Hoạt động</label>
                </div>
              </div>
            }
          </div>

          <div class="flex gap-2 mt-3">
            <p-button
              label="Lưu"
              icon="pi pi-check"
              type="submit"
              [disabled]="customerForm.invalid || saving"
              [loading]="saving"
            ></p-button>
            <p-button
              label="Hủy"
              icon="pi pi-times"
              severity="secondary"
              (onClick)="onCancel()"
              [disabled]="saving"
            ></p-button>
          </div>
        </form>
      </p-card>
    </div>
  `
})
export class CustomerFormComponent implements OnInit {
  customerForm: FormGroup;
  isEditMode = false;
  customerId?: string;
  saving = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private apiClient: SuperMarketApiClient,
    private messageService: MessageService
  ) {
    this.customerForm = this.fb.group({
      name: [null, Validators.required],
      email: [null, Validators.email],
      phone: [null],
      address: [null],
      district: [null],
      city: [null],
      dateOfBirth: [null],
      gender: [null],
      customerType: ['Regular'],
      isActive: [true]
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.customerId = id;
      this.loadCustomer(this.customerId!);
    }
  }

  loadCustomer(id: string) {
    this.saving = true;
    this.apiClient.customersGET(id).subscribe({
      next: (customer) => {
        const dateOfBirth = customer.dateOfBirth ? new Date(customer.dateOfBirth).toISOString().split('T')[0] : null;
        this.customerForm.patchValue({
          name: customer.name,
          email: customer.email,
          phone: customer.phone,
          address: customer.address,
          district: customer.district,
          city: customer.city,
          dateOfBirth: dateOfBirth,
          gender: customer.gender,
          customerType: customer.customerType,
          isActive: customer.isActive
        });
        this.saving = false;
      },
      error: (err) => {
        this.error = 'Không thể tải dữ liệu khách hàng';
        this.saving = false;
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.customerForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  onSubmit() {
    if (this.customerForm.valid) {
      this.saving = true;
      this.error = null;

      const formValue = this.customerForm.value;

      if (this.isEditMode && this.customerId) {
        this.apiClient.customersPUT(this.customerId, formValue).subscribe({
          next: () => {
            this.saving = false;
            this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Cập nhật khách hàng thành công' });
            this.router.navigate(['/customers']);
          },
          error: (err) => {
            this.error = err.error?.message || 'Cập nhật khách hàng thất bại';
            this.saving = false;
          }
        });
      } else {
        this.apiClient.customersPOST(formValue).subscribe({
          next: () => {
            this.saving = false;
            this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Tạo khách hàng thành công' });
            this.router.navigate(['/customers']);
          },
          error: (err) => {
            this.error = err.error?.message || 'Tạo khách hàng thất bại';
            this.saving = false;
          }
        });
      }
    }
  }

  onCancel() {
    this.router.navigate(['/customers']);
  }
}
