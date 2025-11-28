import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SuperMarketApiClient, ProductDto, CreateTransactionDto, CreateTransactionItemDto } from '../../../core/api/api-client';

interface CartItem {
  product: ProductDto;
  quantity: number;
  discount: number;
  total: number;
}

@Component({
    selector: 'app-pos-main',
    imports: [CommonModule, FormsModule],
    templateUrl: './pos-main.component.html',
    styleUrl: './pos-main.component.scss'
})
export class PosMainComponent implements OnInit {
  cart: CartItem[] = [];
  barcode = '';
  paymentMethod = 'Cash';
  customerName = '';
  customerPhone = '';
  discountAmount = 0;

  constructor(
    private apiClient: SuperMarketApiClient
  ) {}

  ngOnInit(): void {}

  addProductByBarcode(): void {
    if (!this.barcode.trim()) return;

    this.apiClient.barcode(this.barcode).subscribe({
      next: (product: ProductDto) => {
        this.addToCart(product);
        this.barcode = '';
      },
      error: (error) => {
        console.error('Product not found:', error);
        alert('Product not found');
      }
    });
  }

  addToCart(product: ProductDto): void {
    const existingItem = this.cart.find(item => item.product.id === product.id);

    if (existingItem) {
      existingItem.quantity++;
      existingItem.total = existingItem.quantity * existingItem.product.price! - existingItem.discount;
    } else {
      this.cart.push({
        product,
        quantity: 1,
        discount: 0,
        total: product.price!
      });
    }
  }

  removeFromCart(index: number): void {
    this.cart.splice(index, 1);
  }

  updateQuantity(item: CartItem, quantity: number): void {
    if (quantity <= 0) {
      const index = this.cart.indexOf(item);
      this.removeFromCart(index);
    } else {
      item.quantity = quantity;
      item.total = item.quantity * item.product.price! - item.discount;
    }
  }

  getSubTotal(): number {
    return this.cart.reduce((sum, item) => sum + item.total, 0);
  }

  getTax(): number {
    return this.getSubTotal() * 0.10; // 10% tax
  }

  getGrandTotal(): number {
    return this.getSubTotal() + this.getTax() - this.discountAmount;
  }

  checkout(): void {
    if (this.cart.length === 0) {
      alert('Cart is empty');
      return;
    }

    const items: CreateTransactionItemDto[] = this.cart.map(item => ({
      productId: item.product.id!,
      quantity: item.quantity,
      discount: item.discount
    }));

    const transaction: CreateTransactionDto = {
      paymentMethod: this.paymentMethod,
      customerName: this.customerName || undefined,
      customerPhone: this.customerPhone || undefined,
      discountAmount: this.discountAmount,
      items: items
    };

    this.apiClient.transactionsPOST(transaction).subscribe({
      next: (result) => {
        alert(`Transaction completed! Transaction #: ${result.transactionNumber}`);
        this.clearCart();
      },
      error: (error) => {
        console.error('Error creating transaction:', error);
        alert('Error processing transaction: ' + (error.error?.message || 'Unknown error'));
      }
    });
  }

  clearCart(): void {
    this.cart = [];
    this.barcode = '';
    this.customerName = '';
    this.customerPhone = '';
    this.discountAmount = 0;
    this.paymentMethod = 'Cash';
  }
}
