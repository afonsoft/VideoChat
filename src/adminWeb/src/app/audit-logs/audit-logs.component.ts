import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DatePipe
  ],
  template: `
    <div class="container">
      <div class="header">
        <h2>Audit Logs</h2>
        <p>Visualização e gerenciamento de logs de auditoria do sistema</p>
      </div>
      
      <div class="filters">
        <form [formGroup]="searchForm" (ngSubmit)="onSearch()">
          <div class="form-row">
            <div class="form-group">
              <label for="startDate">Data Inicial:</label>
              <input 
                type="date" 
                id="startDate" 
                formControlName="startDate" 
                class="form-control"
              />
            </div>
            
            <div class="form-group">
              <label for="endDate">Data Final:</label>
              <input 
                type="date" 
                id="endDate" 
                formControlName="endDate" 
                class="form-control"
              />
            </div>
            
            <div class="form-group">
              <label for="userName">Usuário:</label>
              <input 
                type="text" 
                id="userName" 
                formControlName="userName" 
                placeholder="Filtrar por usuário"
                class="form-control"
              />
            </div>
            
            <div class="form-group">
              <label for="action">Ação:</label>
              <input 
                type="text" 
                id="action" 
                formControlName="action" 
                placeholder="Filtrar por ação"
                class="form-control"
              />
            </div>
            
            <div class="form-group">
              <label for="logLevel">Nível:</label>
              <select id="logLevel" formControlName="logLevel" class="form-control">
                <option value="">Todos</option>
                <option value="Information">Informação</option>
                <option value="Warning">Aviso</option>
                <option value="Error">Erro</option>
                <option value="Critical">Crítico</option>
              </select>
            </div>
          </div>
          
          <div class="form-actions">
            <button type="submit" class="btn btn-primary">Buscar</button>
            <button type="button" class="btn btn-secondary" (click)="onReset()">Limpar</button>
          </div>
        </form>
      </div>
      
      @if (auditLogs.length > 0) {
        <div class="results">
          <div class="results-header">
            <h3>Resultados ({{ auditLogs.length }} logs encontrados)</h3>
            <div class="pagination-info">
              <span>Página {{ currentPage + 1 }} de {{ totalPages }}</span>
            </div>
          </div>
          
          <div class="table-container">
            <table class="audit-table">
              <thead>
                <tr>
                  <th>Data/Hora</th>
                  <th>Usuário</th>
                  <th>Ação</th>
                  <th>Mensagem</th>
                  <th>Nível</th>
                  <th>IP</th>
                </tr>
              </thead>
              <tbody>
                @for (log of auditLogs; track log) {
                  <tr>
                    <td>{{ log.executionTime | date:'dd/MM/yyyy HH:mm:ss' }}</td>
                    <td>{{ log.userName }}</td>
                    <td>{{ log.action }}</td>
                    <td>{{ log.message }}</td>
                    <td>
                      <span class="log-level log-level-{{ log.logLevel.toLowerCase() }}">
                        {{ log.logLevel }}
                      </span>
                    </td>
                    <td>{{ log.clientIpAddress }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
          
          @if (totalPages > 1) {
            <div class="pagination">
              <button 
                class="btn btn-secondary" 
                (click)="previousPage()" 
                [disabled]="currentPage === 0">
                Anterior
              </button>
              
              <span class="page-info">
                Página {{ currentPage + 1 }} de {{ totalPages }}
              </span>
              
              <button 
                class="btn btn-secondary" 
                (click)="nextPage()" 
                [disabled]="currentPage === totalPages - 1">
                Próximo
              </button>
            </div>
          }
        </div>
      }
      
      @if (auditLogs.length === 0 && !loading) {
        <div class="no-results">
          <p>Nenhum log de auditoria encontrado para os filtros selecionados.</p>
        </div>
      }
      
      @if (loading) {
        <div class="loading">
          <p>Carregando logs de auditoria...</p>
        </div>
      }
    </div>
  `,
  styles: `
    .container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
      font-family: Arial, sans-serif;
    }
    
    .header {
      text-align: center;
      margin-bottom: 30px;
    }
    
    .header h2 {
      color: #333;
      margin-bottom: 10px;
    }
    
    .header p {
      color: #666;
      font-size: 14px;
    }
    
    .filters {
      background: #f5f5f5;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    
    .form-row {
      display: flex;
      gap: 15px;
      flex-wrap: wrap;
    }
    
    .form-group {
      flex: 1;
      min-width: 200px;
    }
    
    .form-group label {
      display: block;
      margin-bottom: 5px;
      font-weight: bold;
      color: #333;
    }
    
    .form-control {
      width: 100%;
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
    }
    
    .form-actions {
      display: flex;
      gap: 10px;
      margin-top: 15px;
    }
    
    .btn {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;
    }
    
    .btn-primary {
      background: #007bff;
      color: white;
    }
    
    .btn-secondary {
      background: #6c757d;
      color: white;
    }
    
    .btn:hover {
      opacity: 0.8;
    }
    
    .btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    
    .results {
      margin-top: 20px;
    }
    
    .results-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 15px;
    }
    
    .results-header h3 {
      margin: 0;
      color: #333;
    }
    
    .pagination-info {
      color: #666;
      font-size: 14px;
    }
    
    .table-container {
      overflow-x: auto;
    }
    
    .audit-table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 10px;
    }
    
    .audit-table th,
    .audit-table td {
      border: 1px solid #ddd;
      padding: 12px;
      text-align: left;
    }
    
    .audit-table th {
      background-color: #f8f9fa;
      font-weight: bold;
      color: #333;
    }
    
    .audit-table tr:nth-child(even) {
      background-color: #f9f9f9;
    }
    
    .log-level {
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: bold;
    }
    
    .log-level-information {
      background-color: #d4edda;
      color: #155724;
    }
    
    .log-level-warning {
      background-color: #fff3cd;
      color: #856404;
    }
    
    .log-level-error {
      background-color: #f8d7da;
      color: #721c24;
    }
    
    .log-level-critical {
      background-color: #dc3545;
      color: white;
    }
    
    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 10px;
      margin-top: 20px;
    }
    
    .page-info {
      color: #666;
      font-size: 14px;
    }
    
    .no-results {
      text-align: center;
      padding: 40px;
      color: #666;
    }
    
    .loading {
      text-align: center;
      padding: 40px;
      color: #666;
    }
  `
})
export class AuditLogsComponent implements OnInit {
  auditLogs: any[] = [];
  loading = false;
  currentPage = 0;
  totalPages = 0;
  
