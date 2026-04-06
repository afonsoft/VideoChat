import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { CommonModule } from '@angular/common';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-audit-logs',
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AuditLogsComponent implements OnInit {
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
    // TODO: Implementar chamada à API de audit logs
    // this.listService.getList('/api/audit-logging/audit-logs', { ...this.form.value })
    //   .subscribe((result: PagedResultDto<any>) => {
    //     this.auditLogs = result.items;
    //     this.totalCount = result.totalCount;
    //     this.loading = false;
    //   });

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
    // TODO: Implementar modal de detalhes
    console.log('View details:', log);
  }

  exportToExcel(): void {
    // TODO: Implementar exportação para Excel
    this.toaster.info('Export functionality will be implemented soon');
  }

  onPageChange(event: any): void {
    this.pageIndex = event;
    this.loadAuditLogs();
  }
}
