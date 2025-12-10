import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormArray } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  SuperMarketApiClient,
  PurchaseDto,
  CreatePurchaseDto,
  UpdatePurchaseDto,
  CreatePurchaseItemDto,
  ProviderDto,
  ProductDto
} from '../../../core/api/api-client';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { Select } from 'primeng/select';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { Textarea } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { Toast } from 'primeng/toast';
import { DatePickerModule } from 'primeng/datepicker';
import { TableModule } from 'primeng/table';
import { ProductAutocompleteComponent } from '../../../shared/components/product-autocomplete/product-autocomplete.component';

@Component({
  selector: 'app-purchase-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    Card,
    InputText,
    InputNumber,
    Select,
    Button,
    Message,
    DatePickerModule,
    Textarea,
    Toast,
    TableModule,
    ProductAutocompleteComponent
  ],
  providers: [MessageService],
  templateUrl: './purchase-form.component.html',
  styleUrl: './purchase-form.component.scss'
})
export class PurchaseFormComponent implements OnInit {
  purchaseForm!: FormGroup;
  isEditMode = false;
  purchaseId?: string;
  error = '';
  providers: ProviderDto[] = [];
  products: ProductDto[] = [];
  showNoteForItem: { [key: number]: boolean } = {};
  
  statusOptions = [
    { label: 'Chờ xử lý', value: 1 },
    { label: 'Đã thanh toán', value: 2 },
    { label: 'Đã hủy', value: 3 }
  ];

  constructor(
    private fb: FormBuilder,
    private apiClient: SuperMarketApiClient,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.purchaseId = this.route.snapshot.paramMap.get('id')!;
    this.isEditMode = !!this.purchaseId;

    this.loadProviders();
    this.loadProducts();

    if (this.isEditMode) {
      this.loadPurchase();
    }
  }

  initForm(): void {
    this.purchaseForm = this.fb.group({
      purchaseDate: [new Date(), Validators.required],
      providerId: [null, Validators.required],
      status: [1, Validators.required],
      note: [''],
      items: this.fb.array([])
    });
  }

  get items(): FormArray {
    return this.purchaseForm.get('items') as FormArray;
  }

  createItemFormGroup(item?: CreatePurchaseItemDto): FormGroup {
    const formGroup = this.fb.group({
      productId: [item?.productId || null, Validators.required],
      quantity: [item?.quantity || 1, [Validators.required, Validators.min(1)]],
      purchasePrice: [item?.purchasePrice || 0, [Validators.required, Validators.min(0)]],
      discount: [item?.discount || 0, Validators.min(0)],
      note: [item?.note || '']
    });

    return formGroup;
  }

  onProductSelectedForItem(index: number, product: ProductDto): void {
    const itemFormGroup = this.items.at(index);
    if (product && product.price) {
      itemFormGroup.patchValue({
        purchasePrice: product.price
      });
    }
  }

  toggleNoteForItem(index: number): void {
    this.showNoteForItem[index] = !this.showNoteForItem[index];
  }

  addItem(): void {
    this.items.push(this.createItemFormGroup());
  }

  removeItem(index: number): void {
    this.items.removeAt(index);
  }

  loadProviders(): void {
    this.apiClient.providersAll().subscribe({
      next: (providers) => {
        this.providers = providers;
      },
      error: (err) => {
        console.error('Error loading providers:', err);
        this.error = 'Không thể tải danh sách nhà cung cấp';
      }
    });
  }

  loadProducts(): void {
    this.apiClient.productsAll().subscribe({
      next: (products) => {
        this.products = products;
      },
      error: (err) => {
        console.error('Error loading products:', err);
        this.error = 'Không thể tải danh sách sản phẩm';
      }
    });
  }

  loadPurchase(): void {
    if (!this.purchaseId) return;

    this.apiClient.purchasesGET(this.purchaseId).subscribe({
      next: (purchase: PurchaseDto) => {
        this.purchaseForm.patchValue({
          purchaseDate: new Date(purchase.purchaseDate!),
          providerId: purchase.providerId,
          status: purchase.status,
          note: purchase.note
        });

        // Load items
        purchase.items?.forEach(item => {
          this.items.push(this.createItemFormGroup({
            productId: item.productId,
            quantity: item.quantity,
            purchasePrice: item.purchasePrice,
            discount: item.discount,
            note: item.note
          }));
        });
      },
      error: (err) => {
        this.error = 'Không thể tải phiếu nhập';
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (this.purchaseForm.invalid) {
      this.markFormGroupTouched(this.purchaseForm);
      return;
    }

    if (this.items.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng thêm ít nhất một sản phẩm'
      });
      return;
    }

    this.error = '';

    const formValue = this.purchaseForm.value;

    if (this.isEditMode && this.purchaseId) {
      const updateData: UpdatePurchaseDto = {
        purchaseDate: formValue.purchaseDate,
        providerId: formValue.providerId,
        status: formValue.status,
        note: formValue.note,
        items: formValue.items
      };

      this.apiClient.purchasesPUT(this.purchaseId, updateData).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Đã cập nhật phiếu nhập'
          });
          this.router.navigate(['/purchases']);
        },
        error: (err) => {
          this.error = 'Không thể cập nhật phiếu nhập';
          console.error(err);
        }
      });
    } else {
      const newPurchase: CreatePurchaseDto = {
        purchaseDate: formValue.purchaseDate,
        providerId: formValue.providerId,
        status: formValue.status,
        note: formValue.note,
        items: formValue.items
      };

      this.apiClient.purchasesPOST(newPurchase).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Đã tạo phiếu nhập mới'
          });
          this.router.navigate(['/purchases']);
        },
        error: (err) => {
          this.error = 'Không thể tạo phiếu nhập';
          console.error(err);
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/purchases']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control instanceof FormArray) {
        control.controls.forEach(c => {
          if (c instanceof FormGroup) {
            this.markFormGroupTouched(c);
          }
        });
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.purchaseForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getItemTotal(index: number): number {
    const item = this.items.at(index).value;
    return (item.quantity * item.purchasePrice) - (item.discount || 0);
  }

  getTotalAmount(): number {
    let total = 0;
    for (let i = 0; i < this.items.length; i++) {
      total += this.getItemTotal(i);
    }
    return total;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0
    }).format(value);
  }

  getProductName(productId: string): string {
    const product = this.products.find(p => p.id === productId);
    return product?.name || '';
  }
}