  searchForm = this.fb.group({
    startDate: [''],
    endDate: [''],
    userName: [''],
    action: [''],
    logLevel: ['']
  });

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  onSearch(): void {
    if (this.searchForm.valid) {
      this.loading = true;
      
      setTimeout(() => {
        this.auditLogs = this.getMockAuditLogs();
        this.loading = false;
        this.updatePagination();
      }, 1000);
    }
  }

  onReset(): void {
    this.searchForm.reset();
    this.auditLogs = [];
    this.currentPage = 0;
    this.totalPages = 0;
  }

  previousPage(): void {
    if (this.currentPage > 0) {
      this.currentPage--;
      this.loadAuditLogs();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages - 1) {
      this.currentPage++;
      this.loadAuditLogs();
    }
  }

  private loadAuditLogs(): void {
    this.loading = true;
    
    setTimeout(() => {
      this.auditLogs = this.getMockAuditLogs();
      this.loading = false;
      this.updatePagination();
    }, 500);
  }

  private getMockAuditLogs(): any[] {
    const mockLogs = [
      {
        executionTime: new Date().toISOString(),
        userName: 'admin',
        action: 'Login',
        message: 'Usuário admin realizou login no sistema',
        logLevel: 'Information',
        clientIpAddress: '192.168.1.100'
      },
      {
        executionTime: new Date(Date.now() - 3600000).toISOString(),
        userName: 'user1',
        action: 'CreateRoom',
        message: 'Usuário criou uma nova sala de chat',
        logLevel: 'Information',
        clientIpAddress: '192.168.1.101'
      },
      {
        executionTime: new Date(Date.now() - 7200000).toISOString(),
        userName: 'user2',
        action: 'SendMessage',
        message: 'Usuário enviou mensagem na sala',
        logLevel: 'Information',
        clientIpAddress: '192.168.1.102'
      },
      {
        executionTime: new Date(Date.now() - 1800000).toISOString(),
        userName: 'user3',
        action: 'DeleteMessage',
        message: 'Usuário deletou mensagem',
        logLevel: 'Warning',
        clientIpAddress: '192.168.1.103'
      },
      {
        executionTime: new Date(Date.now() - 900000).toISOString(),
        userName: 'system',
        action: 'BackgroundJobFailed',
        message: 'Falha ao executar job em background',
        logLevel: 'Error',
        clientIpAddress: '127.0.0.1'
      }
    ];
    
    return mockLogs;
  }

  private updatePagination(): void {
    const pageSize = 10;
    this.totalPages = Math.ceil(this.auditLogs.length / pageSize);
    this.currentPage = Math.min(this.currentPage, this.totalPages - 1);
  }
}
