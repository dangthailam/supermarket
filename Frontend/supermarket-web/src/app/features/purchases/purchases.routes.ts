import { Routes } from '@angular/router';

export const PURCHASES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./purchase-list/purchase-list.component').then(m => m.PurchaseListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./purchase-form/purchase-form.component').then(m => m.PurchaseFormComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./purchase-form/purchase-form.component').then(m => m.PurchaseFormComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./purchase-form/purchase-form.component').then(m => m.PurchaseFormComponent)
  }
];
