import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-audit-logs-simple',
  templateUrl: './audit-logs-simple.component.html',
  styles: [`
    .audit-logs-container {
      padding: 24px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .filters-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .filters-grid div {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .filters-grid label {
      font-weight: 600;
      color: #333;
    }

    .filters-grid input,
    .filters-grid select {
      padding: 8px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
    }

    .filters-grid button {
      padding: 8px 16px;
      background: #1890ff;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
    }

    .filters-grid button:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .loading {
      text-align: center;
      padding: 40px;
      font-size: 16px;
      color: #666;
    }

    .results {
      margin-top: 24px;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    th, td {
      padding: 12px;
      text-align: left;
      border-bottom: 1px solid #eee;
    }

    th {
      background: #f5f5f5;
      font-weight: 600;
    }

    tr:hover {
      background: #f9f9f9;
    }
  `],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class AuditLogsSimpleComponent implements OnInit {
  auditLogs: any[] = [];
  loading = false;
  totalCount = 0;
  form: FormGroup;
  pageSize = 10;
  pageIndex = 1;

  constructor(
    private fb: FormBuilder,
    private listService: ListService,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      startDate: [null],
      endDate: [null],
      userId: [null],
      userName: [null],
      action: [null],
      applicationName: [null],
      clientId: [null],
      correlationId: [null],
      minExecutionDuration: [null],
      maxExecutionDuration: [null],
      hasException: [false],
      httpMethod: [null],
      httpStatusCode: [null],
      url: [null]
    });

    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.loading = true;
    // Mock data para demonstração
    setTimeout(() => {
      this.auditLogs = [
        {
          id: '1',
          applicationName: 'FamilyMeet.HttpApi.Host',
          userId: 'admin',
          userName: 'admin',
          action: 'CreateChatMessage',
          parameters: '{"messageId": "123", "content": "Hello World"}',
          serviceName: 'ChatMessageAppService',
          methodName: 'CreateAsync',
          executionTime: '2024-04-05T22:30:15.123Z',
          executionDuration: 150,
          clientIpAddress: '192.168.1.100',
          clientName: 'Mozilla/5.0',
          correlationId: 'abc123-def456',
          browserInfo: 'Chrome 120.0',
          httpMethod: 'POST',
          httpStatusCode: 200,
          url: '/api/chat/messages',
          exceptions: null,
          additionalData: '{"tenantId": "12345"}'
        }
      ];
      this.totalCount = 1;
      this.loading = false;
    }, 1000);
  }

  search(): void {
    this.pageIndex = 1;
    this.loadAuditLogs();
  }

  reset(): void {
    this.form.reset();
    this.auditLogs = [];
    this.totalCount = 0;
    this.pageIndex = 1;
  }

  viewDetails(log: any): void {
    console.log('View details:', log);
  }

  exportToExcel(): void {
    this.toaster.info('Export functionality will be implemented soon');
  }

  onPageChange(event: any): void {
    this.pageIndex = event;
    this.loadAuditLogs();
  }
}
