import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class SimpleConnectConfigService {
  constructor(private rest: RestService) {}

  get() {
    return this.rest.request<void, any>({
      method: 'GET',
      url: '/api/abp/application-configuration',
    }).pipe(map(response => response));
  }
}
