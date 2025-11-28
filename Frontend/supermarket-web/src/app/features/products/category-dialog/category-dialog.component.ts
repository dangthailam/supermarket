import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { SuperMarketApiClient, CategoryDto } from '../../../core/api/api-client';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-category-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputText,
    Textarea,
    Button,
    Message
  ],
  template: `
    <form [formGroup]="categoryForm" (ngSubmit)="onSubmit()">
      <div class="flex flex-column gap-3">
        <div>
          <label for="name" class="block mb-2">Tên danh mục <span class="text-red-500">*</span></label>
          <input
            pInputText
            id="name"
            formControlName="name"
            class="w-full"
            placeholder="Nhập tên danh mục"
            [class.ng-invalid]="isFieldInvalid('name')"
            [class.ng-dirty]="categoryForm.get('name')?.touched"
          />
          @if (isFieldInvalid('name')) {
            <small class="text-red-500">Vui lòng nhập tên danh mục</small>
          }
        </div>

        <div>
          <label for="description" class="block mb-2">Mô tả</label>
          <textarea
            pTextarea
            id="description"
            formControlName="description"
            rows="3"
            class="w-full"
            placeholder="Nhập mô tả"
          ></textarea>
        </div>

        <div class="flex justify-content-end gap-2 mt-2">
          <p-button
            label="Hủy"
            severity="secondary"
            (onClick)="onCancel()"
            [disabled]="saving"
          ></p-button>
          <p-button
            label="Lưu"
            type="submit"
            [disabled]="categoryForm.invalid || saving"
            [loading]="saving"
          ></p-button>
        </div>
      </div>
    </form>
  `
})
export class CategoryDialogComponent implements OnInit {
  categoryForm: FormGroup;
  saving = false;
  category?: CategoryDto;

  constructor(
    private fb: FormBuilder,
    private apiClient: SuperMarketApiClient,
    private ref: DynamicDialogRef,
    private config: DynamicDialogConfig
  ) {
    this.categoryForm = this.fb.group({
      name: ['', Validators.required],
      description: ['']
    });
  }

  ngOnInit() {
    this.category = this.config.data?.category;
    if (this.category) {
      this.categoryForm.patchValue({
        name: this.category.name,
        description: this.category.description
      });
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.categoryForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  onSubmit() {
    if (this.categoryForm.valid) {
      this.saving = true;
      const formValue = this.categoryForm.value;

      if (this.category?.id) {
        // Update existing category
        // const updateDto = new UpdateCategoryDto({
        //   name: formValue.name,
        //   description: formValue.description
        // });

        // this.apiClient.updateCategory(this.category.id, updateDto).subscribe({
        //   next: (result) => {
        //     this.ref.close(result);
        //   },
        //   error: (error) => {
        //     console.error('Error updating category:', error);
        //     this.saving = false;
        //   }
        // });
      } else {
        // Create new category
        // const createDto = new CreateCategoryDto({
        //   name: formValue.name,
        //   description: formValue.description
        // });

        // this.apiClient.createCategory(createDto).subscribe({
        //   next: (result) => {
        //     this.ref.close(result);
        //   },
        //   error: (error) => {
        //     console.error('Error creating category:', error);
        //     this.saving = false;
        //   }
        // });
      }
    }
  }

  onCancel() {
    this.ref.close();
  }
}
