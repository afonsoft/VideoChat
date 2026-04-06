import { Routes } from '@angular/router';
import { AuditLogsComponent } from './audit-logs-final.component';

export const auditLogsRoutes: Routes = [
  {
    path: '',
    component: AuditLogsComponent,
    pathMatch: 'full'
  }
];

export default auditLogsRoutes;
