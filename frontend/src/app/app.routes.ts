import { Routes } from '@angular/router';
import { ProjectListComponent } from './features/project-list/project-list.component';
import { ProjectDashboardComponent } from './features/project-dashboard/project-dashboard.component';
import { LoginComponent } from './features/login/login.component';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: ProjectListComponent, canActivate: [authGuard] },
  { path: 'projects/:id', component: ProjectDashboardComponent, canActivate: [authGuard] },
];
