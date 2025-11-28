import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SuperMarketApiClient, ProductDto, ProductDtoPaginatedResult } from '../../../core/api/api-client';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { CheckboxModule } from 'primeng/checkbox';

@Component({
    selector: 'app-product-list',
    imports: [
        CommonModule,
        FormsModule,
        SelectModule,
        ConfirmDialog,
        TableModule,
        TooltipModule,
        ButtonModule,
        ToastModule,
        CheckboxModule
    ],
    providers: [ConfirmationService, MessageService],
    templateUrl: './product-list.component.html',
    styleUrl: './product-list.component.scss'
})
export class ProductListComponent implements OnInit {
  products: ProductDto[] = [];
  loading = false;
  searchTerm = '';
  categories: any[] = [];
  stockFilter: string = 'all';
  timeFilter: string = 'all';

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
