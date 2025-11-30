import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SuperMarketApiClient, ProductDto, CreateProductDto, UpdateProductDto, CategoryDto, API_BASE_URL } from '../../../core/api/api-client';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import { InputNumber } from 'primeng/inputnumber';
import { Select } from 'primeng/select';
import { Checkbox } from 'primeng/checkbox';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { FileUpload, FileUploadEvent } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-product-form',
    imports: [
        CommonModule,
        ReactiveFormsModule,
        Card,
        InputText,
        Textarea,
        InputNumber,
        Select,
        Checkbox,
        Button,
        Message,
        ProgressSpinner,
        FileUpload
    ],
    providers: [MessageService],
    templateUrl: './product-form.component.html',
    styleUrl: './product-form.component.scss'
})
export class ProductFormComponent implements OnInit {
  productForm!: FormGroup;
  isEditMode = false;
  productId?: string;
  loading = false;
  error = '';
  categories: CategoryDto[] = [];
  uploadedFiles: any[] = [];
  imagePreview: string | null = null;
  private apiBaseUrl = inject(API_BASE_URL);
  uploadUrl = `${this.apiBaseUrl}/api/products/upload-image`;

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
    this.productId = this.route.snapshot.paramMap.get('id')!;
    this.isEditMode = !!this.productId;

    this.loadCategories();

    if (this.isEditMode) {
      this.loadProduct();
    }
  }

  loadCategories(): void {
    this.apiClient.categoriesAll().subscribe({
      next: (categories: CategoryDto[]) => {
        this.categories = categories;
      },
      error: (err: any) => {
        console.error('Error loading categories:', err);
        this.error = 'Failed to load categories. Please refresh the page.';
      }
    });
  }

  initForm(): void {
    this.productForm = this.fb.group({
      sku: ['', [Validators.required, Validators.maxLength(50)]],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.maxLength(1000)],
      barcode: ['', Validators.maxLength(100)],
      price: [0, [Validators.required, Validators.min(0)]],
      costPrice: [0, [Validators.required, Validators.min(0)]],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      minStockLevel: [0, [Validators.required, Validators.min(0)]],
      categoryId: [null, Validators.required],
      imageUrl: ['', Validators.maxLength(500)],
      isActive: [true]
    });
  }

  loadProduct(): void {
    if (!this.productId) return;

    this.loading = true;
    this.apiClient.productsGET(this.productId).subscribe({
      next: (product: ProductDto) => {
        this.productForm.patchValue({
          sku: product.sku,
          name: product.name,
          description: product.description,
          barcode: product.barcode,
          price: product.price,
          costPrice: product.costPrice,
          stockQuantity: product.stockQuantity,
          minStockLevel: product.minStockLevel,
          categoryId: product.categoryId,
          imageUrl: product.imageUrl,
          isActive: product.isActive
        });
        // Disable SKU in edit mode
        this.productForm.get('sku')?.disable();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load product';
        console.error(err);
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.markFormGroupTouched(this.productForm);
      return;
    }

    this.loading = true;
    this.error = '';

    if (this.isEditMode && this.productId) {
      // Update existing product
      const updateData: UpdateProductDto = {
        name: this.productForm.value.name,
        description: this.productForm.value.description,
        barcode: this.productForm.value.barcode,
        price: this.productForm.value.price,
        costPrice: this.productForm.value.costPrice,
        minStockLevel: this.productForm.value.minStockLevel,
        categoryId: this.productForm.value.categoryId,
        imageUrl: this.productForm.value.imageUrl,
        isActive: this.productForm.value.isActive
      };

      this.apiClient.productsPUT(this.productId, updateData).subscribe({
        next: () => {
          this.router.navigate(['/products']);
        },
        error: (err) => {
          this.error = 'Failed to update product';
          console.error(err);
          this.loading = false;
        }
      });
    } else {
      // Create new product
      const newProduct: CreateProductDto = this.productForm.value;
      this.apiClient.productsPOST(newProduct).subscribe({
        next: () => {
          this.router.navigate(['/products']);
        },
        error: (err) => {
          this.error = 'Failed to create product';
          console.error(err);
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/products']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.productForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getErrorMessage(fieldName: string): string {
    const field = this.productForm.get(fieldName);
    if (field?.hasError('required')) return `${fieldName} is required`;
    if (field?.hasError('min')) return `${fieldName} must be at least ${field.errors?.['min'].min}`;
    if (field?.hasError('maxLength')) return `${fieldName} is too long`;
    return '';
  }

  onImageSelect(event: any): void {
    console.log('Image selected:', event);
    const files = event.files;
    if (files && files.length > 0) {
      const file = files[0];
      
      // Create preview
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(file);

      this.uploadedFiles.push(file);
    }
  }

  onImageUpload(event: any): void {
    console.log('Image uploaded:', event);
    const files = event.files;
    if (files && files.length > 0) {
      const formData = new FormData();
      formData.append('file', files[0]);

      this.loading = true;
      // Call the backend upload endpoint
      fetch('/api/products/upload-image', {
        method: 'POST',
        body: formData
      })
      .then(response => response.json())
      .then((data: any) => {
        // Set the returned image URL in the form
        this.productForm.patchValue({ imageUrl: data.imageUrl });
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Image uploaded successfully' });
        this.loading = false;
      })
      .catch(err => {
        this.error = 'Failed to upload image';
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to upload image' });
        this.loading = false;
      });
    }
  }

  onImageUploadError(event: any): void {
    this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to upload image' });
  }

  removeUploadedFile(file: any): void {
    const index = this.uploadedFiles.indexOf(file);
    if (index > -1) {
      this.uploadedFiles.splice(index, 1);
    }
    this.imagePreview = null;
    this.productForm.patchValue({ imageUrl: null });
  }
}
