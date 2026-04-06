import { Routes } from '@angular/router';
import { AuditLogsComponent } from './audit-logs.component';

export const auditLogsRoutes: Routes = [
  {
    path: '',
    component: AuditLogsComponent,
    pathMatch: 'full'
  }
];

export default auditLogsRoutes;
