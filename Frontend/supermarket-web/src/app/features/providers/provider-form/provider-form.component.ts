import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { SuperMarketApiClient } from '../../../core/api/api-client';

@Component({
  selector: 'app-provider-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    Card,
    InputText,
    Textarea,
    Button,
    Message
  ],
  template: `
    <div class="p-3">
      <p-card>
        <ng-template pTemplate="header">
          <div class="px-3 pt-3">
            <h3>{{ isEditMode ? 'Cập nhật' : 'Thêm' }} nhà cung cấp</h3>
          </div>
        </ng-template>

        @if (error) {
          <p-message severity="error" [text]="error" styleClass="mb-3"></p-message>
        }

        <form [formGroup]="providerForm" (ngSubmit)="onSubmit()">
          <div class="grid">
            <div class="col-12">
              <label for="name" class="block mb-2">Tên nhà cung cấp <span class="text-red-500">*</span></label>
              <input
                pInputText
                id="name"
                formControlName="name"
                class="w-full"
                [class.ng-invalid]="isFieldInvalid('name')"
                [class.ng-dirty]="providerForm.get('name')?.touched"
              />
              @if (isFieldInvalid('name')) {
                <small class="text-red-500">Vui lòng nhập tên nhà cung cấp</small>
              }
            </div>

            <div class="col-12 md:col-6">
              <label for="contactName" class="block mb-2">Người liên hệ</label>
              <input
                pInputText
                id="contactName"
                formControlName="contactName"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="phone" class="block mb-2">Số điện thoại</label>
              <input
                pInputText
                id="phone"
                formControlName="phone"
                class="w-full"
                [class.ng-invalid]="isFieldInvalid('phone')"
                [class.ng-dirty]="providerForm.get('phone')?.touched"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="email" class="block mb-2">Email</label>
              <input
                pInputText
                id="email"
                formControlName="email"
                type="email"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="code" class="block mb-2">Mã nhà cung cấp</label>
              <input
                pInputText
                id="code"
                formControlName="code"
                class="w-full"
                readonly
              />
              <small class="form-hint">Auto-generated</small>
            </div>

            <div class="col-12 md:col-6">
              <label for="companyName" class="block mb-2">Tên công ty</label>
              <input
                pInputText
                id="companyName"
                formControlName="companyName"
                class="w-full"
              />
            </div>

            <div class="col-12 md:col-6">
              <label for="taxNumber" class="block mb-2">Mã số thuế</label>
              <input
                pInputText
                id="taxNumber"
                formControlName="taxNumber"
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

            <div class="col-12">
              <label for="note" class="block mb-2">Ghi chú</label>
              <textarea
                pTextarea
                id="note"
                formControlName="note"
                rows="3"
                class="w-full"
              ></textarea>
            </div>
          </div>

          <div class="flex gap-2 mt-3">
            <p-button
              label="Lưu"
              icon="pi pi-check"
              type="submit"
              [disabled]="providerForm.invalid || saving"
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
export class ProviderFormComponent implements OnInit {
  providerForm: FormGroup;
  isEditMode = false;
  providerId?: string;
  saving = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private providerService: SuperMarketApiClient
  ) {
    this.providerForm = this.fb.group({
      name: [null, Validators.required],
      code: [null],
      contactName: [null],
      phone: [null],
      email: [null, [Validators.email]],
      address: [null],
      district: [null],
      city: [null],
      note: [null],
      companyName: [null],
      taxNumber: [null]
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.providerId = id;
      this.loadProvider(this.providerId!);
    } else {
      // Set code as readonly and auto-generated
      this.providerForm.get('code')?.disable();
    }
  }

  loadProvider(id: string) {
    this.saving = true;
    this.providerService.providersGET(id).subscribe({
      next: (provider) => {
        this.providerForm.patchValue({
          name: provider.name,
          code: provider.code,
          contactName: provider.contactName,
          phone: provider.phone,
          email: provider.email,
          address: provider.address,
          district: provider.district,
          city: provider.city,
          note: provider.note,
          companyName: provider.companyName,
          taxNumber: provider.taxNumber
        });
        this.saving = false;
      },
      error: (err) => {
        this.error = 'Không thể tải dữ liệu nhà cung cấp';
        this.saving = false;
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.providerForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  onSubmit() {
    if (this.providerForm.valid) {
      this.saving = true;
      this.error = null;

      const formValue = this.providerForm.getRawValue();
      
      if (this.isEditMode && this.providerId) {
        this.providerService.providersPUT(this.providerId, formValue).subscribe({
          next: () => {
            this.saving = false;
            this.router.navigate(['/providers']);
          },
          error: (err) => {
            this.error = 'Cập nhật nhà cung cấp thất bại';
            this.saving = false;
          }
        });
      } else {
        this.providerService.providersPOST(formValue).subscribe({
          next: () => {
            this.saving = false;
            this.router.navigate(['/providers']);
          },
          error: (err) => {
            this.error = 'Tạo nhà cung cấp thất bại';
            this.saving = false;
          }
        });
      }
    }
  }

  onCancel() {
    this.router.navigate(['/providers']);
  }
}
