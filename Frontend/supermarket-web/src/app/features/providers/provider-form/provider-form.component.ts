import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';

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
              <label for="phone" class="block mb-2">Số điện thoại <span class="text-red-500">*</span></label>
              <input
                pInputText
                id="phone"
                formControlName="phone"
                class="w-full"
                [class.ng-invalid]="isFieldInvalid('phone')"
                [class.ng-dirty]="providerForm.get('phone')?.touched"
              />
              @if (isFieldInvalid('phone')) {
                <small class="text-red-500">Vui lòng nhập số điện thoại</small>
              }
            </div>

            <div class="col-12">
              <label for="email" class="block mb-2">Email</label>
              <input
                pInputText
                id="email"
                formControlName="email"
                type="email"
                class="w-full"
              />
            </div>

            <div class="col-12">
              <label for="address" class="block mb-2">Địa chỉ</label>
              <textarea
                pTextarea
                id="address"
                formControlName="address"
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
  providerId?: number;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.providerForm = this.fb.group({
      name: ['', Validators.required],
      contactName: [''],
      phone: ['', Validators.required],
      email: ['', Validators.email],
      address: ['']
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.providerId = +id;
      this.loadProvider(this.providerId);
    }
  }

  loadProvider(id: number) {
    // Mock data - replace with actual API call
    setTimeout(() => {
      this.providerForm.patchValue({
        name: 'Công ty TNHH ABC',
        contactName: 'Nguyễn Văn A',
        phone: '0123456789',
        email: 'abc@example.com',
        address: 'Hà Nội'
      });
    }, 300);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.providerForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  onSubmit() {
    if (this.providerForm.valid) {
      this.saving = true;
      // Save provider - replace with actual API call
      setTimeout(() => {
        this.saving = false;
        this.router.navigate(['/providers']);
      }, 500);
    }
  }

  onCancel() {
    this.router.navigate(['/providers']);
  }
}
