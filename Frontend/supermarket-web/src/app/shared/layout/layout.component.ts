import { Component } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidebarModule } from 'primeng/sidebar';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarModule, ButtonModule, MenuModule],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  sidebarVisible = false;
  menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'pi pi-home',
      command: () => this.navigate('/dashboard')
    },
    {
      label: 'Point of Sale',
      icon: 'pi pi-shopping-cart',
      command: () => this.navigate('/pos')
    },
    {
      label: 'Products',
      icon: 'pi pi-box',
      command: () => this.navigate('/products')
    },
    {
      label: 'Inventory',
      icon: 'pi pi-list',
      command: () => this.navigate('/inventory')
    },
    {
      label: 'Reports',
      icon: 'pi pi-chart-bar',
      command: () => this.navigate('/reports')
    }
  ];

  constructor(private router: Router) {}

  navigate(path: string) {
    this.router.navigate([path]);
    this.sidebarVisible = false;
  }

  toggleSidebar() {
    this.sidebarVisible = !this.sidebarVisible;
  }
}
