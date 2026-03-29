import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { AccountModule } from '@abp/ng.account';
import { IdentityModule } from '@abp/ng.identity';
import { TenantManagementModule } from '@abp/ng.tenant-management';
import { SettingManagementModule } from '@abp/ng.setting-management';

import { AppComponent } from './app.component';
import { appRoutes } from './app.routes';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    CoreModule.forRoot(),
    ThemeSharedModule.forRoot(),
    AccountModule.forRoot(),
    IdentityModule.forRoot(),
    TenantManagementModule.forRoot(),
    SettingManagementModule.forRoot(),
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
