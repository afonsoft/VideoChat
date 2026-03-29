import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app.routes';
import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { AccountModule } from '@abp/ng.account';
import { IdentityModule } from '@abp/ng.identity';
import { TenantManagementModule } from '@abp/ng.tenant-management';
import { SettingManagementModule } from '@abp/ng.setting-management';
import { ChatRoomComponent } from './components/chat-room/chat-room.component';

@NgModule({
  declarations: [
    ChatRoomComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule,
    CoreModule.forRoot(),
    ThemeSharedModule.forRoot(),
    AccountModule.forRoot(),
    IdentityModule.forLazy(),
    TenantManagementModule.forLazy(),
    SettingManagementModule.forLazy()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
