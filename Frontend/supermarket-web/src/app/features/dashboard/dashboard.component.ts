import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
import { TransactionService } from '../../core/services/transaction.service';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, CardModule, ButtonModule, ProgressSpinnerModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  todaySales = 0;
  lowStockCount = 0;
  loading = true;

  constructor(
    private productService: ProductService,
    private transactionService: TransactionService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    // Load today's sales
    this.transactionService.getTodaysSales().subscribe({
      next: (sales: { totalSales: number }) => {
        this.todaySales = sales.totalSales;
      },
      error: (err: any) => console.error('Error loading sales:', err)
    });

    // Load low stock count
    this.productService.getLowStockProducts().subscribe({
      next: (products: any[]) => {
        this.lowStockCount = products.length;
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading low stock:', err);
        this.loading = false;
      }
    });
  }
}
