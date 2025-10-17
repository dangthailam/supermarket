import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CategoryService } from '../../../core/services/category.service';
import { Product, CreateProduct } from '../../../core/models/product.model';
import { Category } from '../../../core/models/category.model';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    InputTextModule,
    InputTextareaModule,
    InputNumberModule,
    DropdownModule,
    CheckboxModule,
    ButtonModule,
    MessageModule,
    ProgressSpinnerModule
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss'
})
export class ProductFormComponent implements OnInit {
  productForm!: FormGroup;
  isEditMode = false;
  productId?: number;
  loading = false;
  error = '';
  categories: Category[] = [];

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEditMode = !!this.productId;

    this.loadCategories();

    if (this.isEditMode) {
      this.loadProduct();
    }
  }

  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (categories: Category[]) => {
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
    this.productService.getProductById(this.productId).subscribe({
      next: (product) => {
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
      const updateData = this.productForm.value;
      delete updateData.sku; // SKU is not updatable
      delete updateData.stockQuantity; // Stock is managed via inventory

      this.productService.updateProduct(this.productId, updateData).subscribe({
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
      const newProduct: CreateProduct = this.productForm.value;
      this.productService.createProduct(newProduct).subscribe({
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
}
