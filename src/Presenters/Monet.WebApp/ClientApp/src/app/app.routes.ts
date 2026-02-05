import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./dashboard/dashboard.component').then((module) => module.DashboardComponent)
  },
  {
    path: 'accounts',
    loadComponent: () =>
      import('./accounts/accounts.component').then((module) => module.AccountsComponent)
  },
  {
    path: 'transactions',
    loadComponent: () =>
      import('./transactions/transactions.component').then((module) => module.TransactionsComponent)
  },
  {
    path: 'focus',
    loadComponent: () =>
      import('./focus/focus.component').then((module) => module.FocusComponent)
  }
];
