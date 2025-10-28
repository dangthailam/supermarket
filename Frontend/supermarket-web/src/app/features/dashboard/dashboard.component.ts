import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SuperMarketApiClient, ProductDto } from '../../core/api/api-client';
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
    private apiClient: SuperMarketApiClient
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    // Load today's sales
    this.apiClient.sales().subscribe({
      next: (totalSales: number) => {
        this.todaySales = totalSales;
      },
      error: (err: any) => console.error('Error loading sales:', err)
    });

    // Load low stock count
    this.apiClient.lowStock().subscribe({
      next: (products: ProductDto[]) => {
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
