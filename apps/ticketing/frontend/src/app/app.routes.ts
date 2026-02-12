import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'tickets', pathMatch: 'full' },
      {
        path: 'tickets',
        loadComponent: () => import('./features/ticket-list/ticket-list.component').then(m => m.TicketListComponent)
      },
      {
        path: 'tickets/new',
        loadComponent: () => import('./features/ticket-create/ticket-create.component').then(m => m.TicketCreateComponent)
      },
      {
        path: 'tickets/:id',
        loadComponent: () => import('./features/ticket-detail/ticket-detail.component').then(m => m.TicketDetailComponent)
      },
      {
        path: 'equipment',
        loadComponent: () => import('./features/equipment-list/equipment-list.component').then(m => m.EquipmentListComponent)
      },
      {
        path: 'knowledge-base',
        loadComponent: () => import('./features/knowledge-base/knowledge-base.component').then(m => m.KnowledgeBaseComponent)
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/sla-dashboard/sla-dashboard.component').then(m => m.SlaDashboardComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
