import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Card } from 'primeng/card';

@Component({
    selector: 'app-dashboard',
    imports: [CommonModule, RouterLink, Card],
    template: `
      <div class="p-3">
        <h2 class="mb-3">Dashboard</h2>
        
        <div class="grid">
          <div class="col-6 md:col-3">
            <p-card styleClass="text-center">
              <i class="pi pi-shopping-cart text-4xl text-primary mb-2"></i>
              <div class="text-2xl font-bold">150</div>
              <div class="text-sm text-500">Đơn hàng</div>
            </p-card>
          </div>
          
          <div class="col-6 md:col-3">
            <p-card styleClass="text-center">
              <i class="pi pi-box text-4xl text-green-500 mb-2"></i>
              <div class="text-2xl font-bold">1,234</div>
              <div class="text-sm text-500">Sản phẩm</div>
            </p-card>
          </div>
          
          <div class="col-6 md:col-3">
            <p-card styleClass="text-center">
              <i class="pi pi-truck text-4xl text-orange-500 mb-2"></i>
              <div class="text-2xl font-bold">45</div>
              <div class="text-sm text-500">Nhà cung cấp</div>
            </p-card>
          </div>
          
          <div class="col-6 md:col-3">
            <p-card styleClass="text-center">
              <i class="pi pi-dollar text-4xl text-purple-500 mb-2"></i>
              <div class="text-2xl font-bold">25M</div>
              <div class="text-sm text-500">Doanh thu</div>
            </p-card>
          </div>
        </div>

        <div class="mt-4">
          <p-card>
            <ng-template pTemplate="header">
              <div class="px-3 pt-3">
                <h3 class="m-0">Hoạt động gần đây</h3>
              </div>
            </ng-template>
            <div class="text-center text-500 py-4">
              <i class="pi pi-inbox text-4xl mb-3"></i>
              <p>Chưa có hoạt động nào</p>
            </div>
          </p-card>
        </div>
      </div>
    `,
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
}
