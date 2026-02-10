import { Routes } from '@angular/router';
import { ProjectListComponent } from './features/project-list/project-list.component';
import { ProjectDashboardComponent } from './features/project-dashboard/project-dashboard.component';

export const routes: Routes = [
  { path: '', component: ProjectListComponent },
  { path: 'projects/:id', component: ProjectDashboardComponent },
];
