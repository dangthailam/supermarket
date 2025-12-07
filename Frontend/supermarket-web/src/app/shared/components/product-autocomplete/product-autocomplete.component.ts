import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { AutoComplete, AutoCompleteSelectEvent } from 'primeng/autocomplete';
import { SuperMarketApiClient, ProductDto } from '../../../core/api/api-client';
import { debounceTime, Subject, switchMap } from 'rxjs';

@Component({
  selector: 'app-product-autocomplete',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AutoComplete, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ProductAutocompleteComponent),
      multi: true
    }
  ],
  template: `
    <p-autocomplete
      [(ngModel)]="selectedProduct"
      [suggestions]="filteredProducts"
      (completeMethod)="searchProducts($event)"
      (onSelect)="onProductSelect($event)"
      (onClear)="onClear()"
      optionLabel="displayText"
      optionValue="id"
      [placeholder]="placeholder"
      [dropdown]="dropdown"
      [forceSelection]="forceSelection"
      [disabled]="disabled"
      [class]="styleClass"
      [minLength]="minLength"
      [delay]="300"
      [virtualScroll]="true"
      [virtualScrollItemSize]="38"
      [appendTo]="appendTo"
    >
      <ng-template let-product pTemplate="item">
        <div class="product-item">
          <div class="product-info">
            <div class="product-name">{{ product.name }}</div>
            <div class="product-details">
              <span class="sku">SKU: {{ product.sku }}</span>
              @if (product.barcodes && product.barcodes.length > 0) {
                <span class="barcode">| Barcode: {{ product.barcodes[0] }}</span>
              }
            </div>
          </div>
          <div class="product-price">{{ formatCurrency(product.price) }}</div>
        </div>
      </ng-template>
    </p-autocomplete>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
    }

    ::ng-deep {
      .p-autocomplete {
        width: 100%;
      }

      .p-autocomplete-input {
        width: 100%;
      }
    }

    .product-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.5rem;
      gap: 1rem;
    }

    .product-info {
      flex: 1;
      min-width: 0;
    }

    .product-name {
      font-weight: 600;
      color: #333;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .product-details {
      font-size: 0.875rem;
      color: #666;
      margin-top: 0.25rem;
    }

    .sku, .barcode {
      margin-right: 0.5rem;
    }

    .product-price {
      font-weight: 600;
      color: #2196f3;
      white-space: nowrap;
    }
  `]
})
export class ProductAutocompleteComponent implements ControlValueAccessor {
  @Input() placeholder = 'Tìm sản phẩm theo tên, SKU hoặc barcode...';
  @Input() dropdown = false;
  @Input() forceSelection = true;
  @Input() disabled = false;
  @Input() styleClass = '';
  @Input() minLength = 2;
  @Input() appendTo: any = 'body';
  
  @Output() productSelected = new EventEmitter<ProductDto>();
  @Output() productCleared = new EventEmitter<void>();

  selectedProduct: ProductDto | null = null;
  filteredProducts: ProductDto[] = [];

  private searchSubject = new Subject<string>();
  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private apiClient: SuperMarketApiClient) {
    // Setup debounced search
    this.searchSubject.pipe(
      debounceTime(300),
      switchMap(query => this.apiClient.searchProducts(query, 20))
    ).subscribe(products => {
      this.filteredProducts = products.map(p => ({
        ...p,
        displayText: `${p.name} - ${p.sku}`
      }));
    });
  }

  searchProducts(event: any): void {
    const query = event.query;
    if (query && query.length >= this.minLength) {
      this.searchSubject.next(query);
    } else {
      this.filteredProducts = [];
    }
  }

  onProductSelect(event: AutoCompleteSelectEvent): void {
    const product = event.value;
    this.selectedProduct = product;
    this.onChange(product.id);
    this.onTouched();
    this.productSelected.emit(product);
  }

  onClear(): void {
    this.selectedProduct = null;
    this.onChange(null);
    this.onTouched();
    this.productCleared.emit();
  }

  formatCurrency(value: number | undefined): string {
    if (value === undefined || value === null) return '0 ₫';
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
      minimumFractionDigits: 0
    }).format(value);
  }

  // ControlValueAccessor implementation
  writeValue(value: any): void {
    if (value) {
      // If value is a product ID, fetch the product details
      if (typeof value === 'string') {
        this.apiClient.productsGET(value).subscribe(product => {
          this.selectedProduct = product;
        });
      } else if (value && value.id) {
        this.selectedProduct = value;
      }
    } else {
      this.selectedProduct = null;
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}
