import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Card } from 'primeng/card';

interface MenuItem {
  icon: string;
  label: string;
  description: string;
  route?: string;
  action?: () => void;
}

@Component({
  selector: 'app-more',
  imports: [CommonModule, Card],
  template: `
    <div class="p-3">
      <h2 class="mb-3">Thêm</h2>
      
      <div class="grid">
        @for (item of menuItems; track item.label) {
          <div class="col-12 md:col-6 lg:col-4">
            <p-card class="cursor-pointer hover:surface-100 transition-colors transition-duration-150"
                    (click)="onMenuClick(item)">
              <div class="flex align-items-center gap-3">
                <i [class]="item.icon + ' text-3xl text-primary'"></i>
                <div>
                  <div class="font-semibold mb-1">{{ item.label }}</div>
                  <div class="text-sm text-500">{{ item.description }}</div>
                </div>
              </div>
            </p-card>
          </div>
        }
      </div>
    </div>
  `
})
export class MoreComponent {
  menuItems: MenuItem[] = [
    {
      icon: 'pi pi-cog',
      label: 'Cài đặt',
      description: 'Cấu hình hệ thống',
      route: '/settings'
    },
    {
      icon: 'pi pi-users',
      label: 'Nhân viên',
      description: 'Quản lý nhân viên',
      route: '/employees'
    },
    {
      icon: 'pi pi-user',
      label: 'Khách hàng',
      description: 'Quản lý khách hàng',
      route: '/customers'
    },
    {
      icon: 'pi pi-chart-bar',
      label: 'Báo cáo',
      description: 'Xem báo cáo và thống kê',
      route: '/reports'
    },
    {
      icon: 'pi pi-shopping-cart',
      label: 'Đơn hàng',
      description: 'Quản lý đơn hàng',
      route: '/orders'
    },
    {
      icon: 'pi pi-book',
      label: 'Hướng dẫn',
      description: 'Tài liệu và hướng dẫn',
      action: () => window.open('https://docs.example.com', '_blank')
    },
    {
      icon: 'pi pi-question-circle',
      label: 'Hỗ trợ',
      description: 'Liên hệ hỗ trợ',
      action: () => alert('Hotline: 1900 xxxx')
    },
    {
      icon: 'pi pi-sign-out',
      label: 'Đăng xuất',
      description: 'Thoát khỏi hệ thống',
      action: () => this.logout()
    }
  ];

  constructor(private router: Router) {}

  onMenuClick(item: MenuItem) {
    if (item.route) {
      this.router.navigate([item.route]);
    } else if (item.action) {
      item.action();
    }
  }

  logout() {
    if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
      // Implement logout logic
      console.log('Logout');
    }
  }
}
