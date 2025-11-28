import { Routes } from '@angular/router';

export const PROVIDERS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./provider-list/provider-list.component').then(m => m.ProviderListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./provider-form/provider-form.component').then(m => m.ProviderFormComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./provider-form/provider-form.component').then(m => m.ProviderFormComponent)
  }
];
