import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SuperMarketApiClient, ProductDto, ProductDtoPaginatedResult, CategoryDto } from '../../../core/api/api-client';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { CheckboxModule } from 'primeng/checkbox';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';

@Component({
    selector: 'app-product-list',
    imports: [
        CommonModule,
        FormsModule,
        ConfirmDialog,
        TableModule,
        TooltipModule,
        ButtonModule,
        ToastModule,
        CheckboxModule,
        DialogModule,
        InputTextModule
    ],
    providers: [ConfirmationService, MessageService],
    templateUrl: './product-list.component.html',
    styleUrl: './product-list.component.scss'
})
export class ProductListComponent implements OnInit {
  products: ProductDto[] = [];
  loading = false;
  searchTerm = '';
  categories: CategoryDto[] = [];
  stockFilter: string = 'all';
  timeFilter: string = 'all';
  
  // Category dialog
  showCategoryDialog = false;
  newCategoryName = '';

  // Pagination properties
  totalRecords = 0;
  pageSize = 20;
  currentPage = 1;
  sortBy = '';
  sortDescending = false;

  constructor(
    private apiClient: SuperMarketApiClient,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  manageCategories(): void {
    this.showCategoryDialog = true;
    this.newCategoryName = '';
    // Load categories when dialog opens
    this.loadCategoriesForDialog();
  }

  loadCategoriesForDialog(): void {
    // Mock data for now - replace with actual API call when available
    this.categories = [];
  }

  createCategory(): void {
    if (!this.newCategoryName.trim()) {
      return;
    }

    // Mock creation - replace with actual API call when available
    const newCategory: CategoryDto = {
      name: this.newCategoryName.trim(),
      description: ''
    } as CategoryDto;

    this.categories.push(newCategory);
    this.newCategoryName = '';
    this.messageService.add({
      severity: 'success',
      summary: 'Thành công',
      detail: 'Đã thêm danh mục mới'
    });
  }

  editCategory(category: CategoryDto): void {
    const newName = prompt('Nhập tên mới cho danh mục:', category.name || '');
    if (newName && newName.trim() && newName !== category.name) {
      category.name = newName.trim();
      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: 'Đã cập nhật danh mục'
      });
    }
  }

  deleteCategory(category: CategoryDto): void {
    this.confirmationService.confirm({
      message: `Bạn có chắc chắn muốn xóa danh mục "${category.name}"?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.categories = this.categories.filter(c => c.id !== category.id);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã xóa danh mục'
        });
      }
    });
  }

  loadProducts(): void {
    this.loading = true;

    this.apiClient.paged(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.sortBy || undefined,
      this.sortDescending
    ).subscribe({
      next: (result: ProductDtoPaginatedResult) => {
        this.products = result.items || [];
        this.totalRecords = result.totalCount || 0;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.loading = false;
      }
    });
  }

  searchProducts(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1; // Reset to first page on search
    this.loadProducts();
  }

  onPageChange(event: any): void {
    this.currentPage = event.page + 1; // PrimeNG uses 0-based index
    this.pageSize = event.rows;
    this.loadProducts();
  }

  onLazyLoad(event: any): void {
    this.currentPage = (event.first / event.rows) + 1;
    this.pageSize = event.rows;

    // Handle sorting
    if (event.sortField) {
      this.sortBy = event.sortField;
      this.sortDescending = event.sortOrder === -1;
    }

    this.loadProducts();
  }

  onSort(field: string): void {
    if (this.sortBy === field) {
      this.sortDescending = !this.sortDescending;
    } else {
      this.sortBy = field;
      this.sortDescending = false;
    }
    this.loadProducts();
  }

  deleteProduct(product: ProductDto): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete ${product.name}?`,
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.apiClient.productsDELETE(product.id!).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Product deleted successfully'
            });
            this.loadProducts();
          },
          error: (error: any) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete product'
            });
            console.error('Error deleting product:', error);
          }
        });
      }
    });
  }

  addProduct(): void {
    this.router.navigate(['/products/new']);
  }

  editProduct(id: number): void {
    this.router.navigate(['/products/edit', id]);
  }

  getTotalValue(): number {
    return this.products.reduce((sum, product) => sum + ((product.price || 0) * (product.stockQuantity || 0)), 0);
  }
}
